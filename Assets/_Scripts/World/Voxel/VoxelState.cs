using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class VoxelState
{
    public ushort id;
    [System.NonSerialized] public ChunkData chunkData;
    [System.NonSerialized] public int3 position;

    public VoxelState(ushort id, ChunkData chunkData, int3 position)
    {
        this.id = id;
        this.chunkData = chunkData;
        this.position = position;
    }

    public BlockType Properties
    {
        get { return RimecraftWorld.Instance.blockTypes[id]; }
    }
}