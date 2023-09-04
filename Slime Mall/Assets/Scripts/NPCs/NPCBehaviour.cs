using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
/*
*AUTHOR: Antonio Villalta Isidro
*EDITORS: Tanapat Somrid
*DATEOFCREATION: dd/mm/yyyy
*DESCRIPTION: Description of file and usage
*/

public class NPCBehaviour : MonoBehaviour
{
    public enum StateMachine
    {
        IDLE, WANDER, ESCAPE, CHASE
    }

    //protected float Radius;
    [Header("Components")]
    protected AudioSource AudioSource;
    [SerializeField]
    protected SpriteRenderer EntitySpriteRenderer;
    protected NavMeshAgent agent;
    protected Animator EntityAnimator;
    protected NPCSightComponent EntitySight;

    protected string SpotSoundIdentifier;

    [Header("Movement")]
    protected Vector3 dir;
    [Tooltip("The highest speed the entity can move at")]protected float RunSpeed = 1;
    [Tooltip("The original speed the entity can move at")]protected float BaseSpeed = 1;
    [Tooltip("The current speed the entity moves at")] protected float SpeedMultiplier = 1;
    protected bool LookingLeft => EntitySpriteRenderer.flipX;

    [Header("States")]
    protected StateMachine MyState = StateMachine.IDLE;
    protected float IdleTime;
    protected float WanderTime;
    protected float LastStep;

    void Awake()
    {
        AudioSource = GetComponent<AudioSource>();
        EntitySpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        EntityAnimator = GetComponent<Animator>();
        EntitySight = GetComponent<NPCSightComponent>();

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        LastStep = Time.time;
    }

    void Update()
    {
        dir = agent.destination - transform.position;

        //Flip sprite based on dir
        if (dir.x < 0)
        {
            EntitySpriteRenderer.flipX = true;
        }
        else if (dir.x > 0)
        {
            EntitySpriteRenderer.flipX = false;
        }

        //This makes the Entities sight face downards when idle.
        //This is to account for the fact that when we idle, our sprite appears to be facing downwards as we don't have idle animations for other directions
        if (agent.isStopped)
        {
            EntitySight.UpdateSight(new Vector3(0, 0, -1));
        }
        else
        {
            EntitySight.UpdateSight(dir);
        }

        //Animation 
        EntityAnimator.SetFloat("Horizontal", agent.velocity.normalized.x);
        EntityAnimator.SetFloat("Vertical", agent.velocity.normalized.y);
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
            case StateMachine.ESCAPE:
                {
                    EscapeState();
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
            FoundSlime();
        }
        else if (Time.time >= LastStep + IdleTime)
        {
            agent.SetDestination(FindDirection());
            ChangeState(StateMachine.WANDER);
        }
    }

    protected virtual void WanderState()
    {
        SpeedMultiplier = BaseSpeed;
        if (CheckForSlime() == true)
        {
            FoundSlime();
        }
        if (Time.time >= LastStep + WanderTime || agent.isStopped)
        {
            ChangeState(StateMachine.IDLE);
        }
    }

    protected virtual void EscapeState()
    {
        //Runaway from the slime at high speeds or stop running away after a certain distance
        const float RunawayThresholdDistance = 7.5f;
        SpeedMultiplier = RunSpeed;
        if (Vector3.Distance(PlayerController.instance.transform.position, transform.position) >= RunawayThresholdDistance)
        {
            // GetComponent<ActivatePrompt>().HideEmotion();
            agent.destination = transform.position;
            ChangeState(StateMachine.IDLE);
        }
        else
        {
            agent.SetDestination((transform.position - PlayerController.instance.transform.position).normalized * 5);
        }
    }

    protected virtual void FoundSlime()
    {
        //GetComponent<ActivatePrompt>().ShowEmotion();
        //Stop current destination
        agent.destination = transform.position;
        AudioManager.instance.PlaySoundFromSource(SpotSoundIdentifier, AudioSource);
        ChangeState(StateMachine.ESCAPE);
    }

    protected virtual Vector3 FindDirection()
    {
        Vector3 Target = NPCManager.Instance.FindPointForMyZone(this);
        Vector3 Direction = (Target - transform.position).normalized;
        return Direction;
    }

    protected void ChangeState(StateMachine state)
    {
        MyState = state;
        LastStep = Time.time;
    }

    /// <summary>
    /// Check that we have seen the slime and the slime is not hidden
    /// </summary>
    protected bool CheckForSlime()
    {
        bool SlimeAvailableToSpot = (EntitySight.IsSlimeInRange()) && !PlayerController.instance.IsSlimeHidden();
        return SlimeAvailableToSpot;
    }

    public void ResetNPC()
    {
        ChangeState(StateMachine.IDLE);
        this.gameObject.SetActive(true);
    }
}
