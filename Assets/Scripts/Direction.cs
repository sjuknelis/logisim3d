using System;
using UnityEngine;

public enum Direction
{
    Forward,
    Right,
    Back,
    Left,
    Up,
    Down
}

public static class DirectionUtils
{
    public static readonly Direction[] allDirs = new Direction[]
    {
        Direction.Forward,
        Direction.Right,
        Direction.Back,
        Direction.Left,
        Direction.Up,
        Direction.Down
    };

    public static int GetFaceIndex(this Direction dir)
    {
        return Array.IndexOf(allDirs, dir);
    }

    public static Direction FromFaceIndex(int index)
    {
        return allDirs[index];
    }

    public static Vector3Int GetNormal(this Direction dir)
    {
        var vecs = new Vector3Int[]
        {
            Vector3Int.forward,
            Vector3Int.right,
            Vector3Int.back,
            Vector3Int.left,
            Vector3Int.up,
            Vector3Int.down
        };
        return vecs[dir.GetFaceIndex()];
    }
}