using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NormalPlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;

    bool readyToJump = true;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Key Binds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitSlope;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;


    public MovementState state;

    public enum MovementState
    {
        WALK,
        SPRINT,
        CROUCH,
        Air
    }



    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        exitSlope = false;

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        //ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        // handle drag
        if (grounded)
        {
            rb.drag = groundDrag;
        } else
        {
            rb.drag = 1; //air resistance
        }

        //Debug.Log("Current Speed: " + rb.velocity.magnitude.ToString());

    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown); // prevents continuous jumping, might remove later

        }


        // start crouching
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            Debug.Log("Crouching");
        }

        // end crouch
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            Debug.Log("Walking");
        }

    }


    private void StateHandler()
    {
        //mode - Crouch
        if(grounded && Input.GetKey(crouchKey))
        {
            state = MovementState.CROUCH;
            moveSpeed = crouchSpeed;
        }
        
        // mode - Sprint
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.SPRINT;
            moveSpeed = sprintSpeed;
        }

        // mode - Walk
        else if (grounded)
        {
            state = MovementState.WALK;
            moveSpeed = walkSpeed;
        }

        // mode - air
        else
        {
            state = MovementState.Air;
        }

    }



    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on slope
        if (OnSlope() && !exitSlope)
        {
            rb.AddForce(GetSlopeForwardDirection() * moveSpeed * 10f, ForceMode.Force);

            //if moving up a slope, add force to keep player on the surface
            if(rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 50f, ForceMode.Force);
            }
        }
        // on ground
        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        // in air
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        // turn off gravity on a slope
        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {

        // limit speed on slope
        if (OnSlope() && !exitSlope)
        {
            if (rb.velocity.magnitude > moveSpeed) rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        // limit speed on ground and in air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit flat velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
        
    }

    private void Jump()
    {
        // exit slope and jump if on a slope
        exitSlope = true;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        // make player ready to jump and not exiting a slope
        readyToJump = true;
        exitSlope = false;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal); // get the angle of the slope
            return angle < maxSlopeAngle && angle != 0; //raycast hits a slope
        }

        return false; // raycast doesnt hit anything
    }

    private Vector3 GetSlopeForwardDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized; // normalised since it is a direction
    }
}
