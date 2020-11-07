using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct ChunkCoord
{
    public int x;
    public int y;
    public int z;

    public ChunkCoord(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public ChunkCoord(int3 pos)
    {
        x = (int)((float)pos.x / VoxelData.ChunkWidth);
        y = (int)((float)pos.y / VoxelData.ChunkWidth);
        z = (int)((float)pos.z / VoxelData.ChunkWidth);
    }

    public int3 ToInt3()
    {
        return new int3(x, y, z);
    }

    public override bool Equals(object obj)
    {
        if (!(obj is ChunkCoord))
        {
            return false;
        }

        ChunkCoord otherCoord = (ChunkCoord)obj;

        return (x == otherCoord.x && y == otherCoord.y && z == otherCoord.z);
    }

    public override int GetHashCode()
    {
        int hashCode = 373119288;
        hashCode = (hashCode * -1521134295) + x.GetHashCode();
        hashCode = (hashCode * -1521134295) + y.GetHashCode();
        hashCode = (hashCode * -1521134295) + z.GetHashCode();
        return hashCode;
    }

    public static bool operator ==(ChunkCoord left, ChunkCoord right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ChunkCoord left, ChunkCoord right)
    {
        return !(left == right);
    }
}