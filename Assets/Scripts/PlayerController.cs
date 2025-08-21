using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(LineRenderer))]
public class PlayerController : MonoBehaviour
{
    public World world;
    public Hotbar hotbar;
    public Inventory inventory;

    public float mouseSensitivity = 8f;
    public float interactDistance = 5f;

    private Transform cameraPivot;
    private LineRenderer outline;

    private float verticalRotation = 0f;

    void Start()
    {
        cameraPivot = transform.Find("Camera Pivot");
        outline = GetComponent<LineRenderer>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Open inventory
        if (Input.GetKeyDown(KeyCode.E))
        {
            inventory.open = !inventory.open;
            Cursor.lockState = inventory.open ? CursorLockMode.None : CursorLockMode.Locked;
            Time.timeScale = inventory.open ? 0f : 1f;
        }

        if (inventory.open) return;

        // Mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, mouseX, 0);

        float mouseY = -Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation + mouseY, -90f, 90f);
        cameraPivot.localEulerAngles = new(verticalRotation, 0, 0);

        // Outline around hovered block
        if (Physics.Raycast(cameraPivot.position, cameraPivot.forward, out var hit, interactDistance))
        {
            Vector3Int hoveredPos = Vector3Int.FloorToInt(hit.point - hit.normal * 0.01f);
            DrawOutline(hoveredPos);

            // Break, place, rotate, flip - only if there is a hovered block
            if (Input.GetMouseButtonDown(0))
                BreakBlock(hit);
            if (Input.GetMouseButtonDown(1))
                PlaceBlock(hit);
            if (Input.GetKeyDown(KeyCode.R))
                RotateBlock(hit);
            if (Input.GetKeyDown(KeyCode.F))
                FlipBlock(hit);
        }
        else
        {
            outline.positionCount = 0;
        }
    }

    private void DrawOutline(Vector3Int pos)
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

    private void BreakBlock(RaycastHit hit)
    {
        // Minus normal so we are inside the hovered block
        var worldPos = Vector3Int.FloorToInt(hit.point - hit.normal * 0.01f);
        world.PlaceBlock(worldPos, Block.Type.Air);
    }

    void PlaceBlock(RaycastHit hit)
    {
        if (hotbar.selectedType == Block.Type.Air) return;

        // Plus normal so we are outside the hovered block
        var worldPos = Vector3Int.FloorToInt(hit.point + hit.normal * 0.01f);
        world.PlaceBlock(worldPos, hotbar.selectedType);
    }

    void RotateBlock(RaycastHit hit)
    {
        // Minus normal so we are inside the hovered block
        var worldPos = Vector3Int.FloorToInt(hit.point - hit.normal * 0.01f);
        if (!world.GetBlock(worldPos, out var block, out var chunk)) return;
        block.orientation.Rotate();
        chunk.GenerateMesh();
    }

    void FlipBlock(RaycastHit hit)
    {
        // Minus normal so we are inside the hovered block
        var worldPos = Vector3Int.FloorToInt(hit.point - hit.normal * 0.01f);
        if (!world.GetBlock(worldPos, out var block, out var chunk)) return;
        block.orientation.Flip();
        chunk.GenerateMesh();
    }
}
