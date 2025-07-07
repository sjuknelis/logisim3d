
using System.Collections.Generic;
using UnityEngine;

public class Block
{
	public enum Type { Air, Stone, PowerSource, Wire }

	public Chunk chunk;

	public Vector3Int worldPos;
	public Type type;
	public Material material;
	public bool powered;
	public HashSet<Vector3Int> sources = new HashSet<Vector3Int>();

	public Block(Chunk chunk, Vector3Int worldPos, Type type)
	{
		this.chunk = chunk;
		this.worldPos = worldPos;
		this.powered = false;
		Set(type);
	}

	public void Set(Type type)
	{
		Debug.Log("I'm here");
		this.type = type;
		if (type == Type.Wire && powered) {
			Debug.Log("I am changing the material");
			material = BlockMaterialStore.materials["poweredwire"];
		}
		else if (type != Type.Air)
			material = BlockMaterialStore.defaultMaterials[type];
	}
}