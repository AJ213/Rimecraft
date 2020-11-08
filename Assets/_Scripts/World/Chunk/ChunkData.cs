using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[System.Serializable]
public class ChunkData
{
    private int3 coord;

    public int3 Coord
    {
        get { return coord; }
        set
        {
            coord.x = value.x;
            coord.y = value.y;
            coord.z = value.z;
        }
    }

    public ChunkData(int3 pos)
    {
        Coord = pos;
    }

    public ChunkData(int x, int y, int z)
    {
        coord.x = x;
        coord.y = y;
        coord.z = z;
    }

    [System.NonSerialized] public Chunk chunk;

    [HideInInspector]
    public VoxelState[,,] map = new VoxelState[Constants.ChunkSizeX, Constants.ChunkSizeY, Constants.ChunkSizeZ];

    public static void Populate(ChunkData chunk)
    {
        for (int y = 0; y < Constants.ChunkSizeX; y++)
        {
            for (int x = 0; x < Constants.ChunkSizeY; x++)
            {
                for (int z = 0; z < Constants.ChunkSizeZ; z++)
                {
                    int3 localPosition = new int3(x, y, z);
                    chunk.map[x, y, z] = new VoxelState(RimecraftWorld.SamplePosition(WorldHelper.GetVoxelGlobalPositionFromChunk(localPosition, chunk.Coord), RimecraftWorld.Instance.biomes), chunk, localPosition);
                }
            }
        }

        if (!WorldData.modifiedChunks.Contains(chunk))
        {
            WorldData.modifiedChunks.Add(chunk);
        }
    }

    public void ModifyVoxel(int3 localPosition, ushort id)
    {
        if (map[localPosition.x, localPosition.y, localPosition.z].id == id)
        {
            return;
        }

        VoxelState voxel = map[localPosition.x, localPosition.y, localPosition.z];

        voxel.id = id;
        RimecraftWorld.Instance.worldData.AddToModifiedChunkList(this);
        if (chunk != null)
        {
            RimecraftWorld.Instance.AddChunkToUpdate(chunk, true);
        }
    }

    public VoxelState VoxelFromPosition(int3 localPosition)
    {
        return map[localPosition.x, localPosition.y, localPosition.z];
    }
}