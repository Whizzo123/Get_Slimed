using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static PlayerController;
/*
*AUTHOR: Antonio Vilalta Isidro
*EDITORS: Tanapat Somrid
*DATEOFCREATION: NA
*DESCRIPTION: 
*/
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    [Header("Components")]
    Controls PlayerInput;
    InputAction InputCallbacks;
    Animator BodyAnimator;
    Rigidbody2D PlayerRigidBody;
    BoxCollider2D BodyCollider;
    AudioSource PlayerAudioSource;
    SpriteRenderer BodyRenderer;

    [Header("Movement")]
    [SerializeField][Range(1f, 100f)] [Tooltip("Movement Speed")] 
    float MoveSpeed = 5f;
    [SerializeField][Range(1f, 100f)][Tooltip("Sprinting Speed")] 
    float SprintSpeed = 15.0f;
    [SerializeField][Range(1f, 10f)][Tooltip("Delay before recharging the Sprint Time after sprint is used")] 
    float SprintDelay = 2.0f;
    float SpeedModifier;
    float SprintTime;
    Vector2 MovementDirection = new Vector2(0, 0);

    [Header("Inteactions")]
    [SerializeField][Tooltip("Layer that the player can interact with")] LayerMask InteractLayer;
    [SerializeField][Range(0,5f)][Tooltip("Radius that player can trigger interact objects")] float InteractRadius = 1f;
    CircleCollider2D InteractCircle;

    HidingObject ObjectHidingIn;
    bool isMovementEnabled = true;

    bool isInteractEnabled = true;

    float speedJuice = 3;




    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }


        PlayerInput = new Controls();
        PlayerRigidBody = GetComponent<Rigidbody2D>();
        BodyCollider = GetComponent<BoxCollider2D>();
        BodyRenderer = GetComponentInChildren<SpriteRenderer>();
        BodyAnimator = GetComponent<Animator>();
        PlayerAudioSource = GetComponent<AudioSource>();
        InteractCircle = GetComponentInChildren<CircleCollider2D>(); 

        InteractCircle.radius = InteractRadius;

        //Bind movement to an action
        InputCallbacks = PlayerInput.Movement.Move;
        InputCallbacks.Enable();

        PlayerInput.Movement.Interact.performed += DoInteract;
        PlayerInput.Movement.Interact.Enable();

        PlayerInput.Movement.Kill.performed += DoKill;
        PlayerInput.Movement.Kill.Enable();

        PlayerInput.Movement.Pause.performed += DoPause;
        PlayerInput.Movement.Pause.Enable();

        SprintTime = SprintDelay;
    }

    // Update is called once per frame
    void Update()
    {
        if(!GameManager.instance.IsGamePaused())
        {
            //Get dir
            MovementDirection = InputCallbacks.ReadValue<Vector2>();
            if (Input.GetKey(KeyCode.LeftShift))
            {

                SprintTime = SprintDelay;
                speedJuice -= Time.deltaTime;
                if (speedJuice <= 0)
                {
                    SpeedModifier = MoveSpeed;
                    speedJuice = 0;
                }
                else
                {
                    SpeedModifier = SprintSpeed;
                }
            }
            else
            {

                SprintTime -= Time.deltaTime;
                if (SprintTime <= 0)
                {
                    speedJuice += Time.deltaTime;
                    if (speedJuice >= 3)
                    {
                        speedJuice = 3;
                    }
                }

                SpeedModifier = MoveSpeed;
            }
            UI.instance.ChangeSprintBar(speedJuice);
            //Flip sprite based on dir
            if (MovementDirection.x < 0)
                BodyRenderer.flipX = true;
            else if (MovementDirection.x > 0)
                BodyRenderer.flipX = false;
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
            PlayerRigidBody.velocity = Vector2.zero;
            PlayerRigidBody.velocity += new Vector2(MovementDirection.x, MovementDirection.y) * SpeedModifier * 100 * Time.deltaTime;
            BodyAnimator.SetFloat("Horizontal", MovementDirection.x);
            BodyAnimator.SetFloat("Vertical", MovementDirection.y);
        }
        else
        {
            PlayerRigidBody.velocity = Vector2.zero;
        }
    }
    #region INTERACTING
    void EnterHidingObject(HidingObject NewHidingObject)
    {
        if (!BodyAnimator.GetBool(NewHidingObject.GetAnimationBool()))
        {
            BodyAnimator.SetTrigger(NewHidingObject.GetAnimationTrigger());
            BodyAnimator.SetBool(NewHidingObject.GetAnimationBool(), true);
            NewHidingObject.GetComponent<SpriteRenderer>().enabled = false;
            NewHidingObject.EnteredObject(this);
            ObjectHidingIn = NewHidingObject;
            //Disable movement
            isInteractEnabled = false;
            FreezePlayer();
            //Reposition
            Vector3 directionFacingBin = NewHidingObject.transform.position - transform.position;
            transform.position = NewHidingObject.transform.position + (directionFacingBin.normalized) / 4;
            isInteractEnabled = true;
        }
    }
    void ExitHidingObject(HidingObject NewHidingObject)
    {
        if (BodyAnimator.GetBool(NewHidingObject.GetAnimationBool()))
        { 
            isInteractEnabled = false;
            BodyAnimator.SetBool(NewHidingObject.GetAnimationBool(), false);
        }

    }
    public void E_ExitedHidingObject()
    {
        ObjectHidingIn.ExitedObject();
        ObjectHidingIn.GetComponent<SpriteRenderer>().enabled = true;
        ObjectHidingIn = null;
        //Enable movement
        UnFreezePlayer();
        isInteractEnabled = true;
    }
    public void TeleportToNextHidingObject(HidingObject obj)
    {
        Debug.Log("Teleporting to object");
        if (obj)
        {
            Debug.Log("Teleporting to " + obj.name);

        }
    }
    void DoInteract(InputAction.CallbackContext obj)
    {
        // Interact is disabled while in the middle of interacting want to prevent spamming 'E'
        if (isInteractEnabled)
        {
            Debug.Log("Interact");
            //If we are already hiding in an object we will exit
            if (ObjectHidingIn)
            {
                ExitHidingObject(ObjectHidingIn);
                return;
            }
            else
            {
                // Are we within the radius of interacting with the HidingObject?
                Collider2D[] Collisions = Physics2D.OverlapCircleAll(transform.position, InteractRadius, InteractLayer);
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

    public void E_InteractSound()
    {
        AudioManager.instance.PlaySoundFromSource("Interact", PlayerAudioSource);
    }
    #endregion
    public void E_ConsumeSound()
    {
        AudioManager.instance.PlaySoundFromSource("Consume", PlayerAudioSource);
    }


    void DoKill(InputAction.CallbackContext obj)
    {
        // If the slime is hiding don't let them kill an NPC
        if(IsSlimeHidden() == true)
        {
            return;
        }
        // TODO this has an issue with PlayerController still being referenced after destruction
        Collider2D[] colls = Physics2D.OverlapCircleAll(transform.position, InteractRadius, InteractLayer);

        foreach(Collider2D col in colls)
        {
            if(col.CompareTag("Killable"))
            {
                BodyAnimator.SetTrigger("Consume");
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
        return (ObjectHidingIn);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, InteractRadius);
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
        PlayerInput.Movement.Interact.performed -= DoInteract;
        PlayerInput.Movement.Interact.Disable();

        PlayerInput.Movement.Kill.performed -= DoKill;
        PlayerInput.Movement.Kill.Disable();

        PlayerInput.Movement.Pause.performed -= DoPause;
        PlayerInput.Movement.Pause.Disable();
    }

    

}




//public delegate void TeleportToNewHidingObject<in HidingObject>(HidingObject NewObject);

//public void HidingInObject()
//{
//    List<TeleportArrow> HidingObjectArrows = ObjectHidingIn.GetTeleportArrows();
//    foreach (TeleportArrow Arrows in HidingObjectArrows)
//    {
//        Arrows.
//        }
//}
//public static void TeleportToNewHidingObjectsMethod(HidingObject NextHidingObject)
//{
//    Debug.Log("Callback worked");
//    //Cleanup old Hiding object
//    //Teleport to new hiding object
//    //Setup new hiding object
//}