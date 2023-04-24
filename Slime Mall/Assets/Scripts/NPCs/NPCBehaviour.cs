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
    }

    void Update()
    {
        //Flip sprite based on dir
        if (dir.x < 0)
            sr.flipX = true;
        else if (dir.x > 0)
            sr.flipX = false;

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
                if (Time.time >= lastStep + idleTime)
                {
                    dir = FindDirection();
                    ChangeState(StateMachine.WANDER);
                }
                break;

            case StateMachine.WANDER:
                MoveNPC(dir);
                if (Time.time >= lastStep + wanderTime)
                {
                    dir = Vector2.zero;
                    rb.velocity = Vector2.zero;
                    ChangeState(StateMachine.IDLE);
                }
                break;

            case StateMachine.SIGHT:
                // Enter the chase state
                break;

            case StateMachine.CHASE:
                // Run towards slime at full speed
                break;
        }
    }

    public Vector2 FindDirection()
    {
        Vector2 target = rb.position + Random.insideUnitCircle * radius;
        return target.normalized - rb.position.normalized;
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
