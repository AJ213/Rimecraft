using Unity.Mathematics;
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
                WorldHelper.GetChunkFromVector3(highlightBlock.position).EditVoxel(highlightBlock.position, 0);
            }

            // Build Block
            if (Input.GetMouseButtonDown(1))
            {
                if (toolbar.slots[toolbar.slotIndex].HasItem)
                {
                    WorldHelper.GetChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position, toolbar.slots[toolbar.slotIndex].itemSlot.stack.id);
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
        Vector3 lastPos = new Vector3();

        while (step < reach)
        {
            float3 pos = cam.position + (cam.forward * step);

            if (RimecraftWorld.Instance.CheckForVoxel(new int3(pos)) != 0)
            {
                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeBlock.position = lastPos;

                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);

                return;
            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

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