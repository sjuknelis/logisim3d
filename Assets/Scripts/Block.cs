
using System.Collections.Generic;
using UnityEngine;

public class Block
{
	public enum Type { Air, Stone, PowerSource, Wire }

	public Chunk chunk;

	public Vector3Int worldPos;
	public Type type;
	public Vector2Int[] textureOffset;

	static readonly Dictionary<Type, Vector2Int[]> defaultTextureOffsets = new()
	{
		{ Type.Stone, new Vector2Int[] { new(0, 0), new(1, 0), new(2, 0), new(3, 0), new(4, 0), new(5, 0) } },
		{ Type.PowerSource, new Vector2Int[] { new(0, 1), new(0, 1), new(0, 1), new(0, 1), new(0, 1), new(0, 1) } },
		{ Type.Wire, new Vector2Int[] { new(1, 1), new(1, 1), new(1, 1), new(1, 1), new(1, 1), new(1, 1) } }
    };

	public Block(Chunk chunk, Vector3Int worldPos, Type type)
	{
		this.chunk = chunk;
		this.worldPos = worldPos;
		Set(type);
	}

	public void Set(Type type)
	{
		this.type = type;

		if (type != Type.Air)
			textureOffset = defaultTextureOffsets[type];
	}
}