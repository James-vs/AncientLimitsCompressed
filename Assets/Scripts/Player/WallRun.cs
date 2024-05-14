using System.ComponentModel;
using System.Diagnostics.Tracing;
using UnityEngine;

public class WallRun : MonoBehaviour
{
    [Header("Wall Running")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallClimbSpeed;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public float maxWallRunTime;
    private float wallRunTimer;


    [Header("Input")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode upwardsWallRunKey = KeyCode.LeftShift;
    public KeyCode downwardsWallRunKey = KeyCode.LeftControl;
    private bool upwardsWallRun;
    private bool downwardsWallRun;
    private float horizontalInput;
    private float verticalInput;

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
    private AdvancedPlayerMovement apm;
    private Rigidbody rb;

    private void Start()
    {
        // initialising variables
        rb = GetComponent<Rigidbody>();
        apm = GetComponent<AdvancedPlayerMovement>();

        apm.wallrunning = false;
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();

    }

    private void FixedUpdate()
    {
        if(apm.wallrunning) WallRunMovement();
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

        // directional wallrunning
        upwardsWallRun = Input.GetKey(upwardsWallRunKey);
        downwardsWallRun = Input.GetKey(downwardsWallRunKey);


        // State - wallrunning
        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !exitingWall)
        {

            // start wallrunning
            if(!apm.wallrunning)
            {
                Debug.Log("Wallrunning");
                StartWallRun();
            }

            // wall jump
            if (Input.GetKeyDown(jumpKey)) WallJump();

            // wallrunTimer
            if (wallRunTimer > 0) wallRunTimer -= Time.deltaTime;

            if(wallRunTimer <= 0 && apm.wallrunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }
            
        }

        // State - exiting wallrun
        else if (exitingWall)
        {
            if (apm.wallrunning) StopWallRun();

            if (exitWallTimer > 0) exitWallTimer -= Time.deltaTime;

            if (exitWallTimer <= 0) exitingWall = false;
        }

        // State - stop wallrunning
        else
        {
            if(apm.wallrunning) StopWallRun();
        }
    }


    private void StartWallRun()
    {
        apm.wallrunning = true;

        wallRunTimer = maxWallRunTime;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

    }

    private void WallRunMovement()
    {
        rb.useGravity = useGravity;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        // change wallrun direction for left and right walls
        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude) wallForward = -wallForward;

        // forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        // upward/downward force
        if (upwardsWallRun) rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);
        if (downwardsWallRun) rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed, rb.velocity.z);


        // push player into wall unless they are moving away from a wall
        if ((wallLeft && horizontalInput > 0) && (wallRight && horizontalInput < 0) && !exitingWall) rb.AddForce(-wallNormal * 100f, ForceMode.Force);
        // added exiting wall check here - Didnt do anything.

        // weaken gravity
        if(useGravity) rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);

    }

    private void StopWallRun()
    {
        Debug.Log("StopWallRun called");
        apm.wallrunning = false; // stop wall running
    }

    private void WallJump()
    {
        Debug.Log("Wall Jump");

        exitingWall = true;

        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        Debug.Log("Wall Jump Force: " + forceToApply);

        // reset y velocity and add jump force
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse); // changed from ForceMode.Force


    }


}
