using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class Structure
{
    public static ConcurrentQueue<VoxelMod> GenerateMajorFlora(int index, int3 position, int minTrunkHeight, int maxTrunkHeight)
    {
        switch (index)
        {
            case 0:
                return MakeTree(position, minTrunkHeight, maxTrunkHeight);

            default:
                return new ConcurrentQueue<VoxelMod>();
        }
    }

    public static ConcurrentQueue<VoxelMod> MakeTree(int3 position, int minTrunkHeight, int maxTrunkHeight)
    {
        ConcurrentQueue<VoxelMod> queue = new ConcurrentQueue<VoxelMod>();

        int height = Mathf.FloorToInt(maxTrunkHeight * Noise.Get2DSimplex(new int2(position.x, position.z), 222, 3));

        if (height < minTrunkHeight)
        {
            height = minTrunkHeight;
        }

        for (int i = 1; i < height; i++)
        {
            queue.Enqueue(new VoxelMod(new int3(position.x, position.y + i, position.z), 4));
        }
        for (int y = 1; y < 3; y++)
        {
            for (int x = -3; x < 3; x++)
            {
                for (int z = -y; z < 3; z++)
                {
                    queue.Enqueue(new VoxelMod(new int3(position.x + x, position.y + height - y, position.z + z), 6));
                }
            }
        }

        return queue;
    }
}