using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ElipsoidRigidbody : MonoBehaviour
{
    [SerializeField] private float objectWidth = 0.25f;
    [SerializeField] private float objectHeight = 1.6f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private bool usesGravity = false;
    [SerializeField] private Vector3 velocity;

    public float VerticalMomentum { get; set; }
    public bool IsGrounded { get; set; }

    public bool Back
    {
        get
        {
            int3 position = new int3(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z - (objectWidth - velocity.z)));
            return RimecraftWorld.Instance.CheckForVoxel(position) != 0 || RimecraftWorld.Instance.CheckForVoxel(position + new int3(0, 1, 0)) != 0;
        }
    }

    public bool Front
    {
        get
        {
            int3 position = new int3(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z + (objectWidth + velocity.z)));
            return RimecraftWorld.Instance.CheckForVoxel(position) != 0 || RimecraftWorld.Instance.CheckForVoxel(position + new int3(0, 1, 0)) != 0;
        }
    }

    public bool Left
    {
        get
        {
            int3 position = new int3(Mathf.FloorToInt(transform.position.x - (objectWidth - velocity.x)), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z));
            return RimecraftWorld.Instance.CheckForVoxel(position) != 0 || RimecraftWorld.Instance.CheckForVoxel(position + new int3(0, 1, 0)) != 0;
        }
    }

    public bool Right
    {
        get
        {
            int3 position = new int3(Mathf.FloorToInt(transform.position.x + (objectWidth + velocity.x)), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z));
            return RimecraftWorld.Instance.CheckForVoxel(position) != 0 || RimecraftWorld.Instance.CheckForVoxel(position + new int3(0, 1, 0)) != 0;
        }
    }

    public void CalculateVelocity(float horizontal, float vertical, float speed)
    {
        // Affect verical momentum with gravity.
        if (VerticalMomentum > gravity && usesGravity)
        {
            VerticalMomentum += Time.fixedDeltaTime * gravity;
        }

        // if we're sprinting, use the sprint multiplier.

        velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * speed;

        // Apply vertical momentum (falling/jumping).
        velocity += Vector3.up * VerticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && Front) || (velocity.z < 0 && Back))
        {
            velocity.z = 0;
        }

        if ((velocity.x > 0 && Right) || (velocity.x < 0 && Left))
        {
            velocity.x = 0;
        }

        if (velocity.y < 0)
        {
            velocity.y = CheckDownSpeed(velocity.y);
        }
        else if (velocity.y > 0)
        {
            velocity.y = CheckUpSpeed(velocity.y);
        }
    }

    private int3 ObjectWidthBlockLocations(int index, float verticalOffset)
    {
        // Grabs the top right position block relative to object
        float widthAdjustment = (objectWidth);
        if (index == 0)
        {
            return new int3(new float3(transform.position.x - widthAdjustment, transform.position.y + verticalOffset, transform.position.z - widthAdjustment));
        }
        else if (index == 1)
        {
            return new int3(new float3(transform.position.x + widthAdjustment, transform.position.y + verticalOffset, transform.position.z - widthAdjustment));
        }
        else if (index == 2)
        {
            return new int3(new float3(transform.position.x + widthAdjustment, transform.position.y + verticalOffset, transform.position.z + widthAdjustment));
        }
        else
        {
            return new int3(new float3(transform.position.x - widthAdjustment, transform.position.y + verticalOffset, transform.position.z + widthAdjustment));
        }
    }

    private bool ObjectObstructedVerticallyAt(float height)
    {
        return ((RimecraftWorld.Instance.CheckForVoxel(ObjectWidthBlockLocations(0, height)) != 0) ||
            RimecraftWorld.Instance.CheckForVoxel(ObjectWidthBlockLocations(1, height)) != 0 ||
            RimecraftWorld.Instance.CheckForVoxel(ObjectWidthBlockLocations(2, height)) != 0 ||
            RimecraftWorld.Instance.CheckForVoxel(ObjectWidthBlockLocations(3, height)) != 0);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, objectWidth);
    }

    private float CheckDownSpeed(float downSpeed)
    {
        if (ObjectObstructedVerticallyAt(downSpeed))
        {
            IsGrounded = true;
            VerticalMomentum = 0;
            return 0;
        }
        else
        {
            IsGrounded = false;
            return downSpeed;
        }
    }

    private float CheckUpSpeed(float upSpeed)
    {
        if (ObjectObstructedVerticallyAt(upSpeed + objectHeight))
        {
            VerticalMomentum = 0;
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }

    private void FixedUpdate()
    {
        if (!RimecraftWorld.Instance.InUI)
        {
            transform.Translate(velocity, Space.World);
        }
    }
}