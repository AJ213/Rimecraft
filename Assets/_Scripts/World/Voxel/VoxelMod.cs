using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelMod
{
    public Vector3 position;
    public ushort id;

    public VoxelMod()
    {
        position = new Vector3();
        id = 0;
    }

    public VoxelMod(Vector3 position, ushort id)
    {
        this.position = position;
        this.id = id;
    }
}