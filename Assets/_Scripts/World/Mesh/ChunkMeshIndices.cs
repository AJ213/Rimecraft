using Unity.Entities;

[InternalBufferCapacity(0)]
public struct ChunkMeshIndices : IBufferElementData
{
    public int value;

    public static implicit operator int(ChunkMeshIndices c) => c.value;

    public static implicit operator ChunkMeshIndices(int v) => new ChunkMeshIndices { value = v };
}