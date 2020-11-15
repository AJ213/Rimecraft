using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[System.Serializable]
public class ChunkData
{
    private int3 coord;

    public VoxelData[,,] map = new VoxelData[Constants.ChunkSizeX, Constants.ChunkSizeY, Constants.ChunkSizeZ];

    public int3 Coord
    {
        get { return coord; }
        set { coord = value; }
    }

    public ChunkData(int3 pos)
    {
        Coord = pos;
    }

    public ChunkData(int x, int y, int z)
    {
        coord = new int3(x, y, z);
    }

    public void ModifyVoxel(int3 localPosition, ushort id)
    {
        VoxelData voxel = map[localPosition.x, localPosition.y, localPosition.z];

        voxel.id = id;
        WorldData.AddToModifiedChunkList(this);
        RimecraftWorld.chunkMeshes[coord].UpdateAndCreateMesh();
        int3 globalPosition = WorldHelper.GetVoxelGlobalPositionFromChunk(localPosition, coord);
        UpdateSorroundingChunks(new int3(Mathf.FloorToInt(globalPosition.x),
                             Mathf.FloorToInt(globalPosition.y),
                             Mathf.FloorToInt(globalPosition.z)));
    }

    private void UpdateSorroundingChunks(int3 globalPosition)
    {
        for (int p = 0; p < 6; p++)
        {
            int3 currentVoxel = globalPosition + VoxelInfo.faceChecks[p];

            if (!WorldHelper.IsVoxelGlobalPositionInChunk(currentVoxel, coord))
            {
                //Debug.Log(currentVoxel + " not in chunk " + coord);
                RimecraftWorld.chunkMeshes[WorldHelper.GetChunkCoordFromPosition(currentVoxel)].UpdateAndCreateMesh();
            }
        }
    }

    public VoxelData VoxelFromPosition(int3 localPosition)
    {
        return map[localPosition.x, localPosition.y, localPosition.z];
    }
}