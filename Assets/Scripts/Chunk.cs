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

    public Material atlasMaterial;
    public int atlasGridSize, atlasResolution;

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
            var type = (y >= 16) ? Block.Type.Air : Block.Type.Stone;
            for (int x = 0; x < chunkSize.x; x++)
                for (int z = 0; z < chunkSize.z; z++)
                    blocks[x, y, z] = new(this, chunkOrigin + new Vector3Int(x, y, z), type);
        }
    }

    public void GenerateMesh()
    {
        MeshData meshData = new();

        for (int x = 0; x < chunkSize.x; x++)
            for (int y = 0; y < chunkSize.y; y++)
                for (int z = 0; z < chunkSize.z; z++)
                {
                    var block = blocks[x, y, z];
                    if (block.type == Block.Type.Air) continue;

                    Vector3Int chunkPos = new(x, y, z);

                    foreach (var dir in DirectionUtils.allDirs)
                    {
                        var nextPos = chunkPos + dir.GetNormal();

                        // Render face corresponding to this side of the block only if it is at the edge of the chunk or nextPos to air (visible face)
                        if (
                            nextPos.x < 0 || nextPos.y < 0 || nextPos.z < 0 ||
                            nextPos.x >= chunkSize.x || nextPos.y >= chunkSize.y || nextPos.z >= chunkSize.z ||
                            blocks[nextPos.x, nextPos.y, nextPos.z].type == Block.Type.Air
                            )
                        {
                            VoxelFaceGenerator.AddFace(meshData, chunkPos, block, dir, atlasGridSize, atlasResolution);
                        }
                    }
                }

        Mesh mesh = new()
        {
            vertices = meshData.vertices.ToArray(),
            triangles = meshData.triangles.ToArray(),
            uv = meshData.uvs.ToArray()
        };

        meshFilter.mesh = null;
        meshFilter.mesh = mesh;

        meshRenderer.material = atlasMaterial;

        collider.sharedMesh = null;
        collider.sharedMesh = mesh;
    }
}

static class VoxelFaceGenerator
{
    static readonly Vector3[,] faceVertices = new Vector3[,]
    {
        { new(0,0,1), new(1,0,1), new(1,1,1), new(0,1,1) }, // forward
        { new(1,0,1), new(1,0,0), new(1,1,0), new(1,1,1) }, // right
        { new(1,0,0), new(0,0,0), new(0,1,0), new(1,1,0) }, // back
        { new(0,0,0), new(0,0,1), new(0,1,1), new(0,1,0) }, // left
        { new(0,1,0), new(0,1,1), new(1,1,1), new(1,1,0) }, // up
        { new(0,0,0), new(1,0,0), new(1,0,1), new(0,0,1) }  // down
    };

    public static void AddFace(MeshData meshData, Vector3Int chunkPos, Block block, Direction dir, int atlasGridSize, int atlasResolution)
    {
        int worldFaceIndex = dir.GetFaceIndex();
        int blockFaceIndex = block.orientation.Project(dir).GetFaceIndex();

        int index = meshData.vertices.Count;

        for (int i = 0; i < 4; i++)
            meshData.vertices.Add(chunkPos + faceVertices[worldFaceIndex, i]);

        meshData.triangles.Add(index);
        meshData.triangles.Add(index + 1);
        meshData.triangles.Add(index + 2);
        meshData.triangles.Add(index);
        meshData.triangles.Add(index + 2);
        meshData.triangles.Add(index + 3);

        // Determine how to rotate top and bottom faces according to rotation
        // Idk why these are the offsets
        int rotations = 0;
        if (dir == Direction.Up)
            rotations = (block.orientation.forwardIndex + 1) % 4;
        else if (dir == Direction.Down)
            rotations = 4 - block.orientation.forwardIndex;

        var faceUVs = GetUVs(block.textureOffsets[blockFaceIndex], rotations, atlasGridSize, atlasResolution);
        meshData.uvs.AddRange(faceUVs);
    }

    static Vector2[] GetUVs(Vector2Int atlasOffset, int rotations, int atlasGridSize, int atlasResolution)
    {
        float padding = 2f / atlasResolution;

        float tileSize = 1f / atlasGridSize;
        float paddedTileSize = tileSize - padding * 2;

        float xMin = atlasOffset.x * tileSize + padding;
        float yMin = atlasOffset.y * tileSize + padding;

        var standardUVs = new Vector2[]
        {
            new(xMin + paddedTileSize, yMin),
            new(xMin, yMin),
            new(xMin, yMin + paddedTileSize),
            new(xMin + paddedTileSize, yMin + paddedTileSize)
        };

        var rotatedUVs = new Vector2[4];
        for (int i = 0; i < 4; i++)
            rotatedUVs[i] = standardUVs[(i + rotations) % 4];

        return rotatedUVs;
    }
}

class MeshData
{
    public List<Vector3> vertices = new();
    public List<int> triangles = new();
    public List<Vector2> uvs = new();
}