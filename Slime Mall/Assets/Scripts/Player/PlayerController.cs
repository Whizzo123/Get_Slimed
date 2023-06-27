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
    bool bIsHidden = false;
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

        //Bind input to action
        input.Movement.Pause.performed += DoPause;
        input.Movement.Pause.Enable();

        input.Touch.MoveClick.performed += DoMoveOnClick;
        input.Touch.MoveClick.Enable();

        input.Touch.MoveTouch.performed += DoMoveOnTouch;
        input.Touch.MoveTouch.Enable();
    }

    public void Cleanup()
    {
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
                    //anim?
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
                    agent.enabled = false;
                    EnterHidingObject();
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
            Move(ray);           
        }
    }

    void DoMoveOnTouch(InputAction.CallbackContext obj)
    {
        //If we can interact
        if (bIsInteractEnabled)
        {
            //Create ray
            Ray ray = Camera.main.ScreenPointToRay(obj.ReadValue<Vector2>());
            Move(ray);
        }
    }

    void Move(Ray r)
    {
        //Check it hit something
        if (Physics.Raycast(r, out RaycastHit hit))
        {
            //If we are hiding, leave
            if (targetHS)
            {
                ExitHidingObject();
                agent.enabled = true;
            }

            //Check for npcs and hidding spots
            Collider[] interactibles = Physics.OverlapSphere(hit.transform.position, interactRadius / 2, interactLayer);
            foreach (Collider col in interactibles)
            {
                //Hiding spot
                if (col.TryGetComponent<HidingObject>(out HidingObject hs))
                {
                    targetNPC = null;
                    targetHS = hs;
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

    void DoPause(InputAction.CallbackContext obj)
    {
        GameManager.Instance.PauseGame();
    }

    public bool IsSlimeHidden()
    {
        return (targetHS);
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

    void EnterHidingObject()
    {
        if (!animator.GetBool(targetHS.GetAnimationBool()))
        {
            //Animation
            animator.SetTrigger(targetHS.GetAnimationTrigger());
            animator.SetBool(targetHS.GetAnimationBool(), true);
            //Disable Sprite renderer for HidingObject and Enter it
            targetHS.GetComponent<SpriteRenderer>().enabled = false;
            targetHS.EnteredObject(this);
            //Disable movement
            bIsInteractEnabled = false;
            //FreezePlayer();
            //Reposition
            Vector3 DirectionFacingBin = targetHS.transform.position - transform.position;
            transform.position = targetHS.transform.position + (DirectionFacingBin.normalized) / 4;
            bIsInteractEnabled = true;
        }
    }
    void ExitHidingObject()
    {
        if (animator.GetBool(targetHS.GetAnimationBool()))
        {
            bIsInteractEnabled = false;
            animator.SetBool(targetHS.GetAnimationBool(), false);
            targetHS.ExitedObject();
            targetHS = null;
        }

    }
    public void E_ExitedHidingObject()
    {
        targetHS.GetComponent<SpriteRenderer>().enabled = true;
        targetHS = null;
        //Enable movement
        UnFreezePlayer();
        bIsInteractEnabled = true;
    }
    public void TeleportToNextHidingObject(HidingObject NewHidingObject)
    {
        //Cleanup old HidingObject
        targetHS.ExitedObject();
        targetHS.GetComponent<SpriteRenderer>().enabled = true;

        //Start new HidingObject
        this.transform.position = NewHidingObject.transform.position;
        NewHidingObject.GetComponent<SpriteRenderer>().enabled = false;
        NewHidingObject.EnteredObject(this);
        targetHS = NewHidingObject;

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
