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
    [SerializeField] private GameObject[] sounds = default;

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
                // Ice, Snow, Stone
                VoxelState voxel = WorldHelper.GetVoxelFromPosition(breakBlock);
                ushort blockBreakingID = 0;
                if (voxel != null)
                {
                    blockBreakingID = WorldHelper.GetVoxelFromPosition(breakBlock).id;
                }

                Chunk chunk = WorldHelper.GetChunkFromPosition(breakBlock);
                if (chunk != null)
                {
                    chunk.EditVoxel(breakBlock, 0);
                }

                if (blockBreakingID == 2)
                {
                    Instantiate(sounds[0], this.transform.position, Quaternion.identity);
                }
                else if (blockBreakingID == 1)
                {
                    Instantiate(sounds[1], this.transform.position, Quaternion.identity);
                }
                else
                {
                    Instantiate(sounds[2], this.transform.position, Quaternion.identity);
                }

                if (blockBreakingID != 0)
                {
                    GameObject droppedBlock = (GameObject)Instantiate(Resources.Load("DroppedItem"), this.transform.position, Quaternion.identity);
                    droppedBlock.GetComponent<DropItem>().SetItemStack(blockBreakingID, 1);
                }

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