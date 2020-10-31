using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Settings
{
    [Header("Game Data")]
    public string version = "0.1";

    [Header("Performance")]
    public int loadDistance = 16;

    public bool enableThreading = true;

    public int viewDistanceInChunks = 8;

    [Range(0.1f, 10f)]
    public float mouseSensitivity = 2;
}