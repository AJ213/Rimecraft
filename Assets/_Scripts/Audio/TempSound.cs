using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TempSound : MonoBehaviour
{
    private void Awake()
    {
        Destroy(this.gameObject, GetComponent<AudioSource>().clip.length);
    }
}