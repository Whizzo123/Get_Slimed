using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using static NPCManager;

/*
*AUTHOR: Antonio Villalta Isidro
*EDITORS: Tanapat Somrid
*DATEOFCREATION: NA
*DESCRIPTION: 
*/

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    [Header("Components")]
    Controls input;
    Animator animator;
    AudioSource audioSource;
    NavMeshAgent agent;

    SpriteRenderer sr;
    Vector3 dir;
    NPCBehaviour targetNPC;
    HidingObject targetHS;

    [Header("Movement")]
    [SerializeField, Range(1f, 100f), Tooltip("Movement speed")]
    float moveSpeed = 5f;
    bool bIsMovementEnabled = true;

    [Header("Inteactions")]
    [SerializeField][Tooltip("Layer that the player can interact with")] LayerMask interactLayer;
    [SerializeField][Range(0, 5f)][Tooltip("Radius that player can trigger interact objects")] float interactRadius = 1f;
    CircleCollider2D interactCircle;
    HidingObject objectHidingIn;
    bool bIsInteractEnabled = true;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        //References
        input = new Controls();
        sr = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        interactCircle = GetComponentInChildren<CircleCollider2D>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        //Variables
        interactCircle.radius = interactRadius;

        //Bind movement to an action
        input.Movement.Interact.performed += DoInteract;
        input.Movement.Interact.Enable();

        input.Movement.Kill.performed += DoKill;
        input.Movement.Kill.Enable();

        input.Movement.Pause.performed += DoPause;
        input.Movement.Pause.Enable();

        input.Touch.MoveClick.performed += DoMoveOnClick;
        input.Touch.MoveClick.Enable();

        input.Touch.MoveTouch.performed += DoMoveOnTouch;
        input.Touch.MoveTouch.Enable();
    }

    public void Cleanup()
    {
        input.Movement.Interact.performed -= DoInteract;
        input.Movement.Interact.Disable();

        input.Movement.Kill.performed -= DoKill;
        input.Movement.Kill.Disable();

        input.Movement.Pause.performed -= DoPause;
        input.Movement.Pause.Disable();

        input.Touch.MoveTouch.performed -= DoMoveOnTouch;
        input.Touch.MoveTouch.Disable();

        input.Touch.MoveClick.performed -= DoMoveOnClick;
        input.Touch.MoveClick.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.Instance.IsGamePaused)
        {
            dir = agent.destination - transform.position;
            
            //Check on targets
            if(targetNPC)
            {
                //In range
                if(Vector3.Distance(transform.position, targetNPC.transform.position) <= interactRadius)
                {
                    animator.SetTrigger("Consume");
                    agent.enabled = false;
                    transform.position = targetNPC.transform.position;
                    NPCManager.Instance.KillNPC(targetNPC);
                    GameManager.Instance.UpdateScore();
                    agent.enabled = true;
                    targetNPC = null;
                }
            }

            else if(targetHS)
            {
                //In range
                if (Vector3.Distance(transform.position, targetHS.transform.position) <= interactRadius)
                {
                    EnterHidingObject(targetHS);
                    targetHS = null;
                }
            }

            //Flip sprite based on direction
            if (dir.x < 0)
            {
                sr.flipX = true;
            }
            else if (dir.x > 0)
            {
                sr.flipX = false;
            }
        }
    }

    //On tap/click create sphere to check for npcs/hidding spot
    //proceed with the closest object
    //if no object, then move

    void DoMoveOnClick(InputAction.CallbackContext obj)
    {
        //If we can interact
        if (bIsInteractEnabled)
        {
            //Create ray
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            //Check it hit something
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //If we are hiding, leave
                if (objectHidingIn)
                {
                    ExitHidingObject(objectHidingIn);
                    //agent.SetDestination(hit.point);
                    return;
                }

                //Check for npcs and hidding spots
                Collider2D[] interactibles = Physics2D.OverlapCircleAll(transform.position, interactRadius / 2, interactLayer);
                foreach (Collider2D col in interactibles)
                {
                    //Hiding spot
                    if (col.TryGetComponent<HidingObject>(out HidingObject hs))
                    {
                        targetHS = hs;
                        targetNPC = null;
                        agent.SetDestination(targetHS.transform.position);
                        return;
                    }

                    //NPC
                    else if (col.TryGetComponent<NPCBehaviour>(out NPCBehaviour npc))
                    {
                        targetNPC = npc;
                        targetHS = null;
                        agent.SetDestination(targetNPC.transform.position);
                        return;
                    }
                }
                //Ground is the target
                agent.SetDestination(hit.point);
                targetNPC = null;
                targetHS = null;
            }
        }
    }

    void DoMoveOnTouch(InputAction.CallbackContext obj)
    {
        //If we can interact
        if (bIsInteractEnabled)
        {
            //Create ray
            Ray ray = Camera.main.ScreenPointToRay(obj.ReadValue<Vector2>());
            //Check it hit something
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //If we are hiding, leave
                if (objectHidingIn)
                {
                    ExitHidingObject(objectHidingIn);
                    //agent.SetDestination(hit.point);
                    return;
                }

                //Check for npcs and hidding spots
                Collider2D[] interactibles = Physics2D.OverlapCircleAll(transform.position, interactRadius / 2, interactLayer);
                foreach (Collider2D col in interactibles)
                {
                    //Hiding spot
                    if (col.TryGetComponent<HidingObject>(out HidingObject hs))
                    {
                        targetHS = hs;
                        targetNPC = null;
                        agent.SetDestination(targetHS.transform.position);
                        return;
                    }

                    //NPC
                    else if (col.TryGetComponent<NPCBehaviour>(out NPCBehaviour npc))
                    {
                        targetNPC = npc;
                        targetHS = null;
                        agent.SetDestination(targetNPC.transform.position);
                        return;
                    }
                }
                //Ground is the target
                agent.SetDestination(hit.point);
                targetNPC = null;
                targetHS = null;
            }
        }
    }

    void DoInteract(InputAction.CallbackContext obj)
    {
        // Interact is disabled while in the middle of interacting want to prevent spamming 'E'
        if (bIsInteractEnabled)
        {
            //If we are already hiding in an object we will exit
            if (objectHidingIn)
            {
                ExitHidingObject(objectHidingIn);
                return;
            }
            else
            {
                // Are we within the radius of interacting with the HidingObject?
                Collider2D[] Collisions = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactLayer);
                foreach (Collider2D ValidCollision in Collisions)
                {
                    HidingObject NewHidingObject = null;
                    if (ValidCollision.TryGetComponent<HidingObject>(out NewHidingObject))
                    {
                        EnterHidingObject(NewHidingObject);
                        return;
                    }
                }
            }
        }
    }

    void DoKill(InputAction.CallbackContext obj)
    {
        // If the slime is hiding don't let them kill an NPC
        if (IsSlimeHidden() == true)
        {
            return;
        }
        // TODO this has an issue with PlayerController still being referenced after destruction
        Collider2D[] InteractableCollisions = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactLayer);

        foreach (Collider2D Colliders in InteractableCollisions)
        {
            NPCBehaviour NPC;
            if (Colliders.TryGetComponent(out NPC))
            {
                animator.SetTrigger("Consume");
                transform.position = NPC.gameObject.transform.position;
                NPCManager.Instance.KillNPC(NPC);
                GameManager.Instance.UpdateScore();
                return;
            }
        }
    }

    void DoPause(InputAction.CallbackContext obj)
    {
        GameManager.Instance.PauseGame();
    }

    public bool IsSlimeHidden()
    {
        return (objectHidingIn);
    }

    public void FreezePlayer()
    {
        bIsMovementEnabled = false;
        agent.isStopped = true;
        agent.enabled = false;
    }

    private void UnFreezePlayer()
    {
        bIsMovementEnabled = true;
        agent.isStopped = false;
        agent.enabled = true;
    }

    #region INTERACTING

    void EnterHidingObject(HidingObject NewHidingObject)
    {
        if (!animator.GetBool(NewHidingObject.GetAnimationBool()))
        {
            //Animation
            animator.SetTrigger(NewHidingObject.GetAnimationTrigger());
            animator.SetBool(NewHidingObject.GetAnimationBool(), true);
            //Disable Sprite renderer for HidingObject and Enter it
            NewHidingObject.GetComponent<SpriteRenderer>().enabled = false;
            NewHidingObject.EnteredObject(this);
            objectHidingIn = NewHidingObject;
            //Disable movement
            bIsInteractEnabled = false;
            FreezePlayer();
            //Reposition
            Vector3 DirectionFacingBin = NewHidingObject.transform.position - transform.position;
            transform.position = NewHidingObject.transform.position + (DirectionFacingBin.normalized) / 4;
            bIsInteractEnabled = true;
        }
    }
    void ExitHidingObject(HidingObject CurrentHidingObject)
    {
        if (animator.GetBool(CurrentHidingObject.GetAnimationBool()))
        {
            bIsInteractEnabled = false;
            animator.SetBool(CurrentHidingObject.GetAnimationBool(), false);
            CurrentHidingObject.ExitedObject();
        }

    }
    public void E_ExitedHidingObject()
    {
        objectHidingIn.GetComponent<SpriteRenderer>().enabled = true;
        objectHidingIn = null;
        //Enable movement
        UnFreezePlayer();
        bIsInteractEnabled = true;
    }
    public void TeleportToNextHidingObject(HidingObject NewHidingObject)
    {
        //Cleanup old HidingObject
        objectHidingIn.ExitedObject();
        objectHidingIn.GetComponent<SpriteRenderer>().enabled = true;

        //Start new HidingObject
        this.transform.position = NewHidingObject.transform.position;
        NewHidingObject.GetComponent<SpriteRenderer>().enabled = false;
        NewHidingObject.EnteredObject(this);
        objectHidingIn = NewHidingObject;

    }

    public void E_InteractSound()
    {
        AudioManager.instance.PlaySoundFromSource("Interact", audioSource);
    }

    #endregion

    public void E_ConsumeSound()
    {
        AudioManager.instance.PlaySoundFromSource("Consume", audioSource);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
