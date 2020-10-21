using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public int seed;

    public Transform player;
    public Vector3 spawnPosition;

    public Material material = null;
    public BlockType[] blockTypes = null;

    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    private void Start()
    {
        Random.InitState(seed);


        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);

    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);

        if(!playerChunkCoord.Equals(playerLastChunkCoord))
            CheckViewDistance();

    }

    void GenerateWorld ()
    {
        for(int x = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; z++)
            {
                CreateChunk(new ChunkCoord(x, z));
            }
        }
        spawnPosition = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2, VoxelData.ChunkHeight + 2, (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2);
        player.position = spawnPosition;
    }

    ChunkCoord GetChunkCoordFromVector3 (Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);
    }

    private void CheckViewDistance()
    {

        int chunkX = Mathf.FloorToInt(player.position.x / VoxelData.ChunkWidth);
        int chunkZ = Mathf.FloorToInt(player.position.z / VoxelData.ChunkWidth);

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        for (int x = chunkX - VoxelData.ViewDistanceInChunks / 2; x < chunkX + VoxelData.ViewDistanceInChunks / 2; x++)
        {
            for (int z = chunkZ - VoxelData.ViewDistanceInChunks / 2; z < chunkZ + VoxelData.ViewDistanceInChunks / 2; z++)
            {

                // If the chunk is within the world bounds and it has not been created.
                if (!IsChunkInWorld(x, z)) continue;

                ChunkCoord thisChunk = new ChunkCoord(x, z);

                if (chunks[x, z] == null)
                    CreateChunk(thisChunk);
                else if (!chunks[x, z].IsActive)
                {
                    chunks[x, z].IsActive = true;
                    activeChunks.Add(thisChunk);
                }
                // Check if this chunk was already in the active chunks list.
                for (int i = 0; i < previouslyActiveChunks.Count; i++)
                {

                    if (previouslyActiveChunks[i].x == x && previouslyActiveChunks[i].z == z)
                        previouslyActiveChunks.RemoveAt(i);

                }

                
            }
        }

        foreach (ChunkCoord coord in previouslyActiveChunks)
            chunks[coord.x, coord.z].IsActive = false;

    }

    public byte GetVoxel (Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);
        
        /* IMMUTABLE PASS */

        // If outside word, return air.
        if (!IsVoxelInWorld(pos))
            return 0;

        // Bottom of world that is unbreakable.
        if (yPos == 0)
            return 2;

        /* BASIC TERRAIN PASS */

        int terrainHeight = Mathf.FloorToInt(VoxelData.ChunkHeight * Noise.Get2DPerline(new Vector2(pos.x, pos.z), 500, 0.25f));

        if (yPos >= terrainHeight - 3 && yPos <= terrainHeight)
            return 1;
        else if (yPos > terrainHeight)
            return 0;
        else
            return 3;
    }

    void CreateChunk(ChunkCoord coord)
    {
        chunks[coord.x, coord.z] = new Chunk(new ChunkCoord(coord.x, coord.z), this);
        activeChunks.Add(new ChunkCoord (coord.x, coord.z));
    }

    bool IsChunkInWorld (int x, int z)
    {
        return (x > 0 && x < VoxelData.WorldSizeInChunks - 1 && z > 0 && z < VoxelData.WorldSizeInChunks - 1);
    }

    bool IsVoxelInWorld(Vector3 pos)
    {
        return (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.ChunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels);
    }
}

[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;

    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    // Back, Front, Top, Bottom, Left, Right

    public int GetTextureID (int faceIndex)
    {
        switch(faceIndex)
        {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;
        }
    }
}