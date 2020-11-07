using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Lode
{
    public string nodeName;
    public ushort blockID;
    public int minHeight = 5;
    public int maxHeight = 0;
    public float scale;
    public float threshold;
    public float noiseOffset;
}