using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public static readonly Vector3Int chunkSize = new(16, 64, 16);

    public World world;
    public Block[,,] blocks = new Block[chunkSize.x, chunkSize.y, chunkSize.z];

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider collider;

    void Start()
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        collider = gameObject.AddComponent<MeshCollider>();

        GenerateBlocks();
        GenerateMesh();
    }

    void GenerateBlocks()
    {
        var chunkOrigin = Vector3Int.FloorToInt(transform.position);

        // Half air half stone chunk
        for (int y = 0; y < chunkSize.y; y++)
        {
            var type = (y > chunkSize.y / 2) ? Block.Type.Air : Block.Type.Stone;
            for (int x = 0; x < chunkSize.x; x++)
                for (int z = 0; z < chunkSize.z; z++) {
                    blocks[x, y, z] = new(this, chunkOrigin + new Vector3Int(x, y, z), type);
                    
                }
        }
    }

    public void GenerateMesh()
    {
        Dictionary<Material, MeshData> meshDataMap = new();

        for (int x = 0; x < chunkSize.x; x++)
            for (int y = 0; y < chunkSize.y; y++)
                for (int z = 0; z < chunkSize.z; z++)
                {
                    var block = blocks[x, y, z];
                    if (block.type == Block.Type.Air) continue;

                    var material = block.material;
                    if (!meshDataMap.ContainsKey(material))
                        meshDataMap[material] = new();

                    Vector3 pos = new(x, y, z);

                    Vector3Int[] directions = {
                        new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
                        new Vector3Int(0, 0, 1), new Vector3Int(0, 0, -1),
                        new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0)
                    };

                    foreach (var dir in directions)
                    {
                        int nx = x + dir.x;
                        int ny = y + dir.y;
                        int nz = z + dir.z;

                        // Render face corresponding to this side of the block only if it is at the edge of the chunk or next to air (visible face)
                        if (nx < 0 || ny < 0 || nz < 0 || nx >= chunkSize.x || ny >= chunkSize.y || nz >= chunkSize.z || blocks[nx, ny, nz].type == Block.Type.Air)
                        {
                            VoxelFaceGenerator.AddFace(meshDataMap[material], pos, dir);
                        }
                    }
                }

        List<CombineInstance> combineInstances = new();
        List<Material> materials = new();

        foreach (var kvp in meshDataMap)
        {
            if (kvp.Value.vertices.Count == 0) continue;

            Mesh mesh = new()
            {
                vertices = kvp.Value.vertices.ToArray(),
                triangles = kvp.Value.triangles.ToArray()
            };
            mesh.RecalculateNormals();

            CombineInstance ci = new()
            {
                mesh = mesh,
                transform = Matrix4x4.identity
            };
            combineInstances.Add(ci);

            materials.Add(kvp.Key);
        }

        Mesh finalMesh = new();
        finalMesh.CombineMeshes(combineInstances.ToArray(), false, false);

        meshFilter.mesh = null;
        meshFilter.mesh = finalMesh;
        meshRenderer.materials = materials.ToArray();

        collider.sharedMesh = null;
        collider.sharedMesh = finalMesh;
    }
}

public static class VoxelFaceGenerator
{
    static readonly Vector3[,] faceVertices = new Vector3[6, 4]
    {
        { new Vector3(0,1,0), new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(1,1,0) },
        { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(0,0,1) },
        { new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(0,1,1) },
        { new Vector3(1,0,0), new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(1,1,0) },
        { new Vector3(1,0,1), new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(1,1,1) },
        { new Vector3(0,0,0), new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(0,1,0) }
    };

    public static void AddFace(MeshData meshData, Vector3 pos, Vector3Int normal)
    {
        int index = meshData.vertices.Count;
        int dir = GetFaceIndex(normal);

        for (int i = 0; i < 4; i++)
        {
            meshData.vertices.Add(pos + faceVertices[dir, i]);
        }

        meshData.triangles.Add(index);
        meshData.triangles.Add(index + 1);
        meshData.triangles.Add(index + 2);
        meshData.triangles.Add(index);
        meshData.triangles.Add(index + 2);
        meshData.triangles.Add(index + 3);
    }

    private static int GetFaceIndex(Vector3Int normal)
    {
        if (normal == Vector3Int.up) return 0;
        if (normal == Vector3Int.down) return 1;
        if (normal == Vector3Int.forward) return 2;
        if (normal == Vector3Int.back) return 3;
        if (normal == Vector3Int.right) return 4;
        if (normal == Vector3Int.left) return 5;
        return 0;
    }
}

public class MeshData
{
    public List<Vector3> vertices = new();
    public List<int> triangles = new();
}