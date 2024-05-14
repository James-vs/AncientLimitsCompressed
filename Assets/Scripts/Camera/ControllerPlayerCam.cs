using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class ControllerPlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;

    public GameObject player;
    
    //private Controller controller;

    private PlayerInput playerInput;

    InputAction lookAction;

    float xRotation;
    float yRotation;

    private Vector2 lookRotation;

    private void Start()
    {
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerInput = player.GetComponent<PlayerInput>();
        lookAction = playerInput.actions.FindAction("Look");
        //controller.ControllerMovement.Look.performed += context => lookRotation = context.ReadValue<Vector2>();
        //controller.ControllerMovement.Look.canceled += context => lookRotation = Vector2.zero;
    }

    private void Update()
    {
        MovePlayerCam();
    }

    private void MovePlayerCam()
    {
        lookRotation = lookAction.ReadValue<Vector2>();

        // debugging
        //Debug.Log(lookRotation);

        float mouseX = lookRotation.x * Time.deltaTime * sensX;
        float mouseY = lookRotation.y * Time.deltaTime * sensY;

        // debugging
        //Debug.Log("mouseX: " + mouseX);
        //Debug.Log("mouseY: " + mouseY);

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // rotate cam and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

        // rotate the player according to it's orientation
        player.transform.rotation = orientation.rotation;
    }


}
