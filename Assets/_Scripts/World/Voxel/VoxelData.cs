using Unity.Mathematics;
using UnityEngine;

public static class VoxelData
{
    //public static int seed;

    public static readonly int TextureAtlasSizeInBlocks = 4;

    public static float NoramlizedBlockTextureSize
    { get { return 1 / (float)TextureAtlasSizeInBlocks; } }

    public static readonly int3[] voxelVerts = new int3[8]
    {
        new int3(0, 0, 0),
        new int3(1, 0, 0),
        new int3(1, 1, 0),
        new int3(0, 1, 0),
        new int3(0, 0, 1),
        new int3(1, 0, 1),
        new int3(1, 1, 1),
        new int3(0, 1, 1)
    };

    public static readonly int3[] faceChecks = new int3[6]
    {
        new int3(0, 0, -1),
        new int3(0, 0, 1),
        new int3(0, 1, 0),
        new int3(0, -1, 0),
        new int3(-1, 0, 0),
        new int3(1, 0, 0)
    };

    public static readonly int[] revFaceCheckIndex = new int[6] { 1, 0, 3, 2, 5, 4 };

    public static readonly int[,] voxelTris = new int[6, 4]
    {
        // Back, Front, Top, Bottom, Left, Right
        // 0 1 2 2 1 3
        {0, 3, 1, 2}, // Back Face
        {5, 6, 4, 7}, // Front Face
        {3, 7, 2, 6}, // Top Face
        {1, 5, 0, 4}, // Bottom Face
        {4, 7, 0, 3}, // Left Face
        {1, 2, 5, 6}  // Right Face
    };

    public static readonly int2[] voxelUvs = new int2[4]
    {
        new int2(0, 0),
        new int2(0, 1),
        new int2(1, 0),
        new int2(1, 1)
    };
}