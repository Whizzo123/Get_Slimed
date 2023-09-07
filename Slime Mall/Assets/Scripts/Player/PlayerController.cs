using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.AI;
using static NPCManager;
using UnityEditor.UI;

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
    [SerializeField][Tooltip("Layer where arrows are located")] LayerMask arrowLayer;
    [SerializeField][Range(0, 5f)][Tooltip("Radius that player can trigger interact objects")] float interactRadius = 1f;
    bool bIsHidden = false;
    bool bIsTeleporting = false;

    Vector3 hitPos;
    bool isPressed = false;

    public static event Action OnAddScore;

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

        input.Touch.MoveClick.started += DoActionOnClick;
        input.Touch.MoveClick.performed += context =>
        {
            if (context.interaction is HoldInteraction)
            {
                isPressed = true;
            }
        };
        input.Touch.MoveClick.canceled += context =>
        {
            DoActionOnClick(context);
            isPressed = false;
        };
        input.Touch.MoveClick.Enable();

        input.Touch.MoveTouch.started += DoActionOnTouch;
        input.Touch.MoveTouch.performed += context =>
        {
            if (context.interaction is HoldInteraction)
            {
                isPressed = true;
            }
        };
        input.Touch.MoveTouch.canceled += context =>
        {
            DoActionOnTouch(context);
            isPressed = false;
        };
        input.Touch.MoveTouch.Enable();
    }

    public void Cleanup()
    {
        input.Movement.Pause.performed -= DoPause;
        input.Movement.Pause.Disable();

        input.Touch.MoveTouch.started -= DoActionOnTouch;
        input.Touch.MoveTouch.performed -= DoActionOnTouch;
        input.Touch.MoveTouch.canceled -= DoActionOnTouch;
        input.Touch.MoveTouch.Reset();
        input.Touch.MoveTouch.Disable();

        input.Touch.MoveClick.started -= DoActionOnClick;
        input.Touch.MoveClick.performed -= DoActionOnClick;
        input.Touch.MoveClick.canceled -= DoActionOnClick;
        input.Touch.MoveClick.Reset();
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

            //Not hidden and has HS
            else if((targetHS && !bIsHidden) || bIsTeleporting)
            {
                //In range
                if (Vector3.Distance(transform.position, targetHS.transform.position) <= interactRadius/2)
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

    void FixedUpdate()
    {
        if (!GameManager.Instance.IsGamePaused)
        {
            if (isPressed)
            {
                if (!agent.enabled) return;
                //Create ray
#if !UNITY_EDITOR
                Ray ray = Camera.main.ScreenPointToRay(input.Touch.MoveTouch.ReadValue<Vector2>());
#else
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
#endif
                Action(ray);
            }
        }
    }

    void DoActionOnClick(InputAction.CallbackContext obj)
    {
        if (!agent.enabled) return;
        //Create ray
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Action(ray);
    }

    void DoActionOnTouch(InputAction.CallbackContext obj)
    {
        if (!agent.enabled) return;
        //Create ray
        Ray ray = Camera.main.ScreenPointToRay(obj.ReadValue<Vector2>());
        Action(ray);
    }

    void Action(Ray r)
    {
        //Cannot exit teleportation
        if (bIsTeleporting) return;

        //Check it hit something
        if (Physics.Raycast(r, out RaycastHit hit))
        {
            hitPos = hit.point;
            //If we are hiding, check for arrows
            if (bIsHidden)
            {
                //Check for arrows
                Collider[] arrow = Physics.OverlapSphere(hit.point, 0.75f, arrowLayer);
                foreach (Collider col in arrow)
                {
                    //Arrow
                    if (col.TryGetComponent<TeleportArrow>(out TeleportArrow ta))
                    {
                        Debug.Log("Hit Arrow");
                        //Use Arrow (teleport to next HS)
                        targetHS.ArrowToggle();
                        animator.SetTrigger("StopHiding");
                        //Add teleporting anim (idea: very short but long slime, like a trail)
                        bIsTeleporting = true;
                        targetNPC = null;
                        targetHS = ta.GetHidingSpot();
                        agent.speed = moveSpeed * 6;
                        agent.SetDestination(targetHS.transform.position);
                        return;
                    }
                }

                StartCoroutine(ExitHidingObject());
            }

            //Check for npcs and hidding spots
            Collider[] interactibles = Physics.OverlapSphere(hit.point, 1, interactLayer);
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
        gameObject.SetActive(false);
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
        //Trying out new Design Pattern: Observer
        OnAddScore?.Invoke();

        targetNPC = null;
    }

    void EnterHidingObject()
    {
        agent.isStopped = true;

        agent.enabled = false;
        transform.position = targetHS.transform.position;

        switch (targetHS.GetTypeID())
        {
            //Enter Vent
            case 0: animator.SetTrigger("EnterVent");
                break;
            //Enter Bin
            case 1: animator.SetTrigger("EnterBin");
                break;
            default: animator.SetTrigger("EnterVent");
                break;
        }

        targetHS.ArrowToggle();

        bIsHidden = true;
        bIsTeleporting = false;
        //targetHS = null;
        agent.enabled = true;
        agent.speed = moveSpeed;
    }

    IEnumerator ExitHidingObject()
    {
        agent.speed = 0;
        animator.SetTrigger("StopHiding");
        targetHS.ArrowToggle();
        targetHS = null;

        yield return new WaitWhile(() => animator.GetCurrentAnimatorStateInfo(0).IsTag("Leave"));

        agent.enabled = false;
        transform.position += dir.x >= 0 ? new Vector3(1.25f, 0, 0.3f) : new Vector3(-1.25f, 0, 0.3f);
        agent.enabled = true;

        bIsHidden = false;
        agent.speed = moveSpeed;
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

    public Vector3 GetPosition() { return transform.position; }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRadius);

        if (agent)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(agent.destination, 1);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(hitPos, 1);
    }
}
