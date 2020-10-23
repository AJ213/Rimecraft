using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class World : MonoBehaviour
{
    public int seed;
    public BiomeAttributes biome;

    public bool enableThreading;

    public Transform player;
    public Vector3 spawnPosition;

    public Material material = null;
    public Material transparentMaterial = null;
    public BlockType[] blockTypes = null;

    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    public ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    public List<Chunk> chunksToUpdate = new List<Chunk>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();

    bool applyingModifications = false;

    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();

    public GameObject debugScreen;

    Thread ChunkUpdateThread;
    public object ChunkUpdateThreadLock = new object();

    private void Start()
    {
        Random.InitState(seed);

        if(enableThreading)
        {
            ChunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
            ChunkUpdateThread.Start();
        }
        
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);

        if(!playerChunkCoord.Equals(playerLastChunkCoord))
            CheckViewDistance();

        if (chunksToCreate.Count > 0)
            CreateChunk();

        if(chunksToDraw.Count > 0)
        {
            if (chunksToDraw.Peek().IsEditable)
                chunksToDraw.Dequeue().CreateMesh();
        }

        if (!enableThreading)
        {
            if (!applyingModifications)
                ApplyModifications();

            if (chunksToUpdate.Count > 0)
                UpdateChunks();
        }

        if (Input.GetKeyDown(KeyCode.F3))
            debugScreen.SetActive(!debugScreen.activeSelf);
    }

    void GenerateWorld ()
    {
        for(int x = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; z++)
            {
                ChunkCoord newChunk = new ChunkCoord(x, z);
                chunks[x, z] = new Chunk(newChunk, this);
                chunksToCreate.Add(newChunk);
            }
        }

        

        spawnPosition = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2, VoxelData.ChunkHeight - 50, (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2);
        player.position = spawnPosition;
        CheckViewDistance();
    }

    void CreateChunk ()
    {
        ChunkCoord c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        chunks[c.x, c.z].Init();
    }

    void UpdateChunks()
    {
        bool updated = false;
        int index = 0;

        lock(ChunkUpdateThreadLock)
        {
            while (!updated && index < chunksToUpdate.Count - 1)
            {
                if (chunksToUpdate[index].IsEditable)
                {
                    chunksToUpdate[index].UpdateChunk();
                    activeChunks.Add(chunksToUpdate[index].coord);
                    chunksToUpdate.RemoveAt(index);
                    updated = true;
                }
                else
                {
                    index++;
                }
            }
        } 
    }

    void ThreadedUpdate()
    {
        while(true)
        {
            if (!applyingModifications)
                ApplyModifications();

            if (chunksToUpdate.Count > 0)
                UpdateChunks();
        }
    }

    private void OnDisable()
    {
        if (enableThreading)
        {
            ChunkUpdateThread.Abort();
        }
    }

    void ApplyModifications()
    {
        applyingModifications = true;

        while(modifications.Count > 0)
        {

            Queue<VoxelMod> queue = modifications.Dequeue();

            while(queue.Count > 0)
            {
                VoxelMod v = queue.Dequeue();

                ChunkCoord c = GetChunkCoordFromVector3(v.position);

                if (chunks[c.x, c.z] == null)
                {
                    chunks[c.x, c.z] = new Chunk(c, this);
                    chunksToCreate.Add(c);
                }

                chunks[c.x, c.z].modifications.Enqueue(v);
            }
        }
        applyingModifications = false;
    }

    ChunkCoord GetChunkCoordFromVector3 (Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return chunks[x, z];
    }

    private void CheckViewDistance()
    {
        int chunkX = Mathf.FloorToInt(player.position.x / VoxelData.ChunkWidth);
        int chunkZ = Mathf.FloorToInt(player.position.z / VoxelData.ChunkWidth);

        playerLastChunkCoord = playerChunkCoord;

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);
        activeChunks.Clear();

        for (int x = chunkX - VoxelData.ViewDistanceInChunks / 2; x < chunkX + VoxelData.ViewDistanceInChunks / 2; x++)
        {
            for (int z = chunkZ - VoxelData.ViewDistanceInChunks / 2; z < chunkZ + VoxelData.ViewDistanceInChunks / 2; z++)
            {

                // If the chunk is within the world bounds and it has not been created.
                if (!IsChunkInWorld(new ChunkCoord(x, z))) continue;
                
                ChunkCoord thisChunk = new ChunkCoord(x, z);

                if (chunks[x, z] == null)
                {
                    chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
                    chunksToCreate.Add(new ChunkCoord(x, z));
                }
                else if (!chunks[x, z].IsActive)
                {
                    chunks[x, z].IsActive = true;
                }
                activeChunks.Add(thisChunk);
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

    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
            return false;

        if(chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].IsEditable)
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].isSolid;

        return blockTypes[GetVoxel(pos)].isSolid;
    }

    public bool CheckIfVoxelTransparent(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
            return false;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].IsEditable)
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].isTransparent;

        return blockTypes[GetVoxel(pos)].isTransparent;
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

        int terrainHeight = Mathf.FloorToInt(biome.terrainheight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.terrainScale)) + biome.solidGroundHeight;
        byte voxelValue = 0;

        if (yPos == terrainHeight)
            voxelValue = 1;
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
            voxelValue = 1;
        else if (yPos > terrainHeight)
            return 0;
        else
            voxelValue = 3;

        /* SECOND PASS */

        if(voxelValue == 3 || voxelValue == 1)
        {
            foreach(Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                        voxelValue = lode.blockID;
            }
        }

        /* TREE PASS */

        if(yPos == terrainHeight)
        {
            if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 200, biome.treeZoneScale) > biome.treeZoneThreshold)
            {
                if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 700, biome.treePlacementScale) > biome.treePlacementThreshold)
                {
                    modifications.Enqueue(Structure.MakeTree(pos, biome.minTreeHeight, biome.maxTreeHeight));
                }
            }
                
        }

        return voxelValue;
    }

    bool IsChunkInWorld (ChunkCoord coord)
    {
        return (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1 && coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1);
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
    public bool isTransparent;
    public Sprite icon;

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

public class VoxelMod
{
    public Vector3 position;
    public byte id;

    public VoxelMod()
    {
        position = new Vector3();
        id = 0;
    }
    public VoxelMod(Vector3 position, byte id)
    {
        this.position = position;
        this.id = id;
    }
}


