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

    [Header("Interactions")]
    [SerializeField][Tooltip("Layer that the player can interact with")] LayerMask interactLayer;
    [SerializeField][Range(0, 5f)][Tooltip("Radius that player can trigger interact objects")] float interactRadius = 1f;
    bool bIsHidden = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        //References
        input = new Controls();
        sr = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        //Variables

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
                    KillNPC();
                }
            }

            else if(targetHS)
            {
                //In range
                if (Vector3.Distance(transform.position, targetHS.transform.position) <= interactRadius)
                {
                    EnterHidingObject();
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
        //Create ray
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Move(ray);
    }

    void DoMoveOnTouch(InputAction.CallbackContext obj)
    {
        //Create ray
        Ray ray = Camera.main.ScreenPointToRay(obj.ReadValue<Vector2>());
        Move(ray);
    }

    void Move(Ray r)
    {
        //Check it hit something
        if (Physics.Raycast(r, out RaycastHit hit))
        {
            //If we are hiding, leave
            if (bIsHidden)
            {
                ExitHidingObject();
            }

            //Check for npcs and hidding spots
            Collider[] interactibles = Physics.OverlapSphere(hit.transform.position, 1, interactLayer);
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
        return (bIsHidden);
    }

    public void FreezePlayer()
    {
        agent.isStopped = true;
        agent.enabled = false;
    }

    private void UnFreezePlayer()
    {
        agent.isStopped = false;
        agent.enabled = true;
    }

    #region INTERACTING

    void KillNPC()
    {
        animator.SetTrigger("Consume");

        agent.enabled = false;
        transform.position = targetNPC.transform.position;
        agent.enabled = true;

        NPCManager.Instance.KillNPC(targetNPC);
        GameManager.Instance.UpdateScore();

        targetNPC = null;
    }

    void EnterHidingObject()
    {
        if (!animator.GetBool(targetHS.GetAnimationBool()))
        {
            //Animation
            animator.SetTrigger(targetHS.GetAnimationTrigger());
            animator.SetBool(targetHS.GetAnimationBool(), true);

            //Disable Sprite renderer for HidingObject and Enter it
            targetHS.GetComponent<SpriteRenderer>().enabled = false;
            //targetHS.EnteredObject(this);

            //Stop movement
            agent.isStopped = true;

            //Reposition
            agent.enabled = false;
            transform.position = targetHS.transform.position + (dir.normalized) / 4;
            agent.enabled = true;
            bIsHidden = true;
        }
    }
    void ExitHidingObject()
    {
        if (animator.GetBool(targetHS.GetAnimationBool()))
        {
            animator.SetBool(targetHS.GetAnimationBool(), false);
            targetHS.GetComponent<SpriteRenderer>().enabled = true;
            //targetHS.ExitedObject();           
        }
    }
    public void E_ExitedHidingObject()
    {
        targetHS = null;
        bIsHidden = false;
        agent.enabled = true;
        //Enable movement
        UnFreezePlayer();
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

    public void E_ConsumeSound()
    {
        AudioManager.instance.PlaySoundFromSource("Consume", audioSource);
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
