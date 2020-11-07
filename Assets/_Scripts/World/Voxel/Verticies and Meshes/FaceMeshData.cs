using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FaceMeshData
{
    public string direction;
    public Vector3 normal;
    public VertData[] vertData;
    public int[] triangles;
}