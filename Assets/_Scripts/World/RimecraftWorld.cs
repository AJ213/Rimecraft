using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;
using System.Collections.Concurrent;
using System.Configuration;

public class RimecraftWorld : MonoBehaviour
{
    public static Settings settings;
    public BiomeAttributes[] biomes;

    public bool JobsEnabled = false;

    public Transform player;
    public Vector3 spawnPosition;

    public Material material = null;
    public Material transparentMaterial = null;
    public Material shinyMaterial = null;
    public BlockType[] blockTypes = null;

    [HideInInspector]
    public Dictionary<int3, Chunk> chunks = new Dictionary<int3, Chunk>();

    private HashSet<int3> activeChunks = new HashSet<int3>();

    public int3 playerChunkCoord;
    private int3 playerLastChunkCoord;

    private List<Chunk> chunksToUpdate = new List<Chunk>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();

    private bool applyingModifications = false;

    private static ConcurrentQueue<ConcurrentQueue<VoxelMod>> modifications = new ConcurrentQueue<ConcurrentQueue<VoxelMod>>();

    private bool inUI = false;

    public GameObject debugScreen;

    private static RimecraftWorld instance;
    public static RimecraftWorld Instance => instance;

    public static WorldData worldData;

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
    }

    private void Start()
    {
        worldData = new WorldData("null", WorldData.seed);
        //Debug.Log("Generating new world using seed " + WorldData.seed);

        //worldData = SaveSystem.LoadWorld(WorldData.worldName, WorldData.seed);
        if (settings == null)
        {
            settings = new Settings();
            settings.viewDistance = 2;
            settings.mouseSensitivity = 2;
        }

        UnityEngine.Random.InitState(WorldData.seed);
        Camera.main.farClipPlane = Mathf.Sqrt(2) * Constants.ChunkSizeX * 2 * settings.viewDistance;

        spawnPosition = new Vector3(0, 5, 0);
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
            modifications.TryDequeue(out ConcurrentQueue<VoxelMod> queue);
            while (queue.Count > 0)
            {
                queue.TryDequeue(out VoxelMod v);
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

        int3 minimum = new int3(coord.x - settings.viewDistance, coord.y - settings.viewDistance, coord.z - settings.viewDistance);
        int3 maximum = new int3(coord.x + settings.viewDistance, coord.y + settings.viewDistance, coord.z + settings.viewDistance);

        // Uses an alternating sequence. Starts at the center of the player and moves out step by step.
        for (int x = coord.x, counterx = 1; x >= minimum.x && x < maximum.x; x += counterx * (int)math.pow(-1, counterx - 1), counterx++)
        {
            for (int y = coord.y, countery = 1; y >= minimum.y && y < maximum.y; y += countery * (int)math.pow(-1, countery - 1), countery++)
            {
                for (int z = coord.z, counterz = 1; z >= minimum.z && z < maximum.z; z += counterz * (int)math.pow(-1, counterz - 1), counterz++)
                {
                    int3 location = new int3(x, y, z);
                    if (!chunks.ContainsKey(location))
                    {
                        chunks[location] = new Chunk(location);
                    }

                    chunks[location].IsActive = true;
                    activeChunks.Add(location);

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

        // This is our loadDistance * 2 cubed. Shouldn't ever be bigger than this size for the array
        int size = 8 * settings.viewDistance * settings.viewDistance * settings.viewDistance;
        NativeArray<int3> positions = new NativeArray<int3>(size, Allocator.Persistent);
        int usageCount = 0;
        bool newChunks = false;

        for (int x = coord.x - settings.viewDistance; x < coord.x + settings.viewDistance; x++)
        {
            for (int y = coord.y - settings.viewDistance; y < coord.y + settings.viewDistance; y++)
            {
                for (int z = coord.z - settings.viewDistance; z < coord.z + settings.viewDistance; z++)
                {
                    int3 location = new int3(x, y, z);
                    if (!chunks.ContainsKey(location))
                    {
                        positions[usageCount] = location;
                        newChunks = true;
                        usageCount++;
                    }
                }
            }
        }

        if (JobsEnabled)
        {
            if (!newChunks)
            {
                // We don't want to bother loading jobs if there is nothing new to load
                positions.Dispose();
            }
            else
            {
                var job = new LoadJob()
                {
                    positions = positions,
                };
                job.Schedule(usageCount, 2);
                JobHandle.ScheduleBatchedJobs();
            }
        }
        else
        {
            for (int i = 0; i < usageCount; i++)
            {
                WorldData.LoadChunk(positions[i]);
            }
            positions.Dispose();
        }
    }

    internal struct LoadJob : IJobParallelFor
    {
        [DeallocateOnJobCompletion] public NativeArray<int3> positions;

        public void Execute(int i)
        {
            WorldData.LoadChunk(positions[i]);
        }
    }

    public ushort CheckForVoxel(int3 globalPosition)
    {
        VoxelState voxel = worldData.GetVoxel(globalPosition);
        if (voxel == null)
        {
            //Debug.Log("Null block at " + globalPosition);
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

    public static ushort SamplePosition(int3 globalPosition, BiomeAttributes[] biomes)
    {
        // Set biome to the one with the strongest weight.
        int terrainHeight = 0;
        BiomeAttributes mainBiome = biomes[0];
        for (int i = 0; i < biomes.Length; i++)
        {
            terrainHeight += Mathf.FloorToInt(biomes[i].terrainHeight * Noise.Get2DSimplex(new Vector2(globalPosition.x, globalPosition.z), 2 * biomes[i].offset, biomes[i].terrainScale));
        }
        terrainHeight /= biomes.Length;

        ushort voxelID = 0;

        SurfaceBlocks(ref voxelID, globalPosition, mainBiome, terrainHeight, 2, 15);
        LodeGeneration(ref voxelID, globalPosition, mainBiome);
        FloraGeneration(globalPosition, mainBiome, terrainHeight);

        return voxelID;
    }

    private static void SurfaceBlocks(ref ushort voxelID, int3 globalPosition, BiomeAttributes biome, int terrainHeight, int depthMultiplier, int cavernHeight)
    {
        int cavernDepthAtPosition = (int)(math.pow(4, math.ceil(math.log10(math.abs(globalPosition.y)) / math.log10(4))) / depthMultiplier);
        int cavernTerrainHeight = terrainHeight - cavernDepthAtPosition;
        int heightMultiplier = (int)math.max(1, math.log2(math.abs(globalPosition.y)) - 3);
        if (globalPosition.y == terrainHeight)
        {
            voxelID = biome.surfaceBlock;
        }
        else if (globalPosition.y < terrainHeight && globalPosition.y > terrainHeight - 4)
        {
            voxelID = biome.subSurfaceBlock;
        }
        else if (globalPosition.y > terrainHeight)
        {
            voxelID = 0;
        }
        else if (globalPosition.y > cavernTerrainHeight && globalPosition.y < cavernTerrainHeight + (cavernHeight * heightMultiplier))
        {
            voxelID = 0;
        }
        else
        {
            voxelID = 3;
        }
    }

    private static void LodeGeneration(ref ushort voxelID, int3 globalPosition, BiomeAttributes biome)
    {
        if (voxelID == 3 || voxelID == 1)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (globalPosition.y > lode.minHeight && globalPosition.y < lode.maxHeight)
                {
                    if (Noise.Get3DSimplex(globalPosition, lode.noiseOffset, lode.scale) > lode.threshold)
                    {
                        voxelID = lode.blockID;
                    }
                }
            }
        }
    }

    private static void FloraGeneration(int3 globalPosition, BiomeAttributes biome, int terrainHeight)
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