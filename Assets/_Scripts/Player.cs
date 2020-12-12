using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    // Moving Mouse

    private float mouseHorizontal;
    private float mouseVertical;

    // Placing Blocks

    [SerializeField] private float checkIncrement = 0.01f;
    [SerializeField] private float reach = 8;
    [SerializeField] private Transform highlightBlock = null;
    [SerializeField] private Vector3 placeBlockPosition;
    [SerializeField] private Transform weaponPosition;

    // Jumping and falling
    private ElipsoidRigidbody rbody;

    [SerializeField] private float footstepInterval = 0.4f;
    [SerializeField] private float footstepIntervalSprinting = 0.25f;

    [SerializeField] private float jumpForce = 5;
    private bool jumpRequest;
    public AudioManager playerSounds;

    // Moving
    [SerializeField] private bool isSprinting = false;

    [SerializeField] private float sprintSpeed = 6;
    [SerializeField] private float walkSpeed = 3;
    private float horizontal;
    private float vertical;

    // Misc

    private Transform cam;
    [SerializeField] private GameObject inventory = default;

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

            transform.Rotate(Vector3.up * mouseHorizontal * RimecraftWorld.settings.mouseSensitivity);
            cam.Rotate(Vector3.right * -mouseVertical * RimecraftWorld.settings.mouseSensitivity);
        }
    }

    private void GetPlayerInputs()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //SaveSystem.SaveWorld(RimecraftWorld.worldData);
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

        if (rbody.IsGrounded && (Mathf.Abs(horizontal) >= 0.1f || Mathf.Abs(vertical) >= 0.1f))
        {
            float footstepSpeed;
            if (isSprinting)
            {
                footstepSpeed = footstepIntervalSprinting;
            }
            else
            {
                footstepSpeed = footstepInterval;
            }
            this.GetComponent<FootstepsPlayer>().PlayFootstep(footstepSpeed);
        }

        // Destroy Block
        if (Input.GetMouseButtonDown(0))
        {
            playerSounds.Play("WeaponUse");
            GameObject projectile = (GameObject)Instantiate(Resources.Load("Mining Projectile"), Camera.main.transform.position + Camera.main.transform.forward, Quaternion.identity);
            projectile.GetComponent<Projectile>().Fire(Camera.main.transform.forward, 10, 3);
        }

        if (highlightBlock.gameObject.activeSelf)
        {
            // Build Block
            if (Input.GetMouseButtonDown(1))
            {
                Toolbar toolbar = inventory.GetComponent<Inventory>().toolbar;
                if (toolbar.inventory.slots[toolbar.slotIndex].HasItem)
                {
                    playerSounds.Play("BlockPlace");
                    WorldHelper.GetChunkFromPosition(placeBlockPosition).EditVoxel(placeBlockPosition, toolbar.inventory.slots[toolbar.slotIndex].itemSlot.stack.id);
                    toolbar.inventory.slots[toolbar.slotIndex].itemSlot.Take(1);
                }
            }
        }
    }

    private void Jump()
    {
        playerSounds.Play("Jump");
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
                placeBlockPosition = lastPos;

                highlightBlock.gameObject.SetActive(true);

                return;
            }

            lastPos = new float3(roundedPosition);

            step += checkIncrement;
        }

        highlightBlock.gameObject.SetActive(false);
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
            inventory.GetComponent<Inventory>().storage.SetActive(RimecraftWorld.Instance.InUI);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            this.transform.position = RimecraftWorld.Instance.spawnPosition;
        }

        if (!RimecraftWorld.Instance.InUI)
        {
            GetPlayerInputs();
            PlaceCursorBlock();
        }
    }
}