using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class Projectile : MonoBehaviour
{
    [SerializeField] private Vector3 directionVector;
    private SphericalRigidbody rbody;

    public int3 projectileChunkCoord;
    private int3 projectileLastChunkCoord;

    [SerializeField] private float speed = 1;
    [SerializeField] private float bounceCoolDown = 0.3f;
    [SerializeField] private bool bouncing = false;
    [SerializeField] private int bounces = 3;

    private void Awake()
    {
        rbody = GetComponent<SphericalRigidbody>();
        projectileLastChunkCoord = WorldHelper.GetChunkCoordFromPosition(this.transform.position);
    }

    public void Fire(Vector3 directionVector, float speed, int bounceCount)
    {
        this.directionVector = directionVector.normalized;
        this.speed = speed;
        this.bounces = bounceCount;
    }

    private void Update()
    {
        projectileChunkCoord = WorldHelper.GetChunkCoordFromPosition(this.transform.position);

        if (!projectileChunkCoord.Equals(projectileLastChunkCoord))
        {
            if (!RimecraftWorld.Instance.chunks.ContainsKey(projectileChunkCoord))
            {
                Destroy(this.gameObject);
            }
            else
            {
                projectileLastChunkCoord = projectileChunkCoord;
            }
        }

        if (rbody.colliding && bounces > 0)
        {
            if (!bouncing)
            {
                int3 breakBlock = rbody.lastCollidedWithBlockLocation;
                WorldHelper.GetChunkFromPosition(breakBlock).EditVoxel(breakBlock, 0);
                CalculateReflection();
                StartCoroutine(Bouncing());
                bounces--;
            }
        }

        rbody.CalculateVelocity(directionVector, speed);

        if (bounces == 0)
        {
            Destroy(this.gameObject);
        }
    }

    private void CalculateReflection()
    {
        rbody.UpCollision();
        rbody.DownCollision();
        Vector3 normal = rbody.collisionNormal;

        directionVector -= (2 * Vector3.Dot(directionVector, normal) * normal);
    }

    private IEnumerator Bouncing()
    {
        bouncing = true;
        yield return new WaitForSeconds(bounceCoolDown);
        bouncing = false;
    }
}