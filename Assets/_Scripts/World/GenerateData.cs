using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public static class GenerateData
{
    public static ChunkData Chunk(int3 coord)
    {
        ChunkData chunk = new ChunkData(coord);
        for (int y = 0; y < Constants.ChunkSizeX; y++)
        {
            for (int x = 0; x < Constants.ChunkSizeY; x++)
            {
                for (int z = 0; z < Constants.ChunkSizeZ; z++)
                {
                    int3 localPosition = new int3(x, y, z);
                    chunk.map[x, y, z] = Voxel(WorldHelper.GetVoxelGlobalPositionFromChunk(localPosition, chunk.Coord), RimecraftWorld.Instance.biomes);
                }
            }
        }

        WorldData.AddToModifiedChunkList(chunk);
        return chunk;
    }

    public static VoxelData Voxel(int3 globalPosition, BiomeAttributes[] biomes)
    {
        int solidGroundHeight = 0;
        float sumOfHeights = 0;
        int count = 0;
        float strongestWeight = 0;
        int strongestBiomeIndex = 0;

        for (int i = 0; i < biomes.Length; i++)
        {
            float weight = Noise.Get2DSimplex(new Vector2(globalPosition.x, globalPosition.z), biomes[i].offset, biomes[i].scale);

            // Keep track of which weight is strongest.
            if (weight > strongestWeight)
            {
                strongestWeight = weight;
                strongestBiomeIndex = i;
            }

            // Get the height of the terrain (for the current biome) and multiply it by its weight.
            float height = biomes[i].terrainHeight * Noise.Get2DSimplex(new Vector2(globalPosition.x, globalPosition.z), 2 * biomes[i].offset, biomes[i].terrainScale) * weight;

            // If the height value is greater 0 add it to the sum of heights.
            if (height > 0)
            {
                sumOfHeights += height;
                count++;
            }
        }

        // Set biome to the one with the strongest weight.
        BiomeAttributes biome = biomes[strongestBiomeIndex];

        // Get the average of the heights.
        if (count == 0)
        {
            count = 1;
        }
        sumOfHeights /= count;

        int terrainHeight = Mathf.FloorToInt(sumOfHeights + solidGroundHeight);

        ushort voxelID = 0;

        SurfaceBlocks(ref voxelID, globalPosition, biome, terrainHeight);
        LodeGeneration(ref voxelID, globalPosition, biome);
        //FloraGeneration(globalPosition, biome, terrainHeight);

        if (globalPosition.y == -2)
        {
            voxelID = 2;
        }

        return new VoxelData(voxelID, globalPosition);
    }

    private static void SurfaceBlocks(ref ushort voxelID, int3 globalPosition, BiomeAttributes biome, int terrainHeight)
    {
        if (globalPosition.y == terrainHeight)
        {
            voxelID = biome.surfaceBlock;
        }
        else if (globalPosition.y < terrainHeight && globalPosition.y > terrainHeight - 4)
        {
            voxelID = biome.subSurfaceBlock;
        }
        else if (globalPosition.y > terrainHeight)
        {
            voxelID = 0;
        }
        else
        {
            voxelID = 3;
        }
    }

    private static void LodeGeneration(ref ushort voxelID, int3 globalPosition, BiomeAttributes biome)
    {
        if (voxelID == 3 || voxelID == 1)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (globalPosition.y > lode.minHeight && globalPosition.y < lode.maxHeight)
                {
                    if (Noise.Get3DSimplex(globalPosition, lode.noiseOffset, lode.scale) > lode.threshold)
                    {
                        voxelID = lode.blockID;
                    }
                }
            }
        }
    }

    private static void FloraGeneration(int3 globalPosition, BiomeAttributes biome, int terrainHeight)
    {
        if (globalPosition.y == terrainHeight && biome.placeMajorFlora)
        {
            if (Noise.Get2DSimplex(new Vector2(globalPosition.x, globalPosition.z), 200, biome.majorFloraZoneScale) > biome.majorFloraZoneThreshold)
            {
                if (Noise.Get2DSimplex(new Vector2(globalPosition.x, globalPosition.z), 700, biome.majorFloraPlacementScale) > biome.majorFloraPlacementThreshold)
                {
                    RimecraftWorld.modifications.Enqueue(Structure.GenerateMajorFlora(biome.majorFloraIndex, globalPosition, biome.minHeight, biome.maxHeight));
                }
            }
        }
    }
}