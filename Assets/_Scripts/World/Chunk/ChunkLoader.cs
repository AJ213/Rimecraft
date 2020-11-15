using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

public class ChunkLoader : MonoBehaviour
{
    public bool JobsEnabled = false;

    private Transform player;
    private int3 playerChunkCoord;
    private int3 playerLastChunkCoord;

    public NativeHashSet<int3> activeChunks;

    private void Start()
    {
        activeChunks = new NativeHashSet<int3>(10, Allocator.Persistent);
        player = GameObject.FindGameObjectWithTag("Player").transform;
        player.position = new Vector3(0, 5, 0);
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
    }

    public void OnDisable()
    {
        activeChunks.Dispose();
    }

    private void CheckViewDistance()
    {
        int3 coord = WorldHelper.GetChunkCoordFromPosition(player.position);
        playerLastChunkCoord = playerChunkCoord;

        NativeHashSet<int3> previouslyActiveChunks = new NativeHashSet<int3>(100, Allocator.Temp);
        foreach (int3 c in activeChunks)
        {
            previouslyActiveChunks.Add(c);
        }

        activeChunks.Clear();

        int viewDistance = RimecraftWorld.Instance.settings.viewDistance;
        int3 minimum = new int3(coord.x - viewDistance, coord.y - viewDistance, coord.z - viewDistance);
        int3 maximum = new int3(coord.x + viewDistance, coord.y + viewDistance, coord.z + viewDistance);

        // Uses an alternating sequence. Starts at the center of the player and moves out step by step.
        int3 location;
        for (int x = minimum.x; x < maximum.x; x++)
        {
            for (int y = minimum.y; y < maximum.y; y++)
            {
                for (int z = minimum.z; z < maximum.z; z++)
                {
                    location.x = x;
                    location.y = y;
                    location.z = z;
                    if (!RimecraftWorld.chunkMeshes.ContainsKey(location))
                    {
                        RimecraftWorld.chunkMeshes[location] = new ChunkMeshData(location);
                    }

                    activeChunks.Add(location);

                    previouslyActiveChunks.Remove(location);
                }
            }
        }
        if (JobsEnabled)
        {
            NativeArray<int3> locations = activeChunks.ToNativeArray(Allocator.TempJob);
            var job = new MeshJob()
            {
                positions = locations,
            };
            job.Schedule(locations.Length, 1);
            JobHandle.ScheduleBatchedJobs();
        }
        else
        {
            foreach (int3 c in activeChunks)
            {
                if (!RimecraftWorld.chunkMeshes[c].meshGenerated)
                {
                    RimecraftWorld.chunkMeshes[c].UpdateMesh();
                }
            }
        }

        foreach (int3 c in activeChunks)
        {
            if (!RimecraftWorld.chunkMeshes[c].meshGenerated)
            {
                RimecraftWorld.chunkMeshes[c].CreateMesh();
            }
        }

        // Any chunks left in the previousActiveChunks list are no longer in the player's view distance, so loop through and disable them.
        foreach (int3 c in previouslyActiveChunks)
        {
            Destroy(RimecraftWorld.chunkObjects[c].gameObject);
            RimecraftWorld.chunkObjects.Remove(c);
            RimecraftWorld.chunkMeshes[c].DisableMesh();
        }
        previouslyActiveChunks.Dispose();
    }

    internal struct MeshJob : IJobParallelFor
    {
        [DeallocateOnJobCompletion] public NativeArray<int3> positions;

        public void Execute(int i)
        {
            if (!RimecraftWorld.chunkMeshes[positions[i]].meshGenerated && RimecraftWorld.chunkMeshes[positions[i]].instantiated)
            {
                RimecraftWorld.chunkMeshes[positions[i]].UpdateMesh();
            }
        }
    }

    private void CheckLoadDistance()
    {
        int3 coord = WorldHelper.GetChunkCoordFromPosition(player.position);
        playerLastChunkCoord = playerChunkCoord;

        // This is our loadDistance * 2 cubed. Shouldn't ever be bigger than this size for the array
        int loadDistance = RimecraftWorld.Instance.settings.loadDistance;
        int size = 8 * loadDistance * loadDistance * loadDistance;
        NativeArray<int3> positions = new NativeArray<int3>(size, Allocator.Persistent);
        int usageCount = 0;
        bool newChunks = false;

        for (int x = coord.x - loadDistance; x < coord.x + loadDistance; x++)
        {
            for (int y = coord.y - loadDistance; y < coord.y + loadDistance; y++)
            {
                for (int z = coord.z - loadDistance; z < coord.z + loadDistance; z++)
                {
                    int3 location = new int3(x, y, z);
                    if (!WorldData.chunks.ContainsKey(location))
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
                job.Schedule(usageCount, 1);
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
}