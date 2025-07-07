using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class World : MonoBehaviour
{
    public static readonly Vector3Int worldSize = new(2, 1, 2);

    private Chunk[,,] chunks = new Chunk[worldSize.x, worldSize.y, worldSize.z];

    public GameObject chunkPrefab;

    void Start()
    {
        BlockMaterialStore.LoadMaterials();

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
        block.Set(blockType);
        
        // Powered blocks
        int[] deltax = { 0, 0, -1, 1, 0, 0 };
        int[] deltay = { 0, 0, 0, 0, 1, -1 };
        int[] deltaz = { -1, 1, 0, 0, 0, 0 };
        HashSet<Chunk> rerender = new HashSet<Chunk>();
        

        if (blockType == Block.Type.PowerSource) {
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
                        checking.sources.Add(block.worldPos);
                        rerender.Add(chunk2);
                        to_check.Enqueue(checking.worldPos);
                        visited.Add(checking.worldPos);
                    }
                }
            }

        } else if (blockType == Block.Type.Wire) {
            // if any powered wire is adjacent, this is powered too
            for (int i = 0; i < 6; i++) {
                if (!GetBlock(new(worldPos.x + deltax[i], worldPos.y + deltay[i], worldPos.z + deltaz[i]), out var checking, out var chunk2)) continue;
                if (checking.type == Block.Type.Wire && checking.powered) {
                    block.powered = true; 
                    block.Set(block.type); // move later
                    foreach (Vector3Int src in checking.sources) {
                        block.sources.Add(src);
                    }
                    rerender.Add(chunk2);
                } else if (checking.type == Block.Type.PowerSource) {
                    block.powered = true;
                    block.Set(block.type); // move later
                    block.sources.Add(checking.worldPos);
                }
                // when it propagates it needs to pass on all its sources
            }
            // when i add a wire what if it only propagates to unpowered wires
            // breaking/adding power source might be more costly

            if (block.powered) { // THIS ISN'T RIGHT YET (revisits where a wire got powered from)
                foreach (Vector3Int src in block.sources) {
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
                                checking.sources.Add(src);
                                Debug.Log(checking.sources);
                                rerender.Add(chunk2);
                                to_check.Enqueue(checking.worldPos);
                                visited.Add(checking.worldPos);
                            }
                        }
                    }
                }
            }
        }

        chunk.GenerateMesh();
        foreach (Chunk chunkToRerender in rerender) {
            chunkToRerender.GenerateMesh();
        }
        
    }



    bool InBounds(Vector3Int chunkIndex)
    {
        return chunkIndex.x >= 0 && chunkIndex.y >= 0 && chunkIndex.z >= 0 &&
            chunkIndex.x < worldSize.x && chunkIndex.y < worldSize.y && chunkIndex.z < worldSize.z;
    }
}

public static class BlockMaterialStore
{
    private static readonly string[] materialNames =
    {
        "stone",
        "powersource",
        "wire",
        "poweredwire"
    };
    private static readonly Dictionary<Block.Type, string> defaultMaterialNames = new()
    {
        { Block.Type.Stone, "stone" },
        { Block.Type.PowerSource, "powersource" },
        { Block.Type.Wire, "wire" }
    };

    public static Dictionary<string, Material> materials = new();
    public static Dictionary<Block.Type, Material> defaultMaterials = new();

    public static void LoadMaterials()
    {
        foreach (var name in materialNames)
            materials[name] = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/" + name + ".mat", typeof(Material));

        foreach (var kvp in defaultMaterialNames)
            defaultMaterials[kvp.Key] = materials[kvp.Value];
    }
}