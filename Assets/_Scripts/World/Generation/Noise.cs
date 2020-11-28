using UnityEngine;
using Unity.Mathematics;

public static class Noise
{
    public static float Get2DSimplex(float2 position, float offset, float scale)
    {
        position.x += offset + WorldData.seed;
        position.y += offset + WorldData.seed;

        float simplexNoise = noise.cnoise(new float2(position.x / Constants.ChunkSizeX * scale, position.y / Constants.ChunkSizeZ * scale));
        return simplexNoise;
    }

    public static float Get3DSimplex(float3 position, float offset, float scale)
    {
        float x = (position.x + offset + WorldData.seed) * scale;
        float y = (position.y + offset + WorldData.seed) * scale;
        float z = (position.z + offset + WorldData.seed) * scale;

        float simplexNoise = noise.cnoise(new float3(x, y, z));
        return simplexNoise;
    }
}