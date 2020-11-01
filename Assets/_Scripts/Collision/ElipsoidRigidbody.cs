using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElipsoidRigidbody : MonoBehaviour
{
    [SerializeField] private float objectWidth = 0.25f;
    [SerializeField] private float objectHeight = 1.6f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private bool usesGravity = false;
    [SerializeField] private Vector3 velocity;

    public float VerticalMomentum { get; set; }
    public bool IsGrounded { get; private set; }

    public bool Back
    {
        get
        {
            Vector3Int position = new Vector3Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z - objectWidth));
            return World.Instance.CheckForVoxel(position) != 0 || World.Instance.CheckForVoxel(position + Vector3Int.up) != 0;
        }
    }

    public bool Front
    {
        get
        {
            Vector3Int position = new Vector3Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z + objectWidth));
            return World.Instance.CheckForVoxel(position) != 0 || World.Instance.CheckForVoxel(position + Vector3Int.up) != 0;
        }
    }

    public bool Left
    {
        get
        {
            Vector3Int position = new Vector3Int(Mathf.FloorToInt(transform.position.x - objectWidth), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z));
            return World.Instance.CheckForVoxel(position) != 0 || World.Instance.CheckForVoxel(position + Vector3Int.up) != 0;
        }
    }

    public bool Right
    {
        get
        {
            Vector3Int position = new Vector3Int(Mathf.FloorToInt(transform.position.x + objectWidth), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z));
            return World.Instance.CheckForVoxel(position) != 0 || World.Instance.CheckForVoxel(position + Vector3Int.up) != 0;
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

    private Vector3Int ObjectWidthBlockLocations(int index, float verticalOffset)
    {
        // Grabs the top right position block relative to object
        if (index == 0)
        {
            return Vector3Int.FloorToInt(new Vector3(transform.position.x - objectWidth, transform.position.y + verticalOffset, transform.position.z - objectWidth));
        }
        else if (index == 1)
        {
            return Vector3Int.FloorToInt(new Vector3(transform.position.x + objectWidth, transform.position.y + verticalOffset, transform.position.z - objectWidth));
        }
        else if (index == 2)
        {
            return Vector3Int.FloorToInt(new Vector3(transform.position.x + objectWidth, transform.position.y + verticalOffset, transform.position.z + objectWidth));
        }
        else
        {
            return Vector3Int.FloorToInt(new Vector3(transform.position.x - objectWidth, transform.position.y + verticalOffset, transform.position.z + objectWidth));
        }
    }

    private bool ObjectObstructedVerticallyAt(float height)
    {
        return (World.Instance.CheckForVoxel(ObjectWidthBlockLocations(0, height)) != 0 ||
            World.Instance.CheckForVoxel(ObjectWidthBlockLocations(1, height)) != 0 ||
            World.Instance.CheckForVoxel(ObjectWidthBlockLocations(2, height)) != 0 ||
            World.Instance.CheckForVoxel(ObjectWidthBlockLocations(3, height)) != 0);
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
        if (!World.Instance.InUI)
        {
            transform.Translate(velocity, Space.World);
        }
    }
}