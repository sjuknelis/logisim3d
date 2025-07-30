using UnityEngine;
using System.Collections.Generic;

public class World : MonoBehaviour
{
    public static readonly Vector3Int worldSize = new(2, 1, 2);

    private Chunk[,,] chunks = new Chunk[worldSize.x, worldSize.y, worldSize.z];

    public GameObject chunkPrefab;

    void Start()
    {
        // Create chunks
        for (int x = 0; x < worldSize.x; x++)
            for (int y = 0; y < worldSize.y; y++)
                for (int z = 0; z < worldSize.z; z++)
                {
                    Vector3 chunkPos = new(x * Chunk.chunkSize.x, y * Chunk.chunkSize.y, z * Chunk.chunkSize.z);
                    var obj = Instantiate(chunkPrefab, chunkPos, Quaternion.identity);
                    var chunk = obj.GetComponent<Chunk>();
                    chunk.world = this;
                    chunks[x, y, z] = chunk;
                }
    }

    void Update()
    {
        
    }

    public bool GetChunk(Vector3Int worldPos, out Chunk chunk)
    {
        Vector3Int index = new(
            worldPos.x / Chunk.chunkSize.x,
            worldPos.y / Chunk.chunkSize.y,
            worldPos.z / Chunk.chunkSize.z
            );

        if (InBounds(index))
        {
            chunk = chunks[index.x, index.y, index.z];
            return true;
        }
        else
        {
            chunk = null;
            return false;
        }
    }

    public bool GetBlock(Vector3Int worldPos, out Block block, out Chunk chunk)
    {
        if (!GetChunk(worldPos, out chunk))
        {
            block = null;
            chunk = null;
            return false;
        }

        Vector3Int chunkPos = new(
            worldPos.x % Chunk.chunkSize.x,
            worldPos.y % Chunk.chunkSize.y,
            worldPos.z % Chunk.chunkSize.z
            );
        block = chunk.blocks[chunkPos.x, chunkPos.y, chunkPos.z];
        return true;
    }

    public bool GetBlock(Vector3Int worldPos, out Block block)
    {
        return GetBlock(worldPos, out block, out _);
    }

