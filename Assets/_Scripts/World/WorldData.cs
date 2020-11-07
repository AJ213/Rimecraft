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

    public void SetVoxel(int3 pos, ushort value)
    {
        if (!WorldHelper.IsInRange(pos, Constants.WorldSizeInVoxels))
        {
            return;
        }

        int x = Mathf.FloorToInt((float)pos.x / Constants.ChunkSizeX);
        int y = Mathf.FloorToInt((float)pos.y / Constants.ChunkSizeY);
        int z = Mathf.FloorToInt((float)pos.z / Constants.ChunkSizeZ);

        x *= Constants.ChunkSizeX;
        y *= Constants.ChunkSizeY;
        z *= Constants.ChunkSizeZ;

        ChunkData chunk = RequestChunk(new int3(x, y, z), true);

        int3 voxel = new int3((pos.x - x), (pos.y - y), (pos.z - z));

        chunk.ModifyVoxel(voxel, value);
    }

    public VoxelState GetVoxel(int3 pos)
    {
        if (!WorldHelper.IsInRange(pos, Constants.WorldSizeInVoxels))
        {
            return null;
        }

        int x = pos.x / Constants.ChunkSizeX;
        int y = pos.y / Constants.ChunkSizeY;
        int z = pos.z / Constants.ChunkSizeZ;

        x *= Constants.ChunkSizeX;
        y *= Constants.ChunkSizeY;
        z *= Constants.ChunkSizeZ;

        ChunkData chunk = RequestChunk(new int3(x, y, z), false);

        if (chunk == null)
        {
            return null;
        }

        int3 voxel = new int3((pos.x - x), (pos.y - y), (pos.z - z));
        try
        {
            return chunk.map[voxel.x, voxel.y, voxel.z];
        }
        catch (System.Exception e)
        {
            Debug.Log(pos.x + ", " + pos.y + ", " + pos.z);
            Debug.Log(x + ", " + y + ", " + z);
            Debug.Log(voxel.x + ", " + voxel.y + ", " + voxel.z);
            throw e;
        }
    }
}