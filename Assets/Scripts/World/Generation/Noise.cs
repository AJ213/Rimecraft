using UnityEngine;
using Unity.Mathematics;

public static class Noise
{
    public static float Get2DPerlin(Vector2 position, float offset, float scale)
    {
        position.x += (offset + VoxelData.seed + 0.1f);
        position.y += (offset + VoxelData.seed + 0.1f);
        float simplexNoise = noise.snoise(new float2(((position.x + 0.1f) / VoxelData.ChunkWidth * scale) + offset, ((position.y + 0.1f) / VoxelData.ChunkWidth * scale) + offset));
        //float perlineNoise = Mathf.PerlinNoise(((position.x + 0.1f) / VoxelData.ChunkWidth * scale) + offset, ((position.y + 0.1f) / VoxelData.ChunkWidth * scale) + offset);
        return simplexNoise;
    }

    public static float Get3DPerlin(Vector3 position, float offset, float scale)
    {
        float x = (position.x + offset + VoxelData.seed + 0.1f) * scale;
        float y = (position.y + offset + VoxelData.seed + 0.1f) * scale;
        float z = (position.z + offset + VoxelData.seed + 0.1f) * scale;
        /*
        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);
        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);*/

        //return (AB + BC + AC + BA + CB + CA) / 6;
        float simplexNoise = noise.snoise(new float3(x, y, z));
        return simplexNoise;
    }
}