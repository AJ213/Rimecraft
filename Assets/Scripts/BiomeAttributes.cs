using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttirbutes", menuName = "Rimecraft/Biome Attributes")]
public class BiomeAttributes : ScriptableObject
{
    [Header("Scale")]
    public string biomeName;

    public int offset;
    public float scale;

    public int terrainheight;
    public float terrainScale;

    public ushort surfaceBlock;
    public ushort subSurfaceBlock;

    [Header("Major Flora")]
    public float majorFloraZoneScale = 1.3f;

    public int majorFloraIndex;

    [Range(0.1f, 1f)]
    public float majorFloraZoneThreshold = 0.6f;

    public float majorFloraPlacementScale = 15f;

    [Range(0.1f, 1f)]
    public float majorFloraPlacementThreshold = 0.6f;

    public bool placeMajorFlora = true;
    public int maxHeight = 12;
    public int minHeight = 5;

    public Lode[] lodes;
}

[System.Serializable]
public class Lode
{
    public string nodeName;
    public ushort blockID;
    public int minHeight = 5;
    public int maxHeight = VoxelData.WorldSizeInVoxels - 200;
    public float scale;
    public float threshold;
    public float noiseOffset;
}