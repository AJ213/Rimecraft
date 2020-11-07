using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class VoxelState
{
    public ushort id;
    [System.NonSerialized] public ChunkData chunkData;
    [System.NonSerialized] public VoxelNeighbors neighbours;
    [System.NonSerialized] public int3 position;

    public VoxelState(ushort id, ChunkData chunkData, int3 position)
    {
        this.id = id;
        this.chunkData = chunkData;
        neighbours = new VoxelNeighbors(this);
        this.position = position;
    }

    public int3 GlobalPosition
    {
        get { return new int3(position.x + chunkData.Coord.x, position.y + chunkData.Coord.y, position.z + chunkData.Coord.z); }
    }

    public BlockType Properties
    {
        get { return RimecraftWorld.Instance.blockTypes[id]; }
    }
}