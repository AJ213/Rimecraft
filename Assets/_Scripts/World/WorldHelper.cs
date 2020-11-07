using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class WorldHelper
{
    public static ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / Constants.ChunkSizeX);
        int y = Mathf.FloorToInt(pos.y / Constants.ChunkSizeY);
        int z = Mathf.FloorToInt(pos.z / Constants.ChunkSizeZ);
        return new ChunkCoord(x, y, z);
    }

    public static Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / Constants.ChunkSizeX);
        int y = Mathf.FloorToInt(pos.y / Constants.ChunkSizeY);
        int z = Mathf.FloorToInt(pos.z / Constants.ChunkSizeZ);
        try
        {
            return RimecraftWorld.Instance.chunks[new int3(x, y, z)];
        }
        catch (System.Exception e)
        {
            Debug.Log(pos.x + ", " + pos.y + ", " + pos.z + "| due to " + x + ", " + y + ", " + z);
            throw e;
        }
    }

    public static bool IsInRange(int value, int length)
    {
        return (value >= 0 && value < length);
    }

    public static bool IsInRange(int3 value, int length)
    {
        return (IsInRange(value.x, length) && IsInRange(value.y, length) && IsInRange(value.z, length));
    }
}