using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;

public class Chunk
{
    public ChunkCoord coord;

    private GameObject chunkObject;
    private MeshRenderer meshRenderer = null;
    private MeshFilter meshFilter = null;

    private int vertexIndex = 0;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<int> transparentTriangles = new List<int>();
    private Material[] materials = new Material[2];
    private List<Vector2> uvs = new List<Vector2>();
    private List<Vector3> normals = new List<Vector3>();

    public int3 position;

    private bool isActive;

    private ChunkData chunkData;

    public Chunk(ChunkCoord coord)
    {
        this.coord = coord;

        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        materials[0] = RimecraftWorld.Instance.material;
        materials[1] = RimecraftWorld.Instance.transparentMaterial;
        meshRenderer.materials = materials;

        chunkObject.transform.SetParent(RimecraftWorld.Instance.transform);
        chunkObject.transform.position = new Vector3(coord.x * Constants.ChunkSizeX, coord.y * Constants.ChunkSizeY, coord.z * Constants.ChunkSizeZ);
        chunkObject.name = "Chunk " + coord.x + ", " + coord.y + "," + coord.z;
        position = (int3)(float3)chunkObject.transform.position;

        chunkData = RimecraftWorld.Instance.worldData.RequestChunk(new int3(position.x, position.y, position.z), true);
        chunkData.chunk = this;

        RimecraftWorld.Instance.AddChunkToUpdate(this);
    }

    public void UpdateChunk()
    {
        ClearMeshData();

        for (int y = 0; y < Constants.ChunkSizeY; y++)
        {
            for (int x = 0; x < Constants.ChunkSizeX; x++)
            {
                for (int z = 0; z < Constants.ChunkSizeZ; z++)
                {
                    if (RimecraftWorld.Instance.blockTypes[chunkData.map[x, y, z].id].isSolid)
                    {
                        UpdateMeshData(new int3(x, y, z));
                    }
                }
            }
        }
        RimecraftWorld.Instance.chunksToDraw.Enqueue(this);
    }

    public void EditVoxel(Vector3 pos, ushort newID)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        yCheck -= Mathf.FloorToInt(chunkObject.transform.position.y);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        chunkData.ModifyVoxel(new int3(xCheck, yCheck, zCheck), newID);

        UpdateSorroundingVoxels(xCheck, yCheck, zCheck);
    }

    private void UpdateSorroundingVoxels(int x, int y, int z)
    {
        int3 thisVoxel = new int3(x, y, z);
        for (int p = 0; p < 6; p++)
        {
            int3 currentVoxel = thisVoxel + VoxelData.faceChecks[p];

            if (!WorldHelper.IsInRange(currentVoxel.x, Constants.ChunkSizeX) &&
                !WorldHelper.IsInRange(currentVoxel.y, Constants.ChunkSizeY) &&
                !WorldHelper.IsInRange(currentVoxel.z, Constants.ChunkSizeZ) &&
                WorldHelper.IsInRange(currentVoxel + position, Constants.WorldSizeInVoxels))
            {
                RimecraftWorld.Instance.AddChunkToUpdate(WorldHelper.GetChunkFromVector3((float3)(currentVoxel + position)), true);
            }
        }
    }

    public VoxelState GetVoxelFromGlobalVector3(int3 pos)
    {
        return chunkData.map[pos.x - position.x, pos.y - position.y, pos.z - position.z];
    }

    private void UpdateMeshData(int3 pos)
    {
        VoxelState voxel = chunkData.map[pos.x, pos.y, pos.z];

        for (int p = 0; p < 6; p++)
        {
            VoxelState neighbour = chunkData.map[pos.x, pos.y, pos.z].neighbours[p];
            if (neighbour != null && neighbour.Properties.renderNeightborFaces)
            {
                int faceVertCount = 0;
                for (int i = 0; i < voxel.Properties.meshData.faces[p].vertData.Length; i++)
                {
                    vertices.Add(pos + (float3)voxel.Properties.meshData.faces[p].vertData[i].position);
                    normals.Add(voxel.Properties.meshData.faces[p].normal);
                    AddTexture(voxel.Properties.GetTextureID(p), voxel.Properties.meshData.faces[p].vertData[i].uv);
                    faceVertCount++;
                }

                if (!voxel.Properties.renderNeightborFaces)
                {
                    for (int i = 0; i < voxel.Properties.meshData.faces[p].triangles.Length; i++)
                    {
                        triangles.Add(vertexIndex + voxel.Properties.meshData.faces[p].triangles[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < voxel.Properties.meshData.faces[p].triangles.Length; i++)
                    {
                        transparentTriangles.Add(vertexIndex + voxel.Properties.meshData.faces[p].triangles[i]);
                    }
                }

                vertexIndex += faceVertCount;
            }
        }
    }

    public void CreateMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            //triangles = triangles.ToArray(),
            uv = uvs.ToArray()
        };
        mesh.subMeshCount = 2;
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(transparentTriangles.ToArray(), 1);
        mesh.normals = normals.ToArray();
        meshFilter.mesh = mesh;
    }

    private void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        transparentTriangles.Clear();
        uvs.Clear();
        normals.Clear();
    }

    public bool IsActive
    {
        get { return isActive; }
        set
        {
            isActive = value;
            if (chunkObject != null)
            {
                chunkObject.SetActive(value);
            }
        }
    }

    private void AddTexture(int textureID, Vector2 uv)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NoramlizedBlockTextureSize;
        y *= VoxelData.NoramlizedBlockTextureSize;

        y = 1 - y - VoxelData.NoramlizedBlockTextureSize;

        x += VoxelData.NoramlizedBlockTextureSize * uv.x;
        y += VoxelData.NoramlizedBlockTextureSize * uv.y;

        uvs.Add(new Vector2(x, y));
    }
}