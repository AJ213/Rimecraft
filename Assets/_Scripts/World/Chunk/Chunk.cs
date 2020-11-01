using System.Collections.Generic;
using UnityEngine;
using System;

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

    public Vector3Int position;

    private bool isActive;

    private ChunkData chunkData;

    public Chunk(ChunkCoord coord)
    {
        this.coord = coord;

        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        materials[0] = World.Instance.material;
        materials[1] = World.Instance.transparentMaterial;
        meshRenderer.materials = materials;

        chunkObject.transform.SetParent(World.Instance.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, coord.y * VoxelData.ChunkWidth, coord.z * VoxelData.ChunkWidth);
        chunkObject.name = "Chunk " + coord.x + ", " + coord.y + "," + coord.z;
        position = Vector3Int.FloorToInt(chunkObject.transform.position);

        chunkData = World.Instance.worldData.RequestChunk(new Vector3Int(position.x, position.y, position.z), true);
        chunkData.chunk = this;

        World.Instance.AddChunkToUpdate(this);
    }

    public void UpdateChunk()
    {
        ClearMeshData();

        for (int y = 0; y < VoxelData.ChunkWidth; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    if (World.Instance.blockTypes[chunkData.map[x, y, z].id].isSolid)
                    {
                        UpdateMeshData(new Vector3Int(x, y, z));
                    }
                }
            }
        }
        World.Instance.chunksToDraw.Enqueue(this);
    }

    public void EditVoxel(Vector3 pos, ushort newID)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        yCheck -= Mathf.FloorToInt(chunkObject.transform.position.y);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        chunkData.ModifyVoxel(new Vector3Int(xCheck, yCheck, zCheck), newID);

        UpdateSorroundingVoxels(xCheck, yCheck, zCheck);
    }

    private void UpdateSorroundingVoxels(int x, int y, int z)
    {
        Vector3Int thisVoxel = new Vector3Int(x, y, z);
        for (int p = 0; p < 6; p++)
        {
            Vector3Int currentVoxel = thisVoxel + VoxelData.faceChecks[p];

            if (World.IsInRange(currentVoxel, VoxelData.ChunkWidth))
            {
                World.Instance.AddChunkToUpdate(World.Instance.GetChunkFromVector3(currentVoxel + position), true);
            }
        }
    }

    public VoxelState GetVoxelFromGlobalVector3(Vector3Int pos)
    {
        return chunkData.map[pos.x - position.x, pos.y - position.y, pos.z - position.z];
    }

    private void UpdateMeshData(Vector3Int pos)
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
                    vertices.Add(pos + voxel.Properties.meshData.faces[p].vertData[i].position);
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