
using System.Collections.Generic;
using UnityEngine;

public class Block
{
	public enum Type { Air, Stone, PowerSource, Wire }

	public Chunk chunk;

	public Vector3Int worldPos;
	public Type type;
	public Material material;

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
			material = BlockMaterialStore.defaultMaterials[type];
	}
}