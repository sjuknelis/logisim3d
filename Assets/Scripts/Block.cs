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

	public Direction Project(Direction dir)
	{
		if (dir == Direction.Up)
			return upSign == 1 ? Direction.Up : Direction.Down;
		else if (dir == Direction.Down)
            return upSign == 1 ? Direction.Down : Direction.Up;
		else
			return DirectionUtils.FromFaceIndex((dir.GetFaceIndex() + forwardIndex) % 4);
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