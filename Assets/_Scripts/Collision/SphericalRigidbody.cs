using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SphericalRigidbody : MonoBehaviour
{
    [SerializeField] private float objectRadius = 0.25f;
    [SerializeField] private Vector3 velocity;
    public int3 lastCollidedWithBlockLocation;
    public bool colliding = false;
    [SerializeField] private bool usesGravity = false;
    public Vector3 collisionNormal = Vector3.zero;
    public float VerticalMomentum { get; set; }

    private bool Colliding(int3 position)
    {
        if (RimecraftWorld.Instance.CheckForVoxel(position) != 0)
        {
            lastCollidedWithBlockLocation = position;
        }
        bool result = RimecraftWorld.Instance.CheckForVoxel(position) == 0;

        return !result;
    }

    public bool BackCollision()
    {
        int3 position = new int3(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z - (objectRadius - velocity.z)));

        bool result = Colliding(position);
        if (result)
        {
            collisionNormal = Vector3.forward;
        }
        return result;
    }

    public bool FrontCollision()
    {
        int3 position = new int3(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z + (objectRadius + velocity.z)));
        bool result = Colliding(position);
        if (result)
        {
            collisionNormal = Vector3.back;
        }
        return result;
    }

    public bool LeftCollision()
    {
        int3 position = new int3(Mathf.FloorToInt(transform.position.x - (objectRadius - velocity.x)), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z));
        bool result = Colliding(position);
        if (result)
        {
            collisionNormal = Vector3.right;
        }
        return result;
    }

    public bool RightCollision()
    {
        int3 position = new int3(Mathf.FloorToInt(transform.position.x + (objectRadius + velocity.x)), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z));
        bool result = Colliding(position);
        if (result)
        {
            collisionNormal = Vector3.left;
        }
        return result;
    }

    public bool UpCollision()
    {
        int3 position = new int3(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y + (objectRadius + velocity.y)), Mathf.FloorToInt(transform.position.z));
        bool result = Colliding(position);
        if (result)
        {
            collisionNormal = Vector3.down;
        }
        return result;
    }

    public bool DownCollision()
    {
        int3 position = new int3(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y - (objectRadius - velocity.y)), Mathf.FloorToInt(transform.position.z));
        bool result = Colliding(position);
        if (result)
        {
            collisionNormal = Vector3.up;
        }
        return result;
    }

    public void CalculateVelocity(Vector3 directionVect, float speed)
    {
        // Affect verical momentum with gravity.
        if (usesGravity)
        {
            VerticalMomentum += Time.fixedDeltaTime * -9.81f;
        }

        velocity = directionVect * Time.fixedDeltaTime * speed;

        // Apply vertical momentum (falling).
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

        if ((velocity.y > 0 && UpCollision()) || (velocity.y < 0 && DownCollision()))
        {
            velocity.y = 0;
        }

        colliding = (velocity.x == 0 || velocity.y == 0 || velocity.z == 0);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, objectRadius);
    }

    private void FixedUpdate()
    {
        if (!RimecraftWorld.Instance.InUI)
        {
            transform.Translate(velocity, Space.World);
        }
    }
}