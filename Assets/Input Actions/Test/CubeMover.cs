using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CubeMover : MonoBehaviour
{
    TestPlayerControls controls;
    Vector2 move;
    Vector2 look;

    private void Awake()
    {
        controls = new TestPlayerControls();

        controls.GameplayController.Grow.performed += context => Grow();

        controls.GameplayController.Move.performed += context => move = context.ReadValue<Vector2>();
        controls.GameplayController.Move.canceled += context => move = Vector2.zero;

        controls.GameplayController.Look.performed += context => look = context.ReadValue<Vector2>();
        controls.GameplayController.Look.canceled += context => look = Vector2.zero;


    }

    private void Grow()
    {
        transform.localScale *= 1.1f;
    }

    private void Update()
    {
        Vector2 m = new Vector2(move.x, move.y) * Time.deltaTime;
        transform.Translate(m, Space.World);

        Vector2 l = new Vector2(look.x, look.y) * 100f * Time.deltaTime;
        transform.Rotate(l, Space.World);

    }

    void OnEnable()
    {
        controls.GameplayController.Enable();
    }

    void OnDisable()
    {
        controls.GameplayController.Disable();
    }
}
