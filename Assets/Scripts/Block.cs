using System;
using UnityEngine;

public class Block
{
	public enum Type { Air, Stone, PowerSource, Wire }

	public Chunk chunk;

	public Vector3Int worldPos;
	public Type type;
	public Vector2Int[] textureOffsets;
	public BlockOrientation orientation = new();

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
			textureOffsets = BlockProps.textureOffsets[BlockProps.names[type]];

		orientation.Reset();
	}
}

public class BlockOrientation
{
	public int upSign, forwardIndex;

	public BlockOrientation()
	{
		Reset();
	}

	public void Reset()
	{
		upSign = 1;
		forwardIndex = 0;
	}

	public int ProjectFaceIndex(int index)
	{
		if (index < 4)
		{
			return (index + forwardIndex) % 4;
		}
		else if (upSign == 1)
		{
			return index;
		}
		else
		{
			return 9 - index;
		}
	}

	public void Rotate()
	{
		forwardIndex = (forwardIndex + 1) % 4;
	}

	public void Flip()
	{
		upSign = -upSign;
	}
}