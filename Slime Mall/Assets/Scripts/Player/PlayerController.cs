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
    float SprintDelayTime;
    float SprintTime = 3;
    float SpeedModifier;
    bool bIsMovementEnabled = true;
    Vector2 MovementDirection = new Vector2(0, 0);

    [Header("Inteactions")]
    [SerializeField][Tooltip("Layer that the player can interact with")] LayerMask InteractLayer;
    [SerializeField][Range(0,5f)][Tooltip("Radius that player can trigger interact objects")] float InteractRadius = 1f;
    CircleCollider2D InteractCircle;
    HidingObject ObjectHidingIn;
    bool bIsInteractEnabled = true;


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

        //References
        PlayerInput = new Controls();
        
        PlayerRigidBody = GetComponent<Rigidbody2D>();
        BodyCollider = GetComponent<BoxCollider2D>();
        BodyAnimator = GetComponent<Animator>();
        PlayerAudioSource = GetComponent<AudioSource>();
        BodyRenderer = GetComponentInChildren<SpriteRenderer>();
        InteractCircle = GetComponentInChildren<CircleCollider2D>();

        //Variables
        InteractCircle.radius = InteractRadius;
        SprintTime = SprintDelay;

        //Bind movement to an action
        InputCallbacks = PlayerInput.Movement.Move;
        InputCallbacks.Enable();

        PlayerInput.Movement.Interact.performed += DoInteract;
        PlayerInput.Movement.Interact.Enable();

        PlayerInput.Movement.Kill.performed += DoKill;
        PlayerInput.Movement.Kill.Enable();

        PlayerInput.Movement.Pause.performed += DoPause;
        PlayerInput.Movement.Pause.Enable();

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
    void Update()
    {
        if(!GameManager.instance.IsGamePaused())
        {
            //Get input for direction and sprinting
            MovementDirection = InputCallbacks.ReadValue<Vector2>();
            Sprint();

            UI.instance.ChangeSprintBar(SprintTime);

            //Flip sprite based on direction
            if (MovementDirection.x < 0)
            {
                BodyRenderer.flipX = true;
            }
            else if (MovementDirection.x > 0)
            {
                BodyRenderer.flipX = false;
            }
        }
    }
    void FixedUpdate()
    {
        ProcessInput();
    }

    void DoKill(InputAction.CallbackContext obj)
    {
        // If the slime is hiding don't let them kill an NPC
        if (IsSlimeHidden() == true)
        {
            return;
        }
        // TODO this has an issue with PlayerController still being referenced after destruction
        Collider2D[] InteractableCollisions = Physics2D.OverlapCircleAll(transform.position, InteractRadius, InteractLayer);

        foreach (Collider2D Colliders in InteractableCollisions)
        {

            NPCBehaviour NPC;
            if(Colliders.TryGetComponent(out NPC))
            {
                BodyAnimator.SetTrigger("Consume");
                transform.position = NPC.gameObject.transform.position;
                NPCManager.Instance.KillNPC(NPC);
                GameManager.instance.UpdateScore();
                return;
            }
        }
    }
    void DoPause(InputAction.CallbackContext obj)
    {
        GameManager.instance.PauseGame();
    }
    void DoInteract(InputAction.CallbackContext obj)
    {
        // Interact is disabled while in the middle of interacting want to prevent spamming 'E'
        if (bIsInteractEnabled)
        {
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
    public bool IsSlimeHidden()
    {
        return (ObjectHidingIn);
    }

    public void FreezePlayer()
    {
        bIsMovementEnabled = false;
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
    }


    
    void ProcessInput()
    {
        //Movement
        if (bIsMovementEnabled)
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
    void Sprint()
    {
        if (Input.GetKey(KeyCode.LeftShift) && SprintTime > 0)
        {
            SprintDelayTime = SprintDelay;//Sprint delay is reset everytime we sprint
            SprintTime -= Time.deltaTime;
            SpeedModifier = SprintSpeed;
        }
        else
        {
            SpeedModifier = MoveSpeed;
            //Tick sprint delay time down before we start recharging SprintTime
            SprintDelayTime -= Time.deltaTime;
            if (SprintDelayTime <= 0)
            {
                SprintTime += Time.deltaTime;
                if (SprintTime >= 3)
                {
                    SprintTime = 3;
                }
            }

        }
    }

    #region INTERACTING
    void EnterHidingObject(HidingObject NewHidingObject)
    {
        if (!BodyAnimator.GetBool(NewHidingObject.GetAnimationBool()))
        {
            //Animation
            BodyAnimator.SetTrigger(NewHidingObject.GetAnimationTrigger());
            BodyAnimator.SetBool(NewHidingObject.GetAnimationBool(), true);
            //Disable Sprite renderer for HidingObject and Enter it
            NewHidingObject.GetComponent<SpriteRenderer>().enabled = false;
            NewHidingObject.EnteredObject(this);
            ObjectHidingIn = NewHidingObject;
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
        if (BodyAnimator.GetBool(CurrentHidingObject.GetAnimationBool()))
        { 
            bIsInteractEnabled = false;
            BodyAnimator.SetBool(CurrentHidingObject.GetAnimationBool(), false);
            CurrentHidingObject.ExitedObject();
        }

    }
    public void E_ExitedHidingObject()
    {
        ObjectHidingIn.GetComponent<SpriteRenderer>().enabled = true;
        ObjectHidingIn = null;
        //Enable movement
        UnFreezePlayer();
        bIsInteractEnabled = true;
    }
    public void TeleportToNextHidingObject(HidingObject NewHidingObject)
    {
        //Cleanup old HidingObject
        ObjectHidingIn.ExitedObject();
        ObjectHidingIn.GetComponent<SpriteRenderer>().enabled = true;

        //Start new HidingObject
        this.transform.position = NewHidingObject.transform.position;
        NewHidingObject.GetComponent<SpriteRenderer>().enabled = false;
        NewHidingObject.EnteredObject(this);
        ObjectHidingIn = NewHidingObject;

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

    private void UnFreezePlayer()
    {
        bIsMovementEnabled = true;
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
    }



    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawWireSphere(transform.position, InteractRadius);
    //}
}