    public void PlaceBlock(Vector3Int worldPos, Block.Type blockType)
    {
        if (blockType != Block.Type.Air)
        {
            // Check for overlap with player
            Bounds bounds = new(worldPos + Vector3.one * 0.5f, Vector3.one);
            Collider[] hits = Physics.OverlapBox(bounds.center, Vector3.one * 0.5f);
            foreach (Collider c in hits)
            {
                if (c.CompareTag("Player")) return;
            }
        }

        if (!GetBlock(worldPos, out var block, out var chunk)) return;
        Block.Type prevType = block.type;
        block.Set(blockType);

        int[] deltax = { 0, 0, -1, 1, 0, 0 };
        int[] deltay = { 0, 0, 0, 0, 1, -1 };
        int[] deltaz = { -1, 1, 0, 0, 0, 0 };
        HashSet<Chunk> rerender = new HashSet<Chunk>();
        rerender.Add(chunk);

        // case 1. removing a power source
        if (blockType == Block.Type.Air && prevType == Block.Type.PowerSource) { 
            HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
            Queue<Vector3Int> to_check = new Queue<Vector3Int>();
            to_check.Enqueue(block.worldPos);
            visited.Add(block.worldPos);
            block.powered = false; // current block is now unpowered and visited

            while (to_check.Count != 0) {
                Vector3Int top = to_check.Dequeue();

                for (int i = 0; i < 6; i++) { // check all neighbors
                    if (!GetBlock(new(top.x+deltax[i], top.y+deltay[i], top.z+deltaz[i]), out var checking, out var chunk2)) continue;
                    if (visited.Contains(checking.worldPos)) continue; // only visit each one once
                    
                    if (checking.type == Block.Type.Wire) { // if it's a wire, remove block as a source
                        checking.sources.Remove(block.worldPos);
                        if (checking.sources.Count == 0) {
                            checking.powered = false; 
                            checking.Set(checking.type); // move later
                        }
                        rerender.Add(chunk2);
                        to_check.Enqueue(checking.worldPos); // visit neighbors of wire to see if any are wires and need source removed
                        visited.Add(checking.worldPos);
                    }
                }
            }

        // case 2. removing a wire (this is the demon it's here officer)
        } else if (blockType == Block.Type.Air && prevType == Block.Type.Wire) {
            if (block.powered) {
                block.powered = false;
                HashSet<Vector3Int> global_visited = new HashSet<Vector3Int>();

                foreach (var (src, _) in block.sources) {
                    Dictionary<Vector3Int, int> visited = new Dictionary<Vector3Int, int>();
                    Queue<Vector3Int> to_check = new Queue<Vector3Int>();
                    to_check.Enqueue(block.worldPos);
                    visited.Add(block.worldPos, block.sources[src].Count);

                    while (to_check.Count != 0) {
                        Vector3Int top = to_check.Dequeue();

                        for (int i = 0; i < 6; i++) {
                            if (!GetBlock(new(top.x+deltax[i], top.y+deltay[i], top.z+deltaz[i]), out var checking, out var chunk2)) continue;
                            if (visited.ContainsKey(checking.worldPos) && visited[checking.worldPos] == 0) continue;

                            if (checking.type == Block.Type.Wire && checking.sources.ContainsKey(src) && checking.sources[src].Contains(top)) {
                                checking.sources[src].Remove(top);
                                if (checking.sources[src].Count == 0) checking.sources.Remove(src);
                                
                                // unpower bc it MIGHT conceivably become unpowered
                                // but if i set up that case and then it has an incorrect extra src listed... 
                                // the extra src still needs to be removed (which is why visited is a dictionary)
                                checking.powered = false;
                                rerender.Add(chunk2);
                                to_check.Enqueue(checking.worldPos);
                                if (visited.ContainsKey(checking.worldPos)) visited[checking.worldPos] = checking.sources.Count;
                                else visited.Add(checking.worldPos, checking.sources.Count);
                                global_visited.Add(checking.worldPos);
                            }
                        }
                    }
                }

                foreach (var (src, _) in block.sources) {
                    HashSet<Vector3Int> visited2 = new HashSet<Vector3Int>();
                    Queue<Vector3Int> to_check2 = new Queue<Vector3Int>();
                    to_check2.Enqueue(src);
                    visited2.Add(src);
                    
                    while (to_check2.Count != 0) {
                        Vector3Int top = to_check2.Dequeue();

                        for (int i = 0; i < 6; i++) {
                            if (!GetBlock(new(top.x+deltax[i], top.y+deltay[i], top.z+deltaz[i]), out var checking, out var chunk2)) continue;
                            if (visited2.Contains(checking.worldPos)) continue;
                            

                            if (checking.type == Block.Type.Wire && checking.worldPos != block.worldPos) {
                                checking.powered = true; 
                                if (checking.sources.ContainsKey(src)) checking.sources[src].Add(top);
                                else checking.sources.Add(src, new HashSet<Vector3Int>{top});
                                rerender.Add(chunk2);
                                to_check2.Enqueue(checking.worldPos);
                                visited2.Add(checking.worldPos);
                                global_visited.Add(checking.worldPos);
                            }
                        }
                    }
                }
                // Set all visited blocks
                foreach (var block_visited in global_visited) {
                    if (GetBlock(block_visited, out var needtoset, out _)) {
                        needtoset.Set(needtoset.type);
                    }
                }
            }
            block.sources.Clear();

        // case 3. adding a power source
        } else if (blockType == Block.Type.PowerSource) {
            HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
            Queue<Vector3Int> to_check = new Queue<Vector3Int>();
            to_check.Enqueue(block.worldPos);
            visited.Add(block.worldPos);

            while (to_check.Count != 0) {
                Vector3Int top = to_check.Dequeue();

                for (int i = 0; i < 6; i++) {
                    if (!GetBlock(new(top.x+deltax[i], top.y+deltay[i], top.z+deltaz[i]), out var checking, out var chunk2)) continue;
                    if (visited.Contains(checking.worldPos)) continue;

                    if (checking.type == Block.Type.Wire) {
                        checking.powered = true; 
                        checking.Set(checking.type); // move later
                        if (checking.sources.ContainsKey(block.worldPos)) checking.sources[block.worldPos].Add(top);
                        else checking.sources.Add(block.worldPos, new HashSet<Vector3Int>{top});
                        rerender.Add(chunk2);
                        to_check.Enqueue(checking.worldPos);
                        visited.Add(checking.worldPos);
                    }
                }
            }

        // case 4. adding a wire
        } else if (blockType == Block.Type.Wire) {
            // if any powered wire is adjacent, this is powered too
            // if i have received power from an adjacent block, i don't visit it again for that source
            // so when i go to an adjacent block later, i check if it is listed as a power source already for my current block (no bidirections)
            for (int i = 0; i < 6; i++) {
                if (!GetBlock(new(worldPos.x + deltax[i], worldPos.y + deltay[i], worldPos.z + deltaz[i]), out var checking, out var chunk2)) continue;
                if (checking.type == Block.Type.Wire && checking.powered) {
                    block.powered = true; 
                    block.Set(block.type); // move later
                    foreach (var (src, _) in checking.sources) {
                        if (block.sources.ContainsKey(src)) block.sources[src].Add(checking.worldPos);
                        else block.sources.Add(src, new HashSet<Vector3Int>{checking.worldPos});    
                    }
                    rerender.Add(chunk2);
                } else if (checking.type == Block.Type.PowerSource) {
                    block.powered = true;
                    block.Set(block.type); // move later
                    if (block.sources.ContainsKey(checking.worldPos)) 
                        block.sources[checking.worldPos].Add(checking.worldPos);
                    else block.sources.Add(checking.worldPos, new HashSet<Vector3Int>{checking.worldPos});
                }
            }

            if (block.powered) { 
                foreach (var (src, _) in block.sources) {
                    HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
                    Queue<Vector3Int> to_check = new Queue<Vector3Int>();
                    to_check.Enqueue(block.worldPos);
                    visited.Add(block.worldPos);

                    while (to_check.Count != 0) {
                        Vector3Int top = to_check.Dequeue();

                        for (int i = 0; i < 6; i++) {
                            if (!GetBlock(new(top.x+deltax[i], top.y+deltay[i], top.z+deltaz[i]), out var checking, out var chunk2)) continue;
                            if (visited.Contains(checking.worldPos)) continue;
                            if (block.sources[src].Contains(checking.worldPos)) continue;

                            if (checking.type == Block.Type.Wire) {
                                checking.powered = true;
                                checking.Set(checking.type); // move later
                                if (checking.sources.ContainsKey(src)) checking.sources[src].Add(top);
                                else checking.sources.Add(src, new HashSet<Vector3Int>{top});
                                rerender.Add(chunk2);
                                to_check.Enqueue(checking.worldPos);
                                visited.Add(checking.worldPos);
                            }
                        }
                    }
                }
            }
        }

        // possible optimization: when i add a wire what if it only propagates to unpowered wires
        // breaking/adding power source might be more costly
        foreach (Chunk chunkToRerender in rerender) chunkToRerender.GenerateMesh();
    }

    bool InBounds(Vector3Int chunkIndex)
    {
        return chunkIndex.x >= 0 && chunkIndex.y >= 0 && chunkIndex.z >= 0 &&
            chunkIndex.x < worldSize.x && chunkIndex.y < worldSize.y && chunkIndex.z < worldSize.z;
    }
}