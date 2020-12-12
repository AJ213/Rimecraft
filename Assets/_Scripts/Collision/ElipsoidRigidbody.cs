using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ElipsoidRigidbody : MonoBehaviour
{
    [SerializeField] private float objectWidth = 0.25f;
    public float objectHeight = 1.6f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private bool usesGravity = false;
    [SerializeField] private Vector3 velocity;

    public float VerticalMomentum { get; set; }
    public bool IsGrounded { get; set; }

    private bool Colliding(int3 position)
    {
        bool result = RimecraftWorld.Instance.CheckForVoxel(position) == 0;
        for (int i = 1; i <= (int)objectHeight; i++)
        {
            result &= RimecraftWorld.Instance.CheckForVoxel(position + new int3(0, i, 0)) == 0;
        }
        return !result;
    }

    public bool BackCollision()
    {
        int3 position = new int3(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z - (objectWidth - velocity.z)));
        return Colliding(position);
    }

    public bool FrontCollision()
    {
        int3 position = new int3(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z + (objectWidth + velocity.z)));
        return Colliding(position);
    }

    public bool LeftCollision()
    {
        int3 position = new int3(Mathf.FloorToInt(transform.position.x - (objectWidth - velocity.x)), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z));
        return Colliding(position);
    }

    public bool RightCollision()
    {
        int3 position = new int3(Mathf.FloorToInt(transform.position.x + (objectWidth + velocity.x)), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z));
        return Colliding(position);
    }

    public void CalculateVelocity(float horizontal, float vertical, float speed)
    {
        // Affect verical momentum with gravity.
        if (usesGravity)
        {
            VerticalMomentum += Time.fixedDeltaTime * gravity;
        }

        // if we're sprinting, use the sprint multiplier.

        velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * speed;

        // Apply vertical momentum (falling/jumping).
        if (usesGravity)
        {
            velocity += Vector3.up * VerticalMomentum * Time.fixedDeltaTime;
        }

        if ((velocity.z > 0 && FrontCollision()) || (velocity.z < 0 && BackCollision()))
        {
            velocity.z = 0;
        }

        if ((velocity.x > 0 && RightCollision()) || (velocity.x < 0 && LeftCollision()))
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
            return new int3(Mathf.FloorToInt(transform.position.x - widthAdjustment), Mathf.FloorToInt(transform.position.y + verticalOffset), Mathf.FloorToInt(transform.position.z - widthAdjustment));
        }
        else if (index == 1)
        {
            return new int3(Mathf.FloorToInt(transform.position.x + widthAdjustment), Mathf.FloorToInt(transform.position.y + verticalOffset), Mathf.FloorToInt(transform.position.z - widthAdjustment));
        }
        else if (index == 2)
        {
            return new int3(Mathf.FloorToInt(transform.position.x + widthAdjustment), Mathf.FloorToInt(transform.position.y + verticalOffset), Mathf.FloorToInt(transform.position.z + widthAdjustment));
        }
        else
        {
            return new int3(Mathf.FloorToInt(transform.position.x - widthAdjustment), Mathf.FloorToInt(transform.position.y + verticalOffset), Mathf.FloorToInt(transform.position.z + widthAdjustment));
        }
    }

    public bool ObjectObstructedVerticallyAt(float height)
    {
        return !(RimecraftWorld.Instance.CheckForVoxel(ObjectWidthBlockLocations(0, height)) == 0 &&
            RimecraftWorld.Instance.CheckForVoxel(ObjectWidthBlockLocations(1, height)) == 0 &&
            RimecraftWorld.Instance.CheckForVoxel(ObjectWidthBlockLocations(2, height)) == 0 &&
            RimecraftWorld.Instance.CheckForVoxel(ObjectWidthBlockLocations(3, height)) == 0);
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