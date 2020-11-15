using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Entities;

public struct ChunkMeshData
{
    public int3 coord;
    private int vertexIndex;
    private DynamicBuffer<ChunkMeshVerts> vertices;
    private DynamicBuffer<ChunkMeshIndices> triangles;
    private DynamicBuffer<ChunkMeshUVs> uvs;
    private DynamicBuffer<float3> normals;

    public bool instantiated;
    public bool meshGenerated;

    public ChunkMeshData(int3 coord)
    {
        this.coord = coord;
        vertexIndex = 0;
        vertices = new DynamicBuffer<ChunkMeshVerts>();
        triangles = new DynamicBuffer<ChunkMeshIndices>();
        uvs = new DynamicBuffer<ChunkMeshUVs>();
        normals = new DynamicBuffer<float3>();
        meshGenerated = false;
        instantiated = true;
    }

    public void DisableMesh()
    {
        meshGenerated = false;
    }

    /*public void DisposeData()
    {
        vertices.Dispose();
        triangles.Dispose();
        uvs.Dispose();
        normals.Dispose();
    }*/

    public void UpdateMesh()
    {
        ClearMeshData();

        for (int y = 0; y < Constants.ChunkSizeY; y++)
        {
            for (int x = 0; x < Constants.ChunkSizeX; x++)
            {
                for (int z = 0; z < Constants.ChunkSizeZ; z++)
                {
                    int3 location = WorldHelper.GetVoxelGlobalPositionFromChunk(new int3(x, y, z), coord);
                    VoxelData voxel = WorldHelper.GetVoxelFromPosition(location);
                    if (voxel.id != 0)
                    {
                        BlockType block = RimecraftWorld.Instance.blockTypes[voxel.id];
                        if (block != null && block.isSolid)
                        {
                            UpdateMeshData(WorldHelper.GetVoxelLocalPositionInChunk(location));
                        }
                    }
                }
            }
        }
    }

    public void UpdateAndCreateMesh()
    {
        UpdateMesh();
        CreateMesh();
    }

    private void ClearMeshData()
    {
        vertexIndex = 0;
        Debug.Log(instantiated);
        if (!vertices.IsEmpty)
        {
            vertices.Clear();
            triangles.Clear();
            uvs.Clear();
            normals.Clear();
        }
    }

    public void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices.Reinterpret<float3>().AsNativeArray());
        mesh.SetIndices(vertices.Reinterpret<int>().AsNativeArray(), MeshTopology.Triangles, 0);
        mesh.SetUVs(0, uvs.Reinterpret<float2>().AsNativeArray());
        mesh.SetNormals(normals.Reinterpret<float3>().AsNativeArray());

        if (!RimecraftWorld.chunkObjects.ContainsKey(coord) || (RimecraftWorld.chunkObjects.ContainsKey(coord) && RimecraftWorld.chunkObjects[coord] == null))
        {
            GameObject go = new GameObject();

            go.AddComponent<MeshFilter>().mesh = mesh;
            go.AddComponent<MeshRenderer>().material = RimecraftWorld.Instance.material;

            go.transform.SetParent(RimecraftWorld.Instance.transform);

            go.transform.position = new Vector3(coord.x * Constants.ChunkSizeX, coord.y * Constants.ChunkSizeY, coord.z * Constants.ChunkSizeZ);

            go.name = "Chunk " + coord.x + ", " + coord.y + "," + coord.z;

            RimecraftWorld.chunkObjects.Add(coord, go);
        }
        else
        {
            RimecraftWorld.chunkObjects[coord].GetComponent<MeshFilter>().mesh = mesh;
        }

        meshGenerated = true;
    }

    private void UpdateMeshData(int3 localPosition)
    {
        VoxelData voxel = WorldData.chunks[coord].map[localPosition.x, localPosition.y, localPosition.z];

        for (int p = 0; p < 6; p++)
        {
            VoxelData neighbor = WorldHelper.GetVoxelFromPosition(WorldHelper.GetVoxelGlobalPositionFromChunk(localPosition, coord) + VoxelInfo.faceChecks[p]);
            if (neighbor.id != 0 && neighbor.Properties.renderNeightborFaces)
            {
                int faceVertCount = 0;
                for (int i = 0; i < voxel.Properties.meshData.faces[p].vertData.Length; i++)
                {
                    vertices.Add(localPosition + (float3)voxel.Properties.meshData.faces[p].vertData[i].position);
                    normals.Add(voxel.Properties.meshData.faces[p].normal);
                    AddTexture(voxel.Properties.GetTextureID(p), voxel.Properties.meshData.faces[p].vertData[i].uv);
                    faceVertCount++;
                }

                for (int i = 0; i < voxel.Properties.meshData.faces[p].triangles.Length; i++)
                {
                    triangles.Add(vertexIndex + voxel.Properties.meshData.faces[p].triangles[i]);
                }

                vertexIndex += faceVertCount;
            }
        }
    }

    private void AddTexture(int textureID, Vector2 uv)
    {
        float y = textureID / VoxelInfo.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelInfo.TextureAtlasSizeInBlocks);

        x *= VoxelInfo.NoramlizedBlockTextureSize;
        y *= VoxelInfo.NoramlizedBlockTextureSize;

        y = 1 - y - VoxelInfo.NoramlizedBlockTextureSize;

        x += VoxelInfo.NoramlizedBlockTextureSize * uv.x;
        y += VoxelInfo.NoramlizedBlockTextureSize * uv.y;

        uvs.Add(new float2(x, y));
    }
}