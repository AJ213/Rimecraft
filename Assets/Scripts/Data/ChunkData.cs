using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkData
{
    private int x;
    private int y;

    public Vector2Int Position
    {
        get { return new Vector2Int(x, y); }
        set
        {
            x = value.x;
            y = value.y;
        }
    }

    public ChunkData(Vector2Int pos)
    {
        Position = pos;
    }

    public ChunkData(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    [System.NonSerialized] public Chunk chunk;

    [HideInInspector]
    public VoxelState[,,] map = new VoxelState[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    public void Populate()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    Vector3 voxelGlobalPosition = new Vector3(x + Position.x, y, z + Position.y);
                    map[x, y, z] = new VoxelState(World.Instance.GetVoxel(voxelGlobalPosition), this, new Vector3Int(x, y, z));

                    for (int p = 0; p < 6; p++)
                    {
                        Vector3Int neighbourV3 = new Vector3Int(x, y, z) + VoxelData.faceChecks[p];
                        if (IsVoxelInChunk(neighbourV3))
                        {
                            map[x, y, z].neighbours[p] = VoxelFromV3Int(neighbourV3);
                        }
                        else
                        {
                            map[x, y, z].neighbours[p] = World.Instance.worldData.GetVoxel(voxelGlobalPosition + VoxelData.faceChecks[p]);
                        }
                    }
                }
            }
        }

        World.Instance.worldData.AddToModifiedChunkList(this);
    }

    public void ModifyVoxel(Vector3Int pos, ushort id)
    {
        if (map[pos.x, pos.y, pos.z].id == id)
        {
            return;
        }

        VoxelState voxel = map[pos.x, pos.y, pos.z];
        //BlockType newVoxel = World.Instance.blockTypes[id];

        voxel.id = id;
        World.Instance.worldData.AddToModifiedChunkList(this);
        if (chunk != null)
        {
            World.Instance.AddChunkToUpdate(chunk);
        }
    }

    public bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool IsVoxelInChunk(Vector3Int pos)
    {
        return IsVoxelInChunk(pos.x, pos.y, pos.z);
    }

    public VoxelState VoxelFromV3Int(Vector3Int pos)
    {
        return map[pos.x, pos.y, pos.z];
    }
}