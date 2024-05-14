using System.ComponentModel;
using System.Diagnostics.Tracing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerWallRun : MonoBehaviour
{
    [Header("Wall Running")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallClimbSpeed;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public float wallJumpForwardForce;
    public float maxWallRunTime;
    private float wallRunTimer;


    private float horizontalInput;
    private float verticalInput;
    private PlayerInput playerInput;
    private InputAction wallJumpAction;
    private InputAction upWallRunAction;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Exiting")]
    private bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    [Header("Gravity")]
    public bool useGravity;
    public float gravityCounterForce;

    [Header("References")]
    public Transform orientation;
    private ControllerPlayerMovement cpm;
    private Rigidbody rb;

    private void Start()
    {
        // initialising variables
        rb = GetComponent<Rigidbody>();
        cpm = GetComponent<ControllerPlayerMovement>();

        cpm.wallrunning = false;

        playerInput = GetComponent<PlayerInput>();

        wallJumpAction = playerInput.actions.FindAction("Jump");
        upWallRunAction = playerInput.actions.FindAction("Sprint");
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();

    }

    private void FixedUpdate()
    {
        if(cpm.wallrunning) WallRunMovement();
    }

    private void CheckForWall()
    {
        // use raycasts to check for wall presence
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        // check if player has jumped high enough to wall run
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        // get inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");


        // State - wallrunning
        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !exitingWall)
        {

            // start wallrunning
            if(!cpm.wallrunning)
            {
                Debug.Log("Wallrunning");
                StartWallRun();
            }

            // wall jump
            if (wallJumpAction.WasPressedThisFrame())
            {
                
                WallJump();
            }

            // wallrunTimer
            if (wallRunTimer > 0) wallRunTimer -= Time.deltaTime;

            if(wallRunTimer <= 0 && cpm.wallrunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }
            
        }

        // State - exiting wallrun
        else if (exitingWall)
        {
            if (cpm.wallrunning) StopWallRun();

            if (exitWallTimer > 0) exitWallTimer -= Time.deltaTime;

            if (exitWallTimer <= 0) exitingWall = false;
        }

        // State - stop wallrunning
        else
        {
            if(cpm.wallrunning) StopWallRun();
        }
    }


    private void StartWallRun()
    {
        cpm.Animation(2);

        cpm.wallrunning = true;

        wallRunTimer = maxWallRunTime;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

    }

    private void WallRunMovement()
    {
        rb.useGravity = false; // useGravity

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        // change wallrun direction for left and right walls
        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude) wallForward = -wallForward;

        // forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        // upward/downward force
        if (upWallRunAction.IsInProgress() && !exitingWall)
        {
            rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z); // added !exiting wall for testing
            Debug.Log("WallRun Upwards");
        }


        // push player into wall unless they are moving away from a wall
        if ((wallLeft && horizontalInput > 0) && (wallRight && horizontalInput < 0) && !exitingWall) rb.AddForce(-wallNormal * 100f, ForceMode.Force);

    }

    private void StopWallRun()
    {
        cpm.Animation(3);
        Debug.Log("StopWallRun called");
        cpm.wallrunning = false; // stop wall running
        rb.useGravity = true;
    }

    private void WallJump()
    {
        Debug.Log("Wall Jump");

        cpm.Animation(3);

        exitingWall = true;

        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        // add momentum in upwards direction and in direction perpendicular to the wall.
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce + transform.forward * wallJumpForwardForce;

        Debug.Log("Wall Jump Force: " + forceToApply);

        // reset y velocity and add jump force
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse); // changed from ForceMode.Force

    }


}