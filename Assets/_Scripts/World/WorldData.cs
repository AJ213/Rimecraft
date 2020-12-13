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
    public static string worldName = "Prototype";
    public static int seed;

    [System.NonSerialized]
    public static ConcurrentDictionary<int3, ChunkData> chunks = new ConcurrentDictionary<int3, ChunkData>();

    [System.NonSerialized] // We use this as a ConcurrentSet, the byte does nothing
    public static ConcurrentDictionary<ChunkData, byte> modifiedChunks = new ConcurrentDictionary<ChunkData, byte>();

    public static void AddToModifiedChunkList(ChunkData chunk)
    {
        if (!modifiedChunks.ContainsKey(chunk))
        {
            modifiedChunks.TryAdd(chunk, 1);
        }
    }

    public WorldData(string name, int theSeed)
    {
        worldName = name;
        seed = theSeed;
    }

    public ChunkData RequestChunk(int3 coord, bool create)
    {
        if (chunks.ContainsKey(coord))
        {
            return chunks[coord];
        }

        if (create)
        {
            LoadChunk(coord);
            return chunks[coord];
        }
        else
        {
            return null;
        }
    }

    public static void LoadChunk(int3 coord)
    {
        if (chunks.ContainsKey(coord))
        {
            return;
        }

        chunks.TryAdd(coord, new ChunkData(coord));
        ChunkData.Populate(chunks[coord]);
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