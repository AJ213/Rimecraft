﻿using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(0)]
public struct ChunkMeshVerts : IBufferElementData
{
    public float3 value;

    public static implicit operator float3(ChunkMeshVerts c) => c.value;

    public static implicit operator ChunkMeshVerts(float3 v) => new ChunkMeshVerts { value = v };
}