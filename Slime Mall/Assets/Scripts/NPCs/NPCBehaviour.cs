using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBehaviour : MonoBehaviour
{
    public enum StateMachine
    {
        IDLE, WANDER, SIGHT, ESCAPE, CHASE
    }
    protected float speed;
    protected float radius;
    protected string spotSoundIdentifier;
    protected AudioSource audioSource;

    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected Animator animator;
    protected NPCSightComponent sight;
    protected Vector2 dir;

    protected float idleTime;
    protected float wanderTime;
    protected float lastStep;

    protected StateMachine myState = StateMachine.IDLE;
    protected GameObject spottedSlime;
    protected bool lookingLeft => sr.flipX;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        sight = GetComponent<NPCSightComponent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        lastStep = Time.time;
    }

    public void GetSettings(NPCObject settings)
    {
        sr.sprite = settings.sprite;
        sr.color = settings.spriteColour;

        speed = settings.Speed;
        radius = settings.radius;

        float offset = Random.Range(idleTime, idleTime + 5);
        idleTime = settings.idleTime + offset;
        offset = Random.Range(wanderTime, wanderTime + 5);
        wanderTime = settings.wanderTime + offset;
        animator.runtimeAnimatorController = settings.animator;
        sight.SetSightRadius(settings.radius);

        spotSoundIdentifier = settings.spotSoundIdentifier;
        Debug.Log(settings.spotSoundIdentifier);
    }

    void Update()
    {
        //Flip sprite based on dir
        if (dir.x < 0)
            sr.flipX = true;
        else if (dir.x > 0)
            sr.flipX = false;
        if (dir == Vector2.zero)
        {
            sight.UpdateSight(new Vector2(-1, 0));
        }
        else
        {
            sight.UpdateSight(dir);
        }
        UpdateStateMachine();

        animator.SetFloat("Horizontal", rb.velocity.normalized.x);
        animator.SetFloat("Vertical", rb.velocity.normalized.y);
    }

    public virtual void UpdateStateMachine()
    {
        switch (myState)
        {
            case StateMachine.IDLE:
                //Idle enough
                    speed = 10f;
                if(CheckForSlime() == true)
                {
                    GetComponent<ActivatePrompt>().ShowEmotion();
                    ChangeState(StateMachine.SIGHT);
                }
                else if (Time.time >= lastStep + idleTime)
                {
                    dir = FindDirection();
                    ChangeState(StateMachine.WANDER);
                }
                break;

            case StateMachine.WANDER:
                    speed = 10f;
                MoveNPC(dir);
                if (CheckForSlime() == true)
                {
                    GetComponent<ActivatePrompt>().ShowEmotion();
                    ChangeState(StateMachine.SIGHT);
                }
                if (Time.time >= lastStep + wanderTime)
                {
                    dir = Vector2.zero;
                    rb.velocity = Vector2.zero;
                    ChangeState(StateMachine.IDLE);
                }
                break;

            case StateMachine.SIGHT:
                AudioManager.instance.PlaySoundFromSource(spotSoundIdentifier, audioSource);
                ChangeState(StateMachine.ESCAPE);
                break;
            case StateMachine.ESCAPE:
                const float runawayThresholdDistance = 7.5f;
                    speed = 20;
                if (Vector2.Distance(spottedSlime.transform.position, this.transform.position) >= runawayThresholdDistance)
                {
                    GetComponent<ActivatePrompt>().HideEmotion();
                    ChangeState(StateMachine.IDLE);
                }
                else
                {
                    dir = (transform.position - spottedSlime.transform.position).normalized;
                    MoveNPC(dir);
                }
                break;
        }
    }

    public Vector2 FindDirection()
    {
        Vector2 target = NPCManager.instance.FindPointForMyZone(this);
        return (target - rb.position).normalized;
    }

    protected void ChangeState(StateMachine state)
    {
        myState = state;
        lastStep = Time.time;
    }

    protected void MoveNPC(Vector2 dir)
    {
        rb.velocity = Vector2.zero;
        rb.velocity += new Vector2(dir.x, dir.y) * speed * 50 * Time.deltaTime;
    }

    protected bool CheckForSlime()
    {
        spottedSlime = sight.PollForSeenObjectOfType<PlayerController>();
        return spottedSlime != null && spottedSlime.GetComponent<PlayerController>().IsSlimeHidden() == false;
    }
}
