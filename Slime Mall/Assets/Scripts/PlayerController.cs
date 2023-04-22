using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    Controls input;
    InputAction movement;

    Rigidbody2D rb;
    BoxCollider2D boxCol;
    Vector2 dir;

    SpriteRenderer sr;

    [Header("Move")]
    [Range(1f, 10f), Tooltip("Player movement speed")]
    public float moveSpeed = 5f;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        input = new Controls();
        rb = GetComponent<Rigidbody2D>();
        boxCol = GetComponent<BoxCollider2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

        //Bind movement to an action
        movement = input.Movement.Move;
        movement.Enable();

        input.Movement.Interact.performed += DoInteract;
        input.Movement.Interact.Enable();

        input.Movement.Pause.performed += DoPause;
        input.Movement.Pause.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if(!GameManager.instance.IsGamePaused())
        {
            //Get dir
            dir = movement.ReadValue<Vector2>();

            //Flip sprite based on dir
            if (dir.x < 0)
                sr.flipX = true;
            else if (dir.x > 0)
                sr.flipX = false;

            ProcessInput();
        }
    }

    void ProcessInput()
    {
        //Movement
        rb.velocity = Vector2.zero;
        rb.velocity += new Vector2(dir.x, dir.y) * moveSpeed;
    }

    void DoInteract(InputAction.CallbackContext obj)
    {
       
    }

    void DoPause(InputAction.CallbackContext obj)
    {
        GameManager.instance.PauseGame();
    }
}
