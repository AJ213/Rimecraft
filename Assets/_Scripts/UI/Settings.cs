using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Settings
{
    [Header("Game Data")]
    public string version = "0.1";

    [Header("Performance")]
    public int viewDistance = 8;

    [Range(0.1f, 10f)]
    public float mouseSensitivity = 2;

    [Range(0.01f, 1f)]
    public float volume = 0.25f;
}