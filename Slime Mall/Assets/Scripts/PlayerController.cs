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
    Animator animator;
    Rigidbody2D rb;
    BoxCollider2D boxCol;
    AudioSource audioSource;
    Vector2 dir;

    SpriteRenderer sr;

    [Header("Move")]
    [Range(1f, 100f), Tooltip("Player movement speed")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 15.0f;
    public float sprintDelay = 2.0f;
    private float originalSprintDelay;
    private float originalMovespeed;

    [Header("Inteactions")]
    public LayerMask interactLayer;
    [Range(0,5f)]
    public float interactRadius = 1f;

    public CircleCollider2D checkArea;
    
    public bool isVisible = true;

    GameObject binHidingIn;
    GameObject ventHidingIn;
    bool isMovementEnabled = true;

    bool isInteractEnabled = true;

    float speedJuice = 3;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        input = new Controls();
        rb = GetComponent<Rigidbody2D>();
        boxCol = GetComponent<BoxCollider2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

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

        originalMovespeed = moveSpeed;
        originalSprintDelay = sprintDelay;
    }

    // Update is called once per frame
    void Update()
    {
        if(!GameManager.instance.IsGamePaused())
        {
            //Get dir
            dir = movement.ReadValue<Vector2>();
            if (Input.GetKey(KeyCode.LeftShift))
            {
                sprintDelay = originalSprintDelay;
                speedJuice -= Time.deltaTime;
                if (speedJuice <= 0)
                {
                    moveSpeed = originalMovespeed;
                    speedJuice = 0;
                }
                else
                {
                    moveSpeed = sprintSpeed;
                }
            }
            else
            {

                sprintDelay -= Time.deltaTime;
                if (sprintDelay <= 0)
                {
                    speedJuice += Time.deltaTime;
                    if (speedJuice >= 3)
                    {
                        speedJuice = 3;
                    }
                }

                 moveSpeed = originalMovespeed;
            }
            UI.instance.ChangeSprintBar(speedJuice);
            //Flip sprite based on dir
            if (dir.x < 0)
                sr.flipX = true;
            else if (dir.x > 0)
                sr.flipX = false;
        }
    }

    private void FixedUpdate()
    {
        ProcessInput();
    }

    void ProcessInput()
    {
        //Movement
        if (isMovementEnabled)
        {
            rb.velocity = Vector2.zero;
            rb.velocity += new Vector2(dir.x, dir.y) * moveSpeed * 100 * Time.deltaTime;
            animator.SetFloat("Horizontal", dir.x);
            animator.SetFloat("Vertical", dir.y);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    void InteractWithBin(GameObject binGameObject)
    {
        //When called once, will go into bin animation. When called again, will exit bin animation.
        if (!animator.GetBool("Bin"))
        {
            isInteractEnabled = false;
            animator.SetTrigger("EnterBin");
            animator.SetBool("Bin", true);
            binHidingIn = binGameObject;
            binGameObject.GetComponent<SpriteRenderer>().enabled = false;
            //Disable movement
            FreezePlayer();
            //Reposition
            Vector3 directionFacingBin = binGameObject.transform.position - transform.position;
            transform.position = binGameObject.transform.position + (directionFacingBin.normalized) / 4;
            isInteractEnabled = true;
        }
        else
        {
            isInteractEnabled = false;
            animator.SetBool("Bin", false);
            //Animation event will call E_ExitedBin() after animation is finished.
            //Enable Vision by AI
        }


    }
    public void E_ExitedBin()
    {
        Debug.Log("Exited Bin Fully");
            binHidingIn.GetComponent<SpriteRenderer>().enabled = true;
        binHidingIn = null;
        //Enable movement
        UnFreezePlayer();
        isInteractEnabled = true;
    }

    public void InteractWithVent(GameObject ventGameObject)
    {
        Debug.Log("Interact with vent");
        //When called once, will go into vent animation. When called again, will exit vent animation.
        if (!animator.GetBool("Vent"))
        {
            isInteractEnabled = false;
            animator.SetTrigger("EnterVent");
            animator.SetBool("Vent", true);
            ventHidingIn = ventGameObject;
            ventGameObject.GetComponent<SpriteRenderer>().enabled = false;
            //Disable movement
            FreezePlayer();
            //Reposition
            Vector3 directionFacingBin = ventGameObject.transform.position - transform.position;
            transform.position = ventGameObject.transform.position + (directionFacingBin.normalized) / 4;
            isInteractEnabled = true;

        }
        else
        {
            isInteractEnabled = false;
            animator.SetBool("Vent", false);
            //Animation event will call E_ExitedVent() after animation is finished.
            //Enable Vision by AI
        }
    }

    public void E_ExitedVent()
    {
        Debug.Log("Exited Vent Fully");
        ventHidingIn.GetComponent<SpriteRenderer>().enabled = true;
        ventHidingIn = null;
        //Enable movement
        UnFreezePlayer();
        isInteractEnabled = true;
    }
    public void E_InteractSound()
    {
        AudioManager.instance.PlaySoundFromSource("Interact", audioSource);
    }
    public void E_ConsumeSound()
    {
        AudioManager.instance.PlaySoundFromSource("Consume", audioSource);
    }

    void DoInteract(InputAction.CallbackContext obj)
    {
        // Interact is disabled while in the middle of interacting want to prevent spamming 'E'
        if (isInteractEnabled)
        {
            Debug.Log("Do interact");
            if (binHidingIn)
            {
                InteractWithBin(null);
                return;
            }
            else if (ventHidingIn)
            {
                InteractWithVent(null);
                return;
            }
            else
            {
                Collider2D[] colls = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactLayer);
                foreach (Collider2D collider in colls)
                {
                    // Are we within the radius of interacting with the bin?
                    if (collider.GetComponent<Bin>() != null)
                    {
                        InteractWithBin(collider.gameObject);
                        return;
                    }
                    else if (collider.GetComponent<Vent>() != null)
                    {
                        InteractWithVent(collider.gameObject);
                        return;
                    }
                }
            }
        }
    }

    void DoKill(InputAction.CallbackContext obj)
    {
        // If the slime is hiding don't let them kill an NPC
        if(IsSlimeHidden() == true)
        {
            return;
        }
        // TODO this has an issue with PlayerController still being referenced after destruction
        Collider2D[] colls = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactLayer);

        foreach(Collider2D col in colls)
        {
            if(col.CompareTag("Killable"))
            {
                animator.SetTrigger("Consume");
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

    public bool IsSlimeHidden()
    {
        return binHidingIn || ventHidingIn;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }

    public void FreezePlayer()
    {
        isMovementEnabled = false;
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private void UnFreezePlayer()
    {
        isMovementEnabled = true;
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void Cleanup()
    {
        input.Movement.Interact.performed -= DoInteract;
        input.Movement.Interact.Disable();

        input.Movement.Kill.performed -= DoKill;
        input.Movement.Kill.Disable();

        input.Movement.Pause.performed -= DoPause;
        input.Movement.Pause.Disable();
    }
}
