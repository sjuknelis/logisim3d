using System.Collections.Generic;
using UnityEngine;

public static class BlockProps
{
    public static readonly Dictionary<Block.Type, string> names = new()
    {
        { Block.Type.Stone, "stone" },
        { Block.Type.PowerSource, "powersource" },
        { Block.Type.Wire, "wire" }
    };

    public static readonly Dictionary<string, Vector2Int[]> textureOffsets = new()
    {
        { "stone", new Vector2Int[] { new(0, 0), new(1, 0), new(2, 0), new(3, 0), new(4, 0), new(5, 0) } },
        { "powersource", BlockSideTextureGen.OneSide(new(0, 1)) },
        { "wire", BlockSideTextureGen.OneSide(new(1, 1)) },
        { "poweredwire", BlockSideTextureGen.OneSide(new(2, 1)) }
    };
}

static class BlockSideTextureGen
{
    public static Vector2Int[] OneSide(Vector2Int side)
    {
        return new Vector2Int[] { side, side, side, side, side, side };
    }
}