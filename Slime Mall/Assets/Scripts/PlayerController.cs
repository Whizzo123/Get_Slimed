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
    [Range(1f, 100f), Tooltip("Player movement speed")]
    public float moveSpeed = 5f;

    [Header("Inteactions")]
    public LayerMask interactLayer;
    [Range(0,5f)]
    public float interactRadius = 1f;

    public CircleCollider2D checkArea;
    
    public bool isVisible = true;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        input = new Controls();
        rb = GetComponent<Rigidbody2D>();
        boxCol = GetComponent<BoxCollider2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

        checkArea.radius = interactRadius;

        //Bind movement to an action
        movement = input.Movement.Move;
        movement.Enable();

        input.Movement.Interact.performed += DoInteract;
        input.Movement.Interact.Enable();

        input.Movement.Kill.performed += DoKill;
        input.Movement.Kill.Enable();

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
        rb.velocity += new Vector2(dir.x, dir.y) * moveSpeed * 100 * Time.deltaTime;
    }


    void DoInteract(InputAction.CallbackContext obj)
    {
        Collider2D[] colls = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactLayer);
    }

    void DoKill(InputAction.CallbackContext obj)
    {
        Collider2D[] colls = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactLayer);

        foreach(Collider2D col in colls)
        {
            if(col.CompareTag("Killable"))
            {
                transform.position = col.gameObject.transform.position;
                NPCManager.instance.KillNPC(col.gameObject);
                GameManager.instance.UpdateScore();
                return;
            }
        }
    }

    void DoPause(InputAction.CallbackContext obj)
    {
        GameManager.instance.PauseGame();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
