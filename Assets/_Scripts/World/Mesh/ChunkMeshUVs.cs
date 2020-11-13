using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(0)]
public struct ChunkMeshUVs : IBufferElementData
{
    public float2 value;

    public static implicit operator float2(ChunkMeshUVs c) => c.value;

    public static implicit operator ChunkMeshUVs(float2 v) => new ChunkMeshUVs { value = v };
}