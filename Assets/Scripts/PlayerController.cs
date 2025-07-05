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

    private float verticalRotation = 0f;
    private bool grounded;

    private readonly Dictionary<KeyCode, Block.Type> blockTypes = new()
    {
        { KeyCode.Alpha1, Block.Type.Stone },
        { KeyCode.Alpha2, Block.Type.PowerSource },
        { KeyCode.Alpha3, Block.Type.Wire }
    };
    private Block.Type blockTypeToPlace = Block.Type.Stone;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraPivot = transform.Find("Camera Pivot");
        outline = GetComponent<LineRenderer>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
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

        // Select block type
        foreach (var kvp in blockTypes)
        {
            if (Input.GetKeyDown(kvp.Key))
                blockTypeToPlace = kvp.Value;
        }

        // Break and place
        if (Input.GetMouseButtonDown(0))
            BreakBlock();
        if (Input.GetMouseButtonDown(1))
            PlaceBlock();
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
        if (Physics.Raycast(cameraPivot.position, cameraPivot.forward, out RaycastHit hit, interactDistance))
        {
            // Plus normal so we are outside the hovered block
            var worldPos = Vector3Int.FloorToInt(hit.point + hit.normal * 0.01f);
            world.PlaceBlock(worldPos, blockTypeToPlace);
        }
    }
}
