using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class VoxelMod
{
    public int3 position;
    public ushort id;

    public VoxelMod()
    {
        position = new int3();
        id = 0;
    }

    public VoxelMod(int3 position, ushort id)
    {
        this.position = position;
        this.id = id;
    }
}