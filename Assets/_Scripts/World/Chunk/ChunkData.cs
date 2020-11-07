using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[System.Serializable]
public class ChunkData
{
    private int x;
    private int y;
    private int z;

    public int3 Position
    {
        get { return new int3(x, y, z); }
        set
        {
            x = value.x;
            y = value.y;
            z = value.z;
        }
    }

    public ChunkData(int3 pos)
    {
        Position = pos;
    }

    public ChunkData(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    [System.NonSerialized] public Chunk chunk;

    [HideInInspector]
    public VoxelState[,,] map = new VoxelState[Constants.ChunkSizeX, Constants.ChunkSizeY, Constants.ChunkSizeZ];

    public void Populate()
    {
        for (int y = 0; y < Constants.ChunkSizeX; y++)
        {
            for (int x = 0; x < Constants.ChunkSizeY; x++)
            {
                for (int z = 0; z < Constants.ChunkSizeZ; z++)
                {
                    int3 voxelGlobalPosition = new int3(x + this.Position.x, y + this.Position.y, z + this.Position.z);
                    map[x, y, z] = new VoxelState(RimecraftWorld.Instance.GetVoxel(voxelGlobalPosition), this, new int3(x, y, z));

                    for (int p = 0; p < 6; p++)
                    {
                        int3 neighbourV3 = new int3(x, y, z) + VoxelData.faceChecks[p];
                        if (WorldHelper.IsInRange(neighbourV3.x, Constants.ChunkSizeX) &&
                            WorldHelper.IsInRange(neighbourV3.y, Constants.ChunkSizeY) &&
                            WorldHelper.IsInRange(neighbourV3.z, Constants.ChunkSizeZ))
                        {
                            map[x, y, z].neighbours[p] = VoxelFromV3Int(neighbourV3);
                        }
                        else
                        {
                            map[x, y, z].neighbours[p] = RimecraftWorld.Instance.worldData.GetVoxel(voxelGlobalPosition + VoxelData.faceChecks[p]);
                        }
                    }
                }
            }
        }

        RimecraftWorld.Instance.worldData.AddToModifiedChunkList(this);
    }

    public void ModifyVoxel(int3 pos, ushort id)
    {
        if (map[pos.x, pos.y, pos.z] == null || map[pos.x, pos.y, pos.z].id == id)
        {
            return;
        }

        VoxelState voxel = map[pos.x, pos.y, pos.z];
        BlockType newVoxel = RimecraftWorld.Instance.blockTypes[id];

        voxel.id = id;
        RimecraftWorld.Instance.worldData.AddToModifiedChunkList(this);
        if (chunk != null)
        {
            RimecraftWorld.Instance.AddChunkToUpdate(chunk, true);
        }
    }

    public VoxelState VoxelFromV3Int(int3 pos)
    {
        return map[pos.x, pos.y, pos.z];
    }
}