using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayAudioAtArea : MonoBehaviour
{
    private Transform player;
    private int3 lastLocation;
    private AudioManager audioManager;
    [SerializeField] private ParticleSystem snow = default;
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
            if (player.position.y < yValue)
            {
                MakeWindSilent(0);
                snow.Stop();
            }
            else if (!IsVoxelAboveOrBelow(5))
            {
                MakeWindSilent(minValue);
            }
            else
            {
                snow.Play();
                currentVolume = Mathf.Clamp(currentVolume + (volumeChangeSpeed * Time.deltaTime), minValue, maxVolume);
                if (currentVolume == maxVolume)
                {
                    lastLocation = player.position.FloorToInt3();
                }
            }
            wind.volume = currentVolume;
        }
    }

    private ParticleSystem.Particle[] particles;

    private void LateUpdate()
    {
        if (particles == null || particles.Length < snow.main.maxParticles)
            particles = new ParticleSystem.Particle[snow.main.maxParticles];

        int numAlive = snow.GetParticles(particles);
        for (int i = 0; i < particles.Length; i++)
        {
            VoxelState voxel = WorldHelper.GetVoxelFromPosition(particles[i].position);
            if (voxel == null || voxel.id != 0)
            {
                //particles[i].position = new Vector3(particles[i].position.x, snow.transform.position.y, particles[i].position.z);
                particles[i].remainingLifetime = 0;
            }
        }
        snow.SetParticles(particles, numAlive);
    }

    private void MakeWindSilent(float minVolume)
    {
        currentVolume = Mathf.Clamp(currentVolume - (volumeChangeSpeed * Time.deltaTime), minVolume, maxVolume);
        if (currentVolume == minVolume)
        {
            lastLocation = player.position.FloorToInt3();
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