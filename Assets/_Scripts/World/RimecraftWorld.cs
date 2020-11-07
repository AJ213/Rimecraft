using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using System.Linq;

public class RimecraftWorld : MonoBehaviour
{
    public Settings settings;
    public BiomeAttributes[] biomes;

    public Transform player;
    public Vector3 spawnPosition;

    public Material material = null;
    public Material transparentMaterial = null;
    public BlockType[] blockTypes = null;

    [HideInInspector]
    public Dictionary<int3, Chunk> chunks = new Dictionary<int3, Chunk>();

    private List<int3> activeChunks = new List<int3>();
    public int3 playerChunkCoord;
    private int3 playerLastChunkCoord;

    private List<Chunk> chunksToUpdate = new List<Chunk>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();

    private bool applyingModifications = false;

    private Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();

    private bool inUI = false;

    public GameObject debugScreen;

    private static RimecraftWorld instance;
    public static RimecraftWorld Instance => instance;

    public WorldData worldData;

    public string appPath;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        appPath = Application.persistentDataPath;
    }

    private void Start()
    {
        Debug.Log("Generating new world using seed " + VoxelData.seed);

        worldData = SaveSystem.LoadWorld("Prototype");

        UnityEngine.Random.InitState(VoxelData.seed);
        Camera.main.farClipPlane = Mathf.Sqrt(2) * Constants.ChunkSizeX * 2 * settings.viewDistanceInChunks;
        LoadWorld();

        spawnPosition = new Vector3(0, 0, 0);
        player.position = spawnPosition;
        CheckLoadDistance();
        CheckViewDistance();

        playerLastChunkCoord = WorldHelper.GetChunkCoordFromPosition(player.position);
    }

    private void Update()
    {
        playerChunkCoord = WorldHelper.GetChunkCoordFromPosition(player.position);

        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {
            CheckLoadDistance();
            CheckViewDistance();
        }

        if (!applyingModifications)
        {
            ApplyModifications();
        }

        if (chunksToUpdate.Count > 0)
        {
            UpdateChunks();
        }

        if (chunksToDraw.Count > 0)
        {
            chunksToDraw.Dequeue().CreateMesh();
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }
    }

    private void LoadWorld()
    {
        /* for (int x = -Constants.WorldSizeInChunks / 2; x < Constants.WorldSizeInChunks / 2; x++)
         {
             for (int y = -Constants.WorldSizeInChunks; y < 0; y++)
             {
                 for (int z = -Constants.WorldSizeInChunks / 2; z < Constants.WorldSizeInChunks / 2; z++)
                 {
                     worldData.LoadChunk(new int3(x, y, z));
                 }
             }
         }*/
    }

    public void AddChunkToUpdate(Chunk chunk)
    {
        AddChunkToUpdate(chunk, false);
    }

    public void AddChunkToUpdate(Chunk chunk, bool insert)
    {
        // Lock list to ensure only one thing is using the list at a time.

        // Make sure update list doesn't already contain chunk.
        if (!chunksToUpdate.Contains(chunk))
        {
            // If insert is true, chunk gets inserted at the top of the list.
            if (insert)
            {
                chunksToUpdate.Insert(0, chunk);
            }
            else
            {
                chunksToUpdate.Add(chunk);
            }
        }
    }

    private void UpdateChunks()
    {
        chunksToUpdate[0].UpdateChunk();
        if (!activeChunks.Contains(chunksToUpdate[0].coord))
        {
            activeChunks.Add(chunksToUpdate[0].coord);
        }
        chunksToUpdate.RemoveAt(0);
    }

    private void ApplyModifications()
    {
        applyingModifications = true;

        while (modifications.Count > 0)
        {
            Queue<VoxelMod> queue = modifications.Dequeue();

            while (queue.Count > 0)
            {
                VoxelMod v = queue.Dequeue();

                worldData.SetVoxel(v.position, v.id);
            }
        }
        applyingModifications = false;
    }

    private void CheckViewDistance()
    {
        int3 coord = WorldHelper.GetChunkCoordFromPosition(player.position);
        playerLastChunkCoord = playerChunkCoord;

        List<int3> previouslyActiveChunks = new List<int3>(activeChunks);

        activeChunks.Clear();

        // Loop through all chunks currently within view distance of the player.
        for (int x = coord.x - settings.viewDistanceInChunks; x < coord.x + settings.viewDistanceInChunks; x++)
        {
            for (int y = coord.y - settings.viewDistanceInChunks; y < coord.y + settings.viewDistanceInChunks; y++)
            {
                for (int z = coord.z - settings.viewDistanceInChunks; z < coord.z + settings.viewDistanceInChunks; z++)
                {
                    int3 location = new int3(x, y, z);
                    if (!chunks.ContainsKey(location))
                    {
                        chunks[location] = new Chunk(location);
                    }

                    chunks[location].IsActive = true;
                    activeChunks.Add(location);

                    // Check through previously active chunks to see if this chunk is there. If it is, remove it from the list.
                    for (int i = 0; i < previouslyActiveChunks.Count; i++)
                    {
                        if (previouslyActiveChunks[i].Equals(location))
                        {
                            previouslyActiveChunks.RemoveAt(i);
                        }
                    }
                }
            }
        }

        // Any chunks left in the previousActiveChunks list are no longer in the player's view distance, so loop through and disable them.
        foreach (int3 c in previouslyActiveChunks)
        {
            chunks[new int3(c.x, c.y, c.z)].IsActive = false;
        }
    }

    private void CheckLoadDistance()
    {
        int3 coord = WorldHelper.GetChunkCoordFromPosition(player.position);
        playerLastChunkCoord = playerChunkCoord;

        for (int x = coord.x - settings.loadDistance; x < coord.x + settings.loadDistance; x++)
        {
            for (int y = coord.y - settings.loadDistance; y < coord.y + settings.viewDistanceInChunks; y++)
            {
                for (int z = coord.z - settings.loadDistance; z < coord.z + settings.loadDistance; z++)
                {
                    int3 location = new int3(x, y, z);
                    if (!chunks.ContainsKey(location))
                    {
                        worldData.LoadChunk(location);
                    }
                }
            }
        }
    }

    public ushort CheckForVoxel(int3 globalPosition)
    {
        VoxelState voxel = worldData.GetVoxel(globalPosition);
        if (voxel == null)
        {
            Debug.Log("Null block");
            return 0;
        }

        if (blockTypes[voxel.id].isSolid)
        {
            return voxel.id;
        }
        else
        {
            return 0;
        }
    }

    public VoxelState GetVoxelState(int3 globalPosition)
    {
        return worldData.GetVoxel(globalPosition);
    }

    public bool InUI
    {
        get { return inUI; }
        set
        {
            inUI = value;
            if (inUI)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    public ushort GetVoxel(int3 globalPosition)
    {
        /* IMMUTABLE PASS */

        // If outside word, return air.
        /*        if (!WorldHelper.IsInRange(pos, Constants.WorldSizeInVoxels))
                {
                    return 0;
                }*/

        int solidGroundHeight = 0;
        float sumOfHeights = 0;
        int count = 0;
        float strongestWeight = 0;
        int strongestBiomeIndex = 0;

        for (int i = 0; i < biomes.Length; i++)
        {
            float weight = Noise.Get2DSimplex(new Vector2(globalPosition.x, globalPosition.z), biomes[i].offset, biomes[i].scale);

            // Keep track of which weight is strongest.
            if (weight > strongestWeight)
            {
                strongestWeight = weight;
                strongestBiomeIndex = i;
            }

            // Get the height of the terrain (for the current biome) and multiply it by its weight.
            float height = biomes[i].terrainHeight * Noise.Get2DSimplex(new Vector2(globalPosition.x, globalPosition.z), 2 * biomes[i].offset, biomes[i].terrainScale) * weight;

            // If the height value is greater 0 add it to the sum of heights.
            if (height > 0)
            {
                sumOfHeights += height;
                count++;
            }
        }

        // Set biome to the one with the strongest weight.
        BiomeAttributes biome = biomes[strongestBiomeIndex];

        // Get the average of the heights.
        if (count == 0)
        {
            count = 1;
        }
        sumOfHeights /= count;

        int terrainHeight = Mathf.FloorToInt(sumOfHeights + solidGroundHeight);
        ushort voxelValue = 0;

        SurfaceBlocks(ref voxelValue, globalPosition, biome, terrainHeight);
        LodeGeneration(ref voxelValue, globalPosition, biome);
        FloraGeneration(globalPosition, biome, terrainHeight);

        return voxelValue;
    }

    private void SurfaceBlocks(ref ushort voxelValue, int3 globalPosition, BiomeAttributes biome, int terrainHeight)
    {
        if (globalPosition.y == terrainHeight)
        {
            voxelValue = biome.surfaceBlock;
        }
        else if (globalPosition.y < terrainHeight && globalPosition.y > terrainHeight - 4)
        {
            voxelValue = biome.subSurfaceBlock;
        }
        else if (globalPosition.y > terrainHeight)
        {
            voxelValue = 0;
        }
        else
        {
            voxelValue = 3;
        }
    }

    private void LodeGeneration(ref ushort voxelValue, int3 globalPosition, BiomeAttributes biome)
    {
        if (voxelValue == 3 || voxelValue == 1)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (globalPosition.y > lode.minHeight && globalPosition.y < lode.maxHeight)
                {
                    if (Noise.Get3DSimplex(globalPosition, lode.noiseOffset, lode.scale) > lode.threshold)
                    {
                        voxelValue = lode.blockID;
                    }
                }
            }
        }
    }

    private void FloraGeneration(int3 globalPosition, BiomeAttributes biome, int terrainHeight)
    {
        if (globalPosition.y == terrainHeight && biome.placeMajorFlora)
        {
            if (Noise.Get2DSimplex(new Vector2(globalPosition.x, globalPosition.z), 200, biome.majorFloraZoneScale) > biome.majorFloraZoneThreshold)
            {
                if (Noise.Get2DSimplex(new Vector2(globalPosition.x, globalPosition.z), 700, biome.majorFloraPlacementScale) > biome.majorFloraPlacementThreshold)
                {
                    modifications.Enqueue(Structure.GenerateMajorFlora(biome.majorFloraIndex, globalPosition, biome.minHeight, biome.maxHeight));
                }
            }
        }
    }
}