using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class Type3Extensions
{
    public static int3 FloorToInt3(this Vector3 position)
    {
        return new int3(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), Mathf.FloorToInt(position.z));
    }
}