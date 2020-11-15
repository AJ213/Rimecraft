using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using System.Collections.Concurrent;

public class RimecraftWorld : MonoBehaviour
{
    public Settings settings;
    public BiomeAttributes[] biomes;

    public Vector3 spawnPosition;

    public Material material = null;
    public BlockType[] blockTypes = null;

    [HideInInspector]
    public static ConcurrentDictionary<int3, ChunkMeshData> chunkMeshes = new ConcurrentDictionary<int3, ChunkMeshData>();

    [HideInInspector]
    public static Dictionary<int3, GameObject> chunkObjects = new Dictionary<int3, GameObject>();

    private bool applyingModifications = false;

    public static ConcurrentQueue<ConcurrentQueue<VoxelMod>> modifications = new ConcurrentQueue<ConcurrentQueue<VoxelMod>>();

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
        Debug.Log("Generating new world using seed " + VoxelInfo.seed);

        //worldData = SaveSystem.LoadWorld("Prototype", VoxelInfo.seed);

        UnityEngine.Random.InitState(VoxelInfo.seed);
        Camera.main.farClipPlane = Mathf.Sqrt(2) * Constants.ChunkSizeX * 2 * settings.viewDistance;

        spawnPosition = new Vector3(0, 5, 0);
    }

    private void Update()
    {
        if (!applyingModifications)
        {
            //ApplyModifications();
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }
    }

    public void OnDisable()
    {
        //NativeArray<int3> keys = chunkMeshes.GetKeyArray(Allocator.Temp);
        /*foreach (int3 c in chunkMeshes.Keys)
        {
            chunkMeshes.TryGetValue(c, out ChunkMeshData item);
            item.DisposeData();
        }*/
        //keys.Dispose();
        //chunkMeshes.Dispose();
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
                WorldData.SetVoxel(v.position, v.id);
            }
        }
        applyingModifications = false;
    }

    public ushort CheckForVoxel(int3 globalPosition)
    {
        VoxelData voxel = WorldData.GetVoxel(globalPosition);
        if (voxel.id == 0 && voxel.position.Equals(new int3(0, 0, 0)))
        {
            /*            Debug.Log("Null block at " + globalPosition);*/
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
}