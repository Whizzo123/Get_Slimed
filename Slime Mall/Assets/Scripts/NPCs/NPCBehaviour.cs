using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
/*
*AUTHOR: Unknown
*EDITORS: Tanapat Somrid
*DATEOFCREATION: dd/mm/yyyy
*DESCRIPTION: Description of file and usage
*/


public class NPCBehaviour : MonoBehaviour
{
    public enum StateMachine
    {
        IDLE, WANDER, SIGHT, ESCAPE, CHASE
    }

    //protected float Radius;
    [Header("Components")]
    protected AudioSource AudioSource;
    protected Rigidbody2D EntityRigidBody;
    protected SpriteRenderer EntitySpriteRenderer;
    protected Animator EntityAnimator;
    protected NPCSightComponent EntitySight;

    protected string SpotSoundIdentifier;

    [Header("Movement")]
    protected Vector2 MovementDirection;
    [Tooltip("The highest speed the entity can move at")]protected float RunSpeed = 1;
    [Tooltip("The original speed the entity can move at")]protected float BaseSpeed = 1;
    [Tooltip("The current speed the entity moves at")] protected float SpeedMultiplier = 1;
    protected bool LookingLeft => EntitySpriteRenderer.flipX;

    [Header("States")]
    protected StateMachine MyState = StateMachine.IDLE;
    protected float IdleTime;
    protected float WanderTime;
    protected float LastStep;

    protected GameObject SpottedSlime;

    void Awake()
    {
        AudioSource = GetComponent<AudioSource>();
        EntityRigidBody = GetComponent<Rigidbody2D>();
        EntitySpriteRenderer = GetComponent<SpriteRenderer>();
        EntityAnimator = GetComponent<Animator>();
        EntitySight = GetComponent<NPCSightComponent>();
        LastStep = Time.time;
    }


    void Update()
    {
        //Flip sprite based on dir
        if (MovementDirection.x < 0)
        {
            EntitySpriteRenderer.flipX = true;
        }
        else if (MovementDirection.x > 0)
        {
            EntitySpriteRenderer.flipX = false;
        }

        //This makes the Entities sight face downards when idle.
        //This is to account for the fact that when we idle, our sprite appears to be facing downwards as we don't have idle animations for other directions
        if (MovementDirection == Vector2.zero)
        {
            EntitySight.UpdateSight(new Vector2(-1, 0));
        }
        else
        {
            EntitySight.UpdateSight(MovementDirection);
        }

        //Animation 
        EntityAnimator.SetFloat("Horizontal", EntityRigidBody.velocity.normalized.x);
        EntityAnimator.SetFloat("Vertical", EntityRigidBody.velocity.normalized.y);
    }

    private void FixedUpdate()
    {
        UpdateStateMachine();
    }

    public void GetSettings(NPCObject settings)
    {
        //Sprite
        EntitySpriteRenderer.sprite = settings.Sprite;
        EntitySpriteRenderer.color = settings.SpriteColour;
        EntityAnimator.runtimeAnimatorController = settings.Animator;
        //Sound
        SpotSoundIdentifier = settings.SpotSoundIdentifier;
        //Movement
        RunSpeed = settings.Speed;
        //Sight
        EntitySight.SetSightRadius(settings.Radius);

        //Idle Time
        float Offset = Random.Range(IdleTime, IdleTime + 5);
        IdleTime = settings.IdleTime + Offset;
        //Wander Time
        Offset = Random.Range(WanderTime, WanderTime + 5);
        WanderTime = settings.WanderTime + Offset;
    }

    protected virtual void UpdateStateMachine()
    {
        switch (MyState)
        {
            case StateMachine.IDLE:
                { 
                    IdleState();
                    break;
                }
            case StateMachine.WANDER:
                {
                    WanderState();
                    break;
                }
            case StateMachine.SIGHT:
                {
                    SightState(StateMachine.ESCAPE);
                    break;
                }
            case StateMachine.ESCAPE:
                {
                    EscapeState();
                    break;
                }
            case StateMachine.CHASE:
                {
                    ChaseState();
                    break;
                }
            default:
                {
                    ChangeState(StateMachine.IDLE);
                    break;
                }
        }
    }
    protected virtual void IdleState()
    {
        SpeedMultiplier = BaseSpeed;
        if (CheckForSlime() == true)
        {
            GetComponent<ActivatePrompt>().ShowEmotion();
            ChangeState(StateMachine.SIGHT);
        }
        else if (Time.time >= LastStep + IdleTime)
        {
            MovementDirection = FindDirection();
            ChangeState(StateMachine.WANDER);
        }
    }
    protected virtual void WanderState()
    {
        SpeedMultiplier = BaseSpeed;
        MoveNPC(MovementDirection);
        if (CheckForSlime() == true)
        {
            GetComponent<ActivatePrompt>().ShowEmotion();
            ChangeState(StateMachine.SIGHT);
        }
        if (Time.time >= LastStep + WanderTime)
        {
            MovementDirection = Vector2.zero;
            EntityRigidBody.velocity = Vector2.zero;
            ChangeState(StateMachine.IDLE);
        }
    }
    protected virtual void SightState(StateMachine Reaction)
    {
        AudioManager.instance.PlaySoundFromSource(SpotSoundIdentifier, AudioSource);
        ChangeState(Reaction);
    }
    protected virtual void EscapeState()
    {
        //Runaway from the slime at high speeds or stop running away after a certain distance
        const float RunawayThresholdDistance = 7.5f;
        SpeedMultiplier = RunSpeed;
        if (Vector2.Distance(SpottedSlime.transform.position, this.transform.position) >= RunawayThresholdDistance)
        {
            GetComponent<ActivatePrompt>().HideEmotion();
            ChangeState(StateMachine.IDLE);
        }
        else
        {
            MovementDirection = (transform.position - SpottedSlime.transform.position).normalized;
            MoveNPC(MovementDirection);
        }
    }
    protected virtual void ChaseState()
    {
        //Chase the slime at high speeds or stop chasing and go back to idle
        if (CheckForSlime() == true)
        {
            SpeedMultiplier = RunSpeed;
            MovementDirection = (SpottedSlime.transform.position - transform.position).normalized;
            MoveNPC(MovementDirection);
        }
        else
        {
            GetComponent<ActivatePrompt>().HideEmotion();
            ChangeState(StateMachine.IDLE);
            EntityRigidBody.velocity = Vector2.zero;
        }
    }
    public Vector2 FindDirection()
    {
        Vector2 Target = NPCManager.Instance.FindPointForMyZone(this);
        Vector2 Direction = (Target - EntityRigidBody.position).normalized;
        return Direction;
    }

    protected void ChangeState(StateMachine state)
    {
        MyState = state;
        LastStep = Time.time;
    }

    /// <summary>
    /// Move based on RigidBody velocity
    /// </summary>
    protected void MoveNPC(Vector2 Direction)
    {
        EntityRigidBody.velocity = Vector2.zero;
        EntityRigidBody.velocity += new Vector2(Direction.x, Direction.y) * SpeedMultiplier * 50 * Time.deltaTime;
    }
    /// <summary>
    /// Check that we have seen the slime and the slime is not hidden
    /// </summary>
    protected bool CheckForSlime()
    {
        SpottedSlime = EntitySight.PollForSeenObjectOfType<PlayerController>();
        bool SlimeAvailableToSpot = (SpottedSlime != null) && (SpottedSlime.GetComponent<PlayerController>().IsSlimeHidden() == false);
        return SlimeAvailableToSpot;
    }

    public void ResetNPC()
    {
        ChangeState(StateMachine.IDLE);
        this.gameObject.SetActive(true);
    }
}
