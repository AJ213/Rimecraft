using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WorldData
{
    public string worldName = "Prototype";
    public int seed;

    [System.NonSerialized]
    public Dictionary<Vector3Int, ChunkData> chunks = new Dictionary<Vector3Int, ChunkData>();

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

    public ChunkData RequestChunk(Vector3Int coord, bool create)
    {
        ChunkData c;

        lock (World.Instance.ChunkListThreadLock)
        {
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
        }

        return c;
    }

    public void LoadChunk(Vector3Int coord)
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

    public void SetVoxel(Vector3Int pos, ushort value)
    {
        if (!World.IsInRange(pos, VoxelData.WorldSizeInVoxels))
        {
            return;
        }

        int x = Mathf.FloorToInt((float)pos.x / VoxelData.ChunkWidth);
        int y = Mathf.FloorToInt((float)pos.y / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt((float)pos.z / VoxelData.ChunkWidth);

        x *= VoxelData.ChunkWidth;
        y *= VoxelData.ChunkWidth;
        z *= VoxelData.ChunkWidth;

        ChunkData chunk = RequestChunk(new Vector3Int(x, y, z), true);

        Vector3Int voxel = new Vector3Int((pos.x - x), (pos.y - y), (pos.z - z));

        chunk.ModifyVoxel(voxel, value);
    }

    public VoxelState GetVoxel(Vector3Int pos)
    {
        if (!World.IsInRange(pos, VoxelData.WorldSizeInVoxels))
        {
            return null;
        }

        int x = Mathf.FloorToInt((float)pos.x / VoxelData.ChunkWidth);
        int y = Mathf.FloorToInt((float)pos.y / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt((float)pos.z / VoxelData.ChunkWidth);

        x *= VoxelData.ChunkWidth;
        y *= VoxelData.ChunkWidth;
        z *= VoxelData.ChunkWidth;

        ChunkData chunk = RequestChunk(new Vector3Int(x, y, z), false);

        if (chunk == null)
        {
            return null;
        }

        Vector3Int voxel = new Vector3Int((pos.x - x), (pos.y - y), (pos.z - z));
        try
        {
            return chunk.map[voxel.x, voxel.y, voxel.z];
        }
        catch (System.Exception e)
        {
            Debug.Log(pos.x + ", " + pos.y + ", " + pos.z);
            Debug.Log(voxel.x + ", " + voxel.y + ", " + voxel.z);
            throw e;
        }
    }
}