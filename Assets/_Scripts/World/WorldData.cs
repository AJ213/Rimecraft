using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.UIElements;
using System.Collections.Concurrent;

[System.Serializable]
public class WorldData
{
    public string worldName = "Prototype";
    public int seed;

    [System.NonSerialized]
    public ConcurrentDictionary<int3, ChunkData> chunks = new ConcurrentDictionary<int3, ChunkData>();

    [System.NonSerialized]
    public static List<ChunkData> modifiedChunks = new List<ChunkData>();

    public void AddToModifiedChunkList(ChunkData chunk)
    {
        if (!modifiedChunks.Contains(chunk))
        {
            modifiedChunks.Add(chunk);
        }
    }

    public WorldData(string worldName, int seed)
    {
        this.worldName = worldName;
        this.seed = seed;
    }

    public WorldData(WorldData worldData)
    {
        worldName = worldData.worldName;
        seed = worldData.seed;
    }

    public ChunkData RequestChunk(int3 coord, bool create)
    {
        ChunkData c;

        if (chunks.ContainsKey(coord))
        {
            c = chunks[coord];
        }
        else if (!create)
        {
            c = null;
        }
        else
        {
            LoadChunk(coord);
            c = chunks[coord];
        }

        return c;
    }

    public static void LoadChunk(int3 coord)
    {
        if (RimecraftWorld.Instance.worldData.chunks.ContainsKey(coord))
        {
            return;
        }

        ChunkData chunk = SaveSystem.LoadChunk(RimecraftWorld.Instance.worldData.worldName, coord);
        if (chunk != null)
        {
            RimecraftWorld.Instance.worldData.chunks.TryAdd(coord, chunk);
            return;
        }

        RimecraftWorld.Instance.worldData.chunks.TryAdd(coord, new ChunkData(coord));
        ChunkData.Populate(RimecraftWorld.Instance.worldData.chunks[coord]);
    }

    public void SetVoxel(int3 globalPosition, ushort value)
    {
        ChunkData chunk = RequestChunk(WorldHelper.GetChunkCoordFromPosition(globalPosition), true);

        int3 voxel = WorldHelper.GetVoxelLocalPositionInChunk(globalPosition);

        chunk.ModifyVoxel(voxel, value);
    }

    public VoxelState GetVoxel(int3 globalPosition)
    {
        ChunkData chunk = RequestChunk(WorldHelper.GetChunkCoordFromPosition(globalPosition), false);
        if (chunk == null)
        {
            return null;
        }

        int3 voxel = WorldHelper.GetVoxelLocalPositionInChunk(globalPosition);
        try
        {
            return chunk.map[voxel.x, voxel.y, voxel.z];
        }
        catch (System.Exception e)
        {
            Debug.Log(globalPosition.x + ", " + globalPosition.y + ", " + globalPosition.z);
            Debug.Log(voxel.x + ", " + voxel.y + ", " + voxel.z);
            throw e;
        }
    }
}

internal struct PopulateJob : IJob
{
    public int3 coord;

    public void Execute()
    {
        ChunkData.Populate(RimecraftWorld.Instance.worldData.chunks[coord]);
    }
}