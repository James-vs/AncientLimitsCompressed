using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveController : MonoBehaviour
{
    private Rigidbody rb;
    public float speed;
    private Vector2 moveInputValue;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    private void OnMove(InputValue value)
    {
        moveInputValue = value.Get<Vector2>();
        Debug.Log("Movement Input Value: " + moveInputValue);
    }


    private void Move()
    {
        Vector3 result = new Vector3(moveInputValue.x, 0f, moveInputValue.y);
        rb.velocity = result * speed * Time.fixedDeltaTime * 10f;
    }

    private void FixedUpdate()
    {
        Move();
    }
}
