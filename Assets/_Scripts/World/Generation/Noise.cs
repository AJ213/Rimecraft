using UnityEngine;
using Unity.Mathematics;

public static class Noise
{
    public static float Get2DSimplex(float2 position, float offset, float scale)
    {
        position.x += offset + VoxelData.seed;
        position.y += offset + VoxelData.seed;

        float simplexNoise = noise.snoise(new float2(position.x / Constants.ChunkSizeX * scale, position.y / Constants.ChunkSizeZ * scale));
        return simplexNoise;
    }

    public static float Get3DSimplex(float3 position, float offset, float scale)
    {
        float x = (position.x + offset + VoxelData.seed) * scale;
        float y = (position.y + offset + VoxelData.seed) * scale;
        float z = (position.z + offset + VoxelData.seed) * scale;

        float simplexNoise = noise.snoise(new float3(x, y, z));
        return simplexNoise;
    }
}