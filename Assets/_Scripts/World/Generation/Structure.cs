using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
    public static Queue<VoxelMod> GenerateMajorFlora(int index, Vector3Int position, int minTrunkHeight, int maxTrunkHeight)
    {
        switch (index)
        {
            case 0:
                return MakeTree(position, minTrunkHeight, maxTrunkHeight);

            default:
                return new Queue<VoxelMod>();
        }
    }

    public static Queue<VoxelMod> MakeTree(Vector3Int position, int minTrunkHeight, int maxTrunkHeight)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();

        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 222, 3));

        if (height < minTrunkHeight)
        {
            height = minTrunkHeight;
        }

        for (int i = 1; i < height; i++)
        {
            queue.Enqueue(new VoxelMod(new Vector3Int(position.x, position.y + i, position.z), 4));
        }
        for (int y = 1; y < 3; y++)
        {
            for (int x = -3; x < 3; x++)
            {
                for (int z = -y; z < 3; z++)
                {
                    queue.Enqueue(new VoxelMod(new Vector3Int(position.x + x, position.y + height - y, position.z + z), 6));
                }
            }
        }

        return queue;
    }
}