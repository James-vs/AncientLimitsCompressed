using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ControllerPlayerMovement : MonoBehaviour
{
    [Header("Game")]
    public GameController gc;
    public static bool firstPlayer = false;
    
    [Header("Movement")]
    public float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float wallRunSpeed;

    protected float desiredMoveSpeed;
    protected float lastDesiredMoveSpeed;
    protected PlayerInput playerInput;
    protected InputAction sprintAction;
    protected InputAction jumpAction;
    protected InputAction slideAction;
    protected InputAction crouchAction;
    protected InputAction moveAction;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;


    protected bool readyToJump = true;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    protected float startYScale;

    [Header("Animation")]
    public Animator animator;
    public GameObject thyra;
    public GameObject armor;
    public GameObject eyes;
    public GameObject eyebrows;
    public GameObject metarig;
    public GameObject thyraHorizontal;


    //[Header("Key Binds")]
    //public KeyCode jumpKey = KeyCode.Space;
    //public KeyCode sprintKey = KeyCode.LeftShift;
    //public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    protected bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    protected bool exitSlope;

    public Transform orientation;

    [Header("Audio")]
    public AudioSource jumpAudio;
    public AudioSource sprintAudio;
    public AudioSource walkAudio;

    protected float horizontalInput;
    protected float verticalInput;

    protected Vector3 moveDirection;

    protected Rigidbody rb;

    public MovementState state;

    public enum MovementState
    {
        IDLE,
        WALK,
        SPRINT,
        CROUCH,
        SLIDE,
        WALLRUN,
        AIR
    }

    public bool sliding;
    public bool wallrunning;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        exitSlope = false;

        startYScale = transform.localScale.y;

        playerInput = GetComponent<PlayerInput>();

        Debug.Log(playerInput.gameObject);

        animator = gameObject.GetComponent<Animator>();

        // unfreeze time if necessary
        //if (Time.timeScale == 0) Time.timeScale = 1f;

        // assign control scheme to different controllers
        //if (!firstPlayer)
        //{
           // firstPlayer = true;
            //playerInput.SwitchCurrentControlScheme("Controller1");
            //Debug.Log("Control Scheme: " + playerInput.currentControlScheme);
            //Debug.Log("Is first player: " + firstPlayer);
        //} 
        //else
        //{
            //playerInput.SwitchCurrentControlScheme("Controller2");
            //this.GetComponent<PlayerInput>().camera.gameObject.GetComponent<AudioListener>().enabled = false; // disable audiolistener for 2nd controller
            //Debug.Log("Control Scheme: " + playerInput.currentControlScheme);
            //Debug.Log("Is first player: " + firstPlayer);
        //}

        // defining input actions
        jumpAction = playerInput.actions.FindAction("Jump");
        sprintAction = playerInput.actions.FindAction("Sprint");
        crouchAction = playerInput.actions.FindAction("Crouch");
        slideAction = playerInput.actions.FindAction("Slide");
        moveAction = playerInput.actions.FindAction("Move");

        //debugging
        Debug.Log(jumpAction.ToString());
        Debug.Log(sprintAction.ToString());
        Debug.Log(crouchAction.ToString());
        Debug.Log(slideAction.ToString());
        Debug.Log(moveAction.ToString());

        // cancel crouch when crouch button no longer being pressed
        crouchAction.canceled += context => StopCrouch();

        if (gc == null) gc = GameObject.Find("EventSystem").GetComponent<GameController>();

    }

    public void ResetFirstPlayer()
    {
        firstPlayer = false;
    }

    private void Update()
    {
        //ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f, whatIsGround);//+ 0.2f

        //Debug.Log("Grounded: " + grounded);

        MyInput();
        SpeedControl();
        StateHandler();

        // handle drag
        //        if (grounded)
        //      {
        //        rb.drag = groundDrag;
        //  } else
        //{
        //  rb.drag = 0f;
        //}

        rb.drag = grounded ? groundDrag : 0f;

    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = moveAction.ReadValue<Vector2>().x;// Input.GetAxisRaw("Horizontal");
        verticalInput = moveAction.ReadValue<Vector2>().y;//Input.GetAxisRaw("Vertical");

        //debugging
        //Debug.Log("hz: " + horizontalInput + "\nVrt: " + verticalInput);

        // when to jump
        if (jumpAction.WasPressedThisFrame() && readyToJump && grounded)
        {
            readyToJump = false;
            //Animation(3);
            //animator.SetBool("IsJumping", true);

            Jump();
            Invoke(nameof(ResetJump), jumpCooldown); // prevents continuous jumping, might remove later
            Debug.Log("Jumping");

        }


        // if crouching
        if (crouchAction.IsInProgress())
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            Debug.Log("Crouching");
        }

    }

    protected void StopCrouch()
    {
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        Debug.Log("Walking");

    }


    private void StateHandler()
    {
        // mode - Wallrun
        if (wallrunning)
        {
            state = MovementState.WALLRUN;
            desiredMoveSpeed = wallRunSpeed;
        }
        // mode - Slide
        else if (sliding && !exitSlope)
        {
            state = MovementState.SLIDE;
            Animation(4);
            if (OnSlope() && rb.velocity.y < -0.1f)
            {
                desiredMoveSpeed = slideSpeed;
            }
            else
            {
                desiredMoveSpeed = sprintSpeed;
            }

        }

        // mode - Crouch
        else if (grounded && crouchAction.IsInProgress() && !exitSlope)
        {
            Animation(1);
            state = MovementState.CROUCH;
            desiredMoveSpeed = crouchSpeed;
        }

        // mode - Sprint
        else if (grounded && sprintAction.IsInProgress() && !exitSlope)
        {
            Debug.Log("Sprinting");
            //Debug.Log("Is Grounded: " + grounded);
            Animation(2);
            state = MovementState.SPRINT;
            desiredMoveSpeed = sprintSpeed;
        }

        // mode - Walk
        else if (grounded && (horizontalInput != 0 || verticalInput != 0) && !exitSlope) // if character is moving and not jumping
        {
            Animation(1);
            state = MovementState.WALK;
            desiredMoveSpeed = walkSpeed;
        }
        // mode - Idle
        else if (grounded && !exitSlope) // if character is standing still and not jumping
        {
            Animation(0);
            state = MovementState.IDLE;
        }

        // mode - In Air
        else
        {
            state = MovementState.AIR;
        }

        // check if desiredMoveSpeed has changed by a lot quickly
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;

    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movement speed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            time += Time.deltaTime;
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }



    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on slope
        if (OnSlope() && !exitSlope)
        {
            rb.AddForce(GetSlopeForwardDirection(moveDirection) * moveSpeed * 30f, ForceMode.Force);

            // if moving up a slope, add force to keep player on the surface
            if (rb.velocity.y > 0) // might need to change this to 0.1f
            {
                rb.AddForce(Vector3.down * 40f, ForceMode.Force);
            }
        }
        // on ground - walk
        //else if (grounded)
        //{
            //rb.AddForce(moveDirection.normalized * moveSpeed * 30f, ForceMode.Force);
        //}
        // in air
        //else if (!grounded)
        //{
            //rb.AddForce(moveDirection.normalized * moveSpeed * 30f * airMultiplier, ForceMode.Force);
        //}

        // on ground or in air
        rb.AddForce(moveDirection.normalized * moveSpeed * 30f, ForceMode.Force);

        // turn off gravity on a slope
        if (!wallrunning) rb.useGravity = !OnSlope();
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

    public void SetDesiredMoveSpeed(float speed)
    {
        desiredMoveSpeed = speed;
    }

    private void Jump()
    {
        if (!wallrunning) Animation(3);
        // exit slope and jump if on a slope
        exitSlope = true;

        //rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        // make player ready to jump and not exiting a slope
        //Animation(6);
        readyToJump = true;
        exitSlope = false;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal); // get the angle of the slope
            return angle < maxSlopeAngle && angle != 0; //raycast hits a slope
        }

        return false; // raycast doesnt hit anything
    }

    public Vector3 GetSlopeForwardDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized; // normalised since it is a direction
    }


    // animation handler
    public void Animation(int anim)
    {
        if (anim == 0)
        {
            //Debug.Log("Idle Animation");
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsIdle", true);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsJumping", false);
            if (walkAudio.isPlaying) walkAudio.Stop();
            if (sprintAudio.isPlaying) sprintAudio.Stop();
        }
        else if (anim == 1)
        {
            //Debug.Log("Walk Animation");
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsWalking", true);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsJumping", false);
            if (sprintAudio.isPlaying) sprintAudio.Stop();
            if (!walkAudio.isPlaying) walkAudio.Play();
        }
        else if (anim == 2)
        {
            //Debug.Log("Sprint Animation");
            animator.SetBool("IsRunning", true);
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsJumping", false);
            if (walkAudio.isPlaying) walkAudio.Stop();
            if (!sprintAudio.isPlaying) sprintAudio.Play();
        }
        else if (anim == 3)
        {
            Debug.Log("Jump Animation");
            //animator.SetBool("IsJumping", false);
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsJumping", true);
            if (walkAudio.isPlaying) walkAudio.Stop();
            if (sprintAudio.isPlaying) sprintAudio.Stop();
            jumpAudio.Play();
        }
        else if (anim == 4)
        {
            //Debug.Log("Slide Animation");
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsJumping", false);
            thyraHorizontal.SetActive(true);
            thyra.SetActive(false);
            metarig.SetActive(false);
            eyes.SetActive(false);
            eyebrows.SetActive(false);
            armor.SetActive(false);
            if (walkAudio.isPlaying) walkAudio.Stop();
            if (sprintAudio.isPlaying) sprintAudio.Stop();
        }
        else if (anim == 5)
        {
            //Debug.Log("End Slide Animation");
            thyraHorizontal.SetActive(false);
            thyra.SetActive(true);
            metarig.SetActive(true);
            eyes.SetActive(true);
            eyebrows.SetActive(true);
            armor.SetActive(true);
        }
        else if (anim == 6)
        {
            //Debug.Log("Stop Animation");
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsJumping", false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Complete")
        {
            if (gc != null) gc.LevelComplete(this.gameObject);
        }
    }
}
