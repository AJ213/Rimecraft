using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelNeighbors
{
    public readonly VoxelState parent;

    public VoxelNeighbors(VoxelState parent)
    {
        this.parent = parent;
    }

    private VoxelState[] neighbors = new VoxelState[6];

    public int Length { get { return neighbors.Length; } }

    public VoxelState this[int index]
    {
        get
        {
            if (neighbors[index] == null)
            {
                neighbors[index] = RimecraftWorld.Instance.worldData.GetVoxel(parent.GlobalPosition + VoxelData.faceChecks[index]);
                ReturnNeighbour(index);
            }
            return neighbors[index];
        }
        set
        {
            neighbors[index] = value;
            ReturnNeighbour(index);
        }
    }

    private void ReturnNeighbour(int index)
    {
        if (neighbors[index] == null)
        {
            return;
        }

        if (neighbors[index].neighbours[VoxelData.revFaceCheckIndex[index]] != parent)
        {
            neighbors[index].neighbours[VoxelData.revFaceCheckIndex[index]] = parent;
        }
    }
}