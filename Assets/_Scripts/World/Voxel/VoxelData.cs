using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct VoxelData
{
    public ushort id;
    [System.NonSerialized] public int3 position;

    public VoxelData(ushort id, int3 position)
    {
        this.id = id;
        this.position = position;
    }

    public BlockType Properties
    {
        get { return RimecraftWorld.Instance.blockTypes[id]; }
    }
}