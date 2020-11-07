using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VertData
{
    public Vector3 position;
    public Vector2 uv;

    public VertData(Vector3 position, Vector2 uv)
    {
        this.position = position;
        this.uv = uv;
    }
}