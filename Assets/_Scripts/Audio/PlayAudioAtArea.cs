using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayAudioAtArea : MonoBehaviour
{
    private Transform player;
    private int3 lastLocation;
    private AudioManager audioManager;
    [SerializeField] private float maxVolume = 0.7f;
    [SerializeField] private float minValue = 0;
    [SerializeField] private float volumeChangeSpeed = 0.3f;
    [SerializeField] private int yValue = -10;

    private float currentVolume = 1;
    private AudioSource wind;

    private void Start()
    {
        player = this.transform;
        audioManager = GetComponent<AudioManager>();
        audioManager.Play("Wind");
        wind = audioManager.GetAudioSource("Wind");
        Mathf.Clamp(currentVolume, minValue, maxVolume);
        wind.volume = maxVolume;
    }

    private void Update()
    {
        if (!player.Equals(lastLocation))
        {
            if (!IsVoxelAboveOrBelow(5) || player.position.y < yValue)
            {
                currentVolume = Mathf.Clamp(currentVolume - (volumeChangeSpeed * Time.deltaTime), minValue, maxVolume);
                if (currentVolume == minValue)
                {
                    lastLocation = player.position.FloorToInt3();
                }
            }
            else
            {
                currentVolume = Mathf.Clamp(currentVolume + (volumeChangeSpeed * Time.deltaTime), minValue, maxVolume);
                if (currentVolume == maxVolume)
                {
                    lastLocation = player.position.FloorToInt3();
                }
            }
            wind.volume = currentVolume;
        }
    }

    private bool IsVoxelAboveOrBelow(int height)
    {
        for (int i = 1; i <= height; i++)
        {
            if (RimecraftWorld.Instance.CheckForVoxel((player.position + (Vector3.up * i)).FloorToInt3()) != 0)
            {
                return false;
            }
        }
        return true;
    }
}