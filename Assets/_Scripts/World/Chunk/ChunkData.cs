using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkData
{
    private int x;
    private int y;
    private int z;

    public Vector3Int Position
    {
        get { return new Vector3Int(x, y, z); }
        set
        {
            x = value.x;
            y = value.y;
            z = value.z;
        }
    }

    public ChunkData(Vector3Int pos)
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
    public VoxelState[,,] map = new VoxelState[VoxelData.ChunkWidth, VoxelData.ChunkWidth, VoxelData.ChunkWidth];

    public void Populate()
    {
        for (int y = 0; y < VoxelData.ChunkWidth; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    Vector3Int voxelGlobalPosition = new Vector3Int(x + this.Position.x, y + this.Position.y, z + this.Position.z);
                    map[x, y, z] = new VoxelState(World.Instance.GetVoxel(voxelGlobalPosition), this, new Vector3Int(x, y, z));

                    for (int p = 0; p < 6; p++)
                    {
                        Vector3Int neighbourV3 = new Vector3Int(x, y, z) + VoxelData.faceChecks[p];
                        if (World.IsInRange(neighbourV3, VoxelData.ChunkWidth))
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
        BlockType newVoxel = World.Instance.blockTypes[id];

        voxel.id = id;
        World.Instance.worldData.AddToModifiedChunkList(this);
        if (chunk != null)
        {
            World.Instance.AddChunkToUpdate(chunk, true);
        }
    }

    public VoxelState VoxelFromV3Int(Vector3Int pos)
    {
        return map[pos.x, pos.y, pos.z];
    }
}