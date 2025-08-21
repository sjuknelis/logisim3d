using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 5f, jumpSpeed = 5f, flySpeed = 5f,
        friction = 20f, flyResistance = 10f, gravity = 10f;

    public float doubleJumpSecs = 0.3f;

    private Rigidbody rb;

    private bool isGrounded, isFlying;
    private float lastJumpTime;

    private Vector3 xzRelativeVelocity, effectiveRight, effectiveForward;
    private float yVelocity = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Update grounded state
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1f);

        if (Input.GetButtonDown("Jump"))
        {
            if (Time.time - lastJumpTime < doubleJumpSecs && !isGrounded)
            {
                // Double jump to trigger flying
                isFlying = !isFlying;
            }
            lastJumpTime = Time.time;

            // Jump
            if (isGrounded) yVelocity = jumpSpeed;
        }

        if (isFlying)
        {
            // Flight controls
            if (Input.GetButton("Jump")) yVelocity = flySpeed;
            if (Input.GetKey(KeyCode.LeftShift)) yVelocity = -flySpeed;

            if (isGrounded) isFlying = false;
        }
    }

    void FixedUpdate()
    {
        // WASD velocity
        if (Input.GetKey(KeyCode.D))
        {
            xzRelativeVelocity.x = walkSpeed;
            effectiveRight = transform.right;
        }
        if (Input.GetKey(KeyCode.A))
        {
            xzRelativeVelocity.x = -walkSpeed;
            effectiveRight = transform.right;
        }
        if (Input.GetKey(KeyCode.W))
        {
            xzRelativeVelocity.z = walkSpeed;
            effectiveForward = transform.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            xzRelativeVelocity.z = -walkSpeed;
            effectiveForward = transform.forward;
        }

        // Apply friction
        xzRelativeVelocity.x -= Mathf.Sign(xzRelativeVelocity.x) * friction * Time.fixedDeltaTime;
        xzRelativeVelocity.z -= Mathf.Sign(xzRelativeVelocity.z) * friction * Time.fixedDeltaTime;

        var xzRealVelocity = xzRelativeVelocity.x * effectiveRight + xzRelativeVelocity.z * effectiveForward;

        if (isGrounded)
        {
            // Stop moving down on hitting ground
            yVelocity = Mathf.Max(yVelocity, 0);
        }
        else
        {
            if (isFlying)
            {
                // Apply fly resistance
                if (Mathf.Abs(yVelocity) < 0.1f)
                    yVelocity = 0;
                else
                    yVelocity -= Mathf.Sign(yVelocity) * flyResistance * Time.fixedDeltaTime;
            }
            else
            {
                // Apply gravity
                yVelocity -= gravity * Time.fixedDeltaTime;
            }
        }

        rb.linearVelocity = new(xzRealVelocity.x, yVelocity, xzRealVelocity.z);
    }
}