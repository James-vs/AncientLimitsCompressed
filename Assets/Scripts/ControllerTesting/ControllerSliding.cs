using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerSliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private ControllerPlayerMovement cpm;

    [Header("Sliding")]
    public float maxTimeSlide;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    public float maxSlideCooldown;
    private float slideCooldwnTimer;
    private bool slideCooldown;
    

    //[Header("Input")]
    //public KeyCode slideKey = KeyCode.LeftControl;
    private InputAction slideAction;
    private float horizontalInput;
    private float verticalInput;



    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cpm = GetComponent<ControllerPlayerMovement>();

        startYScale = transform.localScale.y;

        cpm.sliding = false;

        slideCooldown = false;

        slideAction = GetComponent<PlayerInput>().actions.FindAction("Slide");

    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal"); // Left and Right inputs
        verticalInput = Input.GetAxisRaw("Vertical"); // Forward & Backward inputs

        // add cooldown period after sliding
        if (slideCooldown)
        {
            SlideCooldown();
            Debug.Log("Slide cooldown:" + slideCooldown);
        } 
        else
        {
            if (!cpm.sliding && slideAction.IsInProgress() && (horizontalInput != 0 || verticalInput != 0) && !cpm.wallrunning)
            {
                StartSlide();
                Debug.Log("Slide cooldown:" + slideCooldown);
            }

            if (!slideAction.IsInProgress() && cpm.sliding)
            {
                StopSlide();
            }
        }
    }

    private void FixedUpdate()
    {
        if (cpm.sliding)
        {
            SlideMovement();
        }
    }

    private void SlideCooldown()
    {
        if (slideCooldwnTimer > 0)
        {
            slideCooldwnTimer -= Time.deltaTime;
        } 
        else
        {
            //slideCooldwnTimer = maxSlideCooldown;
            slideCooldown = false;
        }
    }

    private void StartSlide()
    {
        cpm.sliding = true;

        playerObj.localScale = new Vector3(playerObj.transform.localScale.x, slideYScale, playerObj.transform.localScale.z);

        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxTimeSlide;

        slideCooldwnTimer = maxSlideCooldown;
    }

    private void SlideMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // if player not on slope
        if (!cpm.OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }
        // if player on a slope
        else
        {
            rb.AddForce(cpm.GetSlopeForwardDirection(inputDirection) * slideForce, ForceMode.Force);
            // note no timer on a slope
        }
        
        // if slide timer runs out
        if (slideTimer <= 0)
        {
            StopSlide();
        }
    }

    private void StopSlide()
    {
        cpm.sliding = false;

        cpm.Animation(5); // stop sliding animation

        slideCooldown = true;

        playerObj.localScale = new Vector3(playerObj.transform.localScale.x, startYScale, playerObj.transform.localScale.z);

    }
}
