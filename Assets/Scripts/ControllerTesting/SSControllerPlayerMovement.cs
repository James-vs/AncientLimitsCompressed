using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SSControllerPlayerMovement : ControllerPlayerMovement
{
    //public new SSGameController gc;


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
        if (Time.timeScale == 0) Time.timeScale = 1f;

        
        
        gc = GameObject.Find("EventSystem").GetComponent<SSGameController>();
        gc.AssignPlayer(this.gameObject, firstPlayer);
        Debug.Log("Assigned Player " + firstPlayer);
        

        // assign control scheme to different controllers
        if (!firstPlayer)
        {
            firstPlayer = true;
        } 
        else
        {
            this.GetComponent<PlayerInput>().camera.gameObject.GetComponent<AudioListener>().enabled = false; // disable audiolistener for 2nd controller
        }

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
    }
}
