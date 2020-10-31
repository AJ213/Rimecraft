using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelMod
{
    public Vector3Int position;
    public ushort id;

    public VoxelMod()
    {
        position = new Vector3Int();
        id = 0;
    }

    public VoxelMod(Vector3Int position, ushort id)
    {
        this.position = position;
        this.id = id;
    }
}