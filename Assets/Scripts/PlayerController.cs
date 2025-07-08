using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(LineRenderer))]
public class PlayerController : MonoBehaviour
{
    public World world;

    public float speed = 5f;
    public float jumpForce = 5f;
    public float mouseSensitivity = 8f;
    public float interactDistance = 5f;

    private Rigidbody rb;
    private Transform cameraPivot;
    private LineRenderer outline;

    private Inventory inventory;

    private float verticalRotation = 0f;
    private bool grounded;

    private bool isMouseLocked = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraPivot = transform.Find("Camera Pivot");
        outline = GetComponent<LineRenderer>();

        inventory = GetComponent<Inventory>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Open inventory
        if (Input.GetKeyDown(KeyCode.E))
        {
            isMouseLocked = !isMouseLocked;
            Cursor.lockState = isMouseLocked ? CursorLockMode.Locked : CursorLockMode.None;
        }

        if (!isMouseLocked) return;

        // Mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, mouseX, 0);

        float mouseY = -Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation + mouseY, -90f, 90f);
        cameraPivot.localEulerAngles = new(verticalRotation, 0, 0);

        // Jump
        if (Input.GetButtonDown("Jump") && grounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Outline around hovered block
        if (Physics.Raycast(cameraPivot.position, cameraPivot.forward, out RaycastHit hit, interactDistance))
        {
            Vector3Int hoveredPos = Vector3Int.FloorToInt(hit.point - hit.normal * 0.01f);
            DrawOutline(hoveredPos);
        }
        else
        {
            outline.positionCount = 0;
        }

        // Break, place, rotate, flip
        if (Input.GetMouseButtonDown(0))
            BreakBlock();
        if (Input.GetMouseButtonDown(1))
            PlaceBlock();
        if (Input.GetKeyDown(KeyCode.R))
            RotateBlock();
        if (Input.GetKeyDown(KeyCode.F))
            FlipBlock();
    }

    void FixedUpdate()
    {
        // Movement
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = transform.forward * v + transform.right * h;
        Vector3 velocity = move * speed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
    }

    void DrawOutline(Vector3Int pos)
    {
        Vector3[] corners = new Vector3[8]
        {
            pos + new Vector3(0, 0, 0),
            pos + new Vector3(1, 0, 0),
            pos + new Vector3(1, 1, 0),
            pos + new Vector3(0, 1, 0),
            pos + new Vector3(0, 0, 1),
            pos + new Vector3(1, 0, 1),
            pos + new Vector3(1, 1, 1),
            pos + new Vector3(0, 1, 1)
        };

        Vector3[] points = new Vector3[]
        {
            corners[0], corners[1], corners[2], corners[3], corners[0],
            corners[4], corners[5], corners[1], corners[5], corners[6],
            corners[2], corners[6], corners[7], corners[3], corners[7], corners[4]
        };

        outline.positionCount = points.Length;
        outline.SetPositions(points);
    }

    void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
            {
                grounded = true;
                return;
            }
        }
        grounded = false;
    }

    void OnCollisionExit(Collision collision)
    {
        grounded = false;
    }

    void BreakBlock()
    {
        if (Physics.Raycast(cameraPivot.position, cameraPivot.forward, out RaycastHit hit, interactDistance))
        {
            // Minus normal so we are inside the hovered block
            var worldPos = Vector3Int.FloorToInt(hit.point - hit.normal * 0.01f);
            world.PlaceBlock(worldPos, Block.Type.Air);
        }
    }

    void PlaceBlock()
    {
        var selectedType = inventory.GetSelectedBlockType();
        if (selectedType == Block.Type.Air) return;

        if (Physics.Raycast(cameraPivot.position, cameraPivot.forward, out RaycastHit hit, interactDistance))
        {
            // Plus normal so we are outside the hovered block
            var worldPos = Vector3Int.FloorToInt(hit.point + hit.normal * 0.01f);
            world.PlaceBlock(worldPos, selectedType);
        }
    }

    void RotateBlock()
    {
        if (Physics.Raycast(cameraPivot.position, cameraPivot.forward, out var hit, interactDistance))
        {
            // Minus normal so we are inside the hovered block
            var worldPos = Vector3Int.FloorToInt(hit.point - hit.normal * 0.01f);
            if (!world.GetBlock(worldPos, out var block, out var chunk)) return;
            block.orientation.Rotate();
            chunk.GenerateMesh();
        }
    }

    void FlipBlock()
    {
        if (Physics.Raycast(cameraPivot.position, cameraPivot.forward, out var hit, interactDistance))
        {
            // Minus normal so we are inside the hovered block
            var worldPos = Vector3Int.FloorToInt(hit.point - hit.normal * 0.01f);
            if (!world.GetBlock(worldPos, out var block, out var chunk)) return;
            block.orientation.Flip();
            chunk.GenerateMesh();
        }
    }

    private Vector3Int GetAxisVector(Vector3 vec)
    {
        float maxValue;
        int maxValueIndex;

        if (Mathf.Abs(vec.x) >= Mathf.Abs(vec.y))
        {
            maxValue = vec.x;
            maxValueIndex = 0;
        }
        else
        {
            maxValue = vec.y;
            maxValueIndex = 1;
        }

        if (Mathf.Abs(vec.z) > Mathf.Abs(maxValue))
        {
            maxValue = vec.z;
            maxValueIndex = 2;
        }

        Vector3Int pos = new(0, 0, 0);
        if (maxValueIndex == 0)
            pos = new(1, 0, 0);
        else if (maxValueIndex == 1)
            pos = new(0, 1, 0);
        else if (maxValueIndex == 2)
            pos = new(0, 0, 1);

        return pos * (int)Mathf.Sign(maxValue);
    }
}
