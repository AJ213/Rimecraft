using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

[System.Serializable]
public class WorldData
{
    public string worldName = "Prototype";
    public int seed;

    [System.NonSerialized]
    public Dictionary<int3, ChunkData> chunks = new Dictionary<int3, ChunkData>();

    [System.NonSerialized]
    public List<ChunkData> modifiedChunks = new List<ChunkData>();

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

    public void LoadChunk(int3 coord)
    {
        if (chunks.ContainsKey(coord))
        {
            return;
        }

        ChunkData chunk = SaveSystem.LoadChunk(worldName, coord);
        if (chunk != null)
        {
            chunks.Add(coord, chunk);
            return;
        }

        chunks.Add(coord, new ChunkData(coord));
        chunks[coord].Populate();
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