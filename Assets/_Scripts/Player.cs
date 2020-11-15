﻿using Unity.Mathematics;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Moving Mouse

    private float mouseHorizontal;
    private float mouseVertical;

    // Placing Blocks

    [SerializeField] private float checkIncrement = 0.1f;
    [SerializeField] private float reach = 8;
    [SerializeField] private Transform highlightBlock = null;
    [SerializeField] private Transform placeBlock = null;

    // Jumping and falling

    private ElipsoidRigidbody rbody;
    [SerializeField] private float jumpForce = 5;
    private bool jumpRequest;

    // Moving

    [SerializeField] private bool isSprinting = false;
    [SerializeField] private float sprintSpeed = 6;
    [SerializeField] private float walkSpeed = 3;
    private float horizontal;
    private float vertical;

    // Misc

    private Transform cam;
    [SerializeField] private Toolbar toolbar = null;

    private void FixedUpdate()
    {
        if (!RimecraftWorld.Instance.InUI)
        {
            if (jumpRequest)
            {
                Jump();
            }
            if (isSprinting)
            {
                rbody.CalculateVelocity(horizontal, vertical, sprintSpeed);
            }
            else
            {
                rbody.CalculateVelocity(horizontal, vertical, walkSpeed);
            }

            transform.Rotate(Vector3.up * mouseHorizontal * RimecraftWorld.Instance.settings.mouseSensitivity);
            cam.Rotate(Vector3.right * -mouseVertical * RimecraftWorld.Instance.settings.mouseSensitivity);
        }
    }

    private void GetPlayerInputs()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint"))
        {
            isSprinting = true;
        }
        if (Input.GetButtonUp("Sprint"))
        {
            isSprinting = false;
        }

        if (rbody.IsGrounded && Input.GetButtonDown("Jump"))
        {
            jumpRequest = true;
        }

        if (highlightBlock.gameObject.activeSelf)
        {
            // Destroy Block
            if (Input.GetMouseButtonDown(0))
            {
                WorldData.chunks.TryGetValue(WorldHelper.GetChunkCoordFromPosition(highlightBlock.position), out ChunkData value);
                value.ModifyVoxel(WorldHelper.GetVoxelLocalPositionInChunk(new int3(Mathf.FloorToInt(highlightBlock.position.x),
                    Mathf.FloorToInt(highlightBlock.position.y), Mathf.FloorToInt(highlightBlock.position.z))),
                0);
            }

            // Build Block
            if (Input.GetMouseButtonDown(1))
            {
                if (toolbar.slots[toolbar.slotIndex].HasItem)
                {
                    WorldData.chunks.TryGetValue(WorldHelper.GetChunkCoordFromPosition(placeBlock.position), out ChunkData value);
                    value.ModifyVoxel(WorldHelper.GetVoxelLocalPositionInChunk(new int3(Mathf.FloorToInt(placeBlock.position.x),
                        Mathf.FloorToInt(placeBlock.position.y), Mathf.FloorToInt(placeBlock.position.z))),
                    toolbar.slots[toolbar.slotIndex].itemSlot.stack.id);
                    toolbar.slots[toolbar.slotIndex].itemSlot.Take(1);
                }
            }
        }
    }

    private void Jump()
    {
        rbody.VerticalMomentum = jumpForce;
        rbody.IsGrounded = false;
        jumpRequest = false;
    }

    private void PlaceCursorBlock()
    {
        float step = checkIncrement;
        float3 lastPos = new Vector3();

        while (step < reach)
        {
            float3 position = cam.position + (cam.forward * step);
            int3 roundedPosition = new int3(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), Mathf.FloorToInt(position.z));

            if (RimecraftWorld.Instance.CheckForVoxel(roundedPosition) != 0)
            {
                highlightBlock.position = new float3(roundedPosition);
                placeBlock.position = lastPos;

                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);

                return;
            }

            lastPos = new float3(roundedPosition);

            step += checkIncrement;
        }

        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);
    }

    private void Start()
    {
        rbody = this.gameObject.GetComponent<ElipsoidRigidbody>();
        cam = Camera.main.transform;
        RimecraftWorld.Instance.InUI = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RimecraftWorld.Instance.InUI = !RimecraftWorld.Instance.InUI;
        }

        if (!RimecraftWorld.Instance.InUI)
        {
            GetPlayerInputs();
            PlaceCursorBlock();
        }
    }
}