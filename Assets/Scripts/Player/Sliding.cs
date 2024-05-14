using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private AdvancedPlayerMovement apm;

    [Header("Sliding")]
    public float maxTimeSlide;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;



    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        apm = GetComponent<AdvancedPlayerMovement>();

        startYScale = transform.localScale.y;

        apm.sliding = false;
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal"); // A & D key inputs
        verticalInput = Input.GetAxisRaw("Vertical"); // W & S key inputs

        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0) && !apm.wallrunning)
        {
            StartSlide();
        }

        if (Input.GetKeyUp(slideKey) && apm.sliding)
        {
            StopSlide();
        }

    }

    private void FixedUpdate()
    {
        if (apm.sliding)
        {
            SlideMovement();
        }
    }

    private void StartSlide()
    {
        apm.sliding = true;

        playerObj.localScale = new Vector3(playerObj.transform.localScale.x, slideYScale, playerObj.transform.localScale.z);

        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxTimeSlide;
    }

    private void SlideMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // if player not on slope
        if (!apm.OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }
        // if player on a slope
        else
        {
            rb.AddForce(apm.GetSlopeForwardDirection(inputDirection) * slideForce, ForceMode.Force);
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
        apm.sliding = false;

        playerObj.localScale = new Vector3(playerObj.transform.localScale.x, startYScale, playerObj.transform.localScale.z);

    }
}
