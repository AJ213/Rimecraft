using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkCoord
{
    public int x;
    public int y;
    public int z;

    public ChunkCoord()
    {
        x = 0;
        y = 0;
        z = 0;
    }

    public ChunkCoord(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public ChunkCoord(Vector3Int pos)
    {
        x = pos.x / VoxelData.ChunkWidth;
        y = pos.y / VoxelData.ChunkWidth;
        z = pos.z / VoxelData.ChunkWidth;
    }

    public Vector3Int ToVector3Int()
    {
        return new Vector3Int(x, y, z);
    }

    public bool Equals(ChunkCoord other)
    {
        if (other == null)
        {
            return false;
        }

        if (other.x != x || other.y != y || other.z != z)
        {
            return false;
        }

        return true;
    }
}