﻿using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class WorldHelper
{
    public static int3 GetChunkCoordFromPosition(float3 globalPosition)
    {
        int x = Mathf.FloorToInt(globalPosition.x / Constants.ChunkSizeX);
        int y = Mathf.FloorToInt(globalPosition.y / Constants.ChunkSizeY);
        int z = Mathf.FloorToInt(globalPosition.z / Constants.ChunkSizeZ);
        return new int3(x, y, z);
    }

    public static int3 GetVoxelLocalPositionInChunk(float3 globalPosition)
    {
        return new int3(Mod(Mathf.FloorToInt(globalPosition.x), Constants.ChunkSizeX),
                        Mod(Mathf.FloorToInt(globalPosition.y), Constants.ChunkSizeY),
                        Mod(Mathf.FloorToInt(globalPosition.z), Constants.ChunkSizeZ));
    }

    public static int3 GetVoxelGlobalPositionFromChunk(float3 localPosition, int3 coord)
    {
        return new int3(Mathf.FloorToInt(localPosition.x) + (Constants.ChunkSizeX * coord.x),
                        Mathf.FloorToInt(localPosition.y) + (Constants.ChunkSizeY * coord.y),
                        Mathf.FloorToInt(localPosition.z) + (Constants.ChunkSizeZ * coord.z));
    }

    public static ChunkMeshData GetChunkMeshFromPosition(float3 globalPosition)
    {
        int3 coord = GetChunkCoordFromPosition(globalPosition);

        if (RimecraftWorld.chunkMeshes.ContainsKey(coord))
        {
            return RimecraftWorld.chunkMeshes[coord];
        }
        else
        {
            ChunkMeshData mesh = new ChunkMeshData(coord);
            RimecraftWorld.chunkMeshes.TryAdd(coord, mesh);
            mesh.UpdateMesh();
            return mesh;
        }
    }

    public static VoxelData GetVoxelFromPosition(float3 globalPosition)
    {
        ChunkMeshData chunk = GetChunkMeshFromPosition(globalPosition);
        if (!WorldData.chunks.ContainsKey(chunk.coord))
        {
            Debug.Log("No good");
            return new VoxelData();
        }
        else
        {
            return WorldData.chunks[chunk.coord].VoxelFromPosition(GetVoxelLocalPositionInChunk(globalPosition));
        }
    }

    public static bool IsVoxelGlobalPositionInChunk(float3 globalPosition, int3 coord)
    {
        return ((float3)GetVoxelGlobalPositionFromChunk(GetVoxelLocalPositionInChunk(globalPosition), coord)).Equals(globalPosition);
    }

    public static bool IsInRange(int value, int length)
    {
        return (value >= 0 && value < length);
    }

    public static bool IsInRange(int3 value, int length)
    {
        return (IsInRange(value.x, length) && IsInRange(value.y, length) && IsInRange(value.z, length));
    }

    public static int Mod(int a, int n)
    {
        return ((a % n) + n) % n;
    }
}