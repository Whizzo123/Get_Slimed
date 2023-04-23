using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBehaviour : MonoBehaviour
{
    public enum StateMachine
    {
        IDLE, WANDER, SIGHT, ESCAPE
    }
    float speed;
    float radius;

    Rigidbody2D rb;
    SpriteRenderer sr;

    Vector2 dir;

    float idleTime;
    float wanderTime;
    float lastStep;

    StateMachine myState = StateMachine.IDLE;

    bool lookingLeft => sr.flipX;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
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
    }

    void Update()
    {
        //Flip sprite based on dir
        if (dir.x < 0)
            sr.flipX = true;
        else if (dir.x > 0)
            sr.flipX = false;

        switch (myState)
        {
            case StateMachine.IDLE:
                //Idle enough
                if(Time.time >= lastStep + idleTime)
                {
                    dir = FindDirection();
                    ChangeState(StateMachine.WANDER);
                }
                break;

            case StateMachine.WANDER:
                rb.velocity = Vector2.zero;
                rb.velocity += new Vector2(dir.x, dir.y) * speed * 50 * Time.deltaTime;
                if (Time.time >= lastStep + wanderTime)
                {
                    dir = Vector2.zero;
                    rb.velocity = Vector2.zero;
                    ChangeState(StateMachine.IDLE);
                }
                break;

            case StateMachine.SIGHT:
                //Get scared
                //Call guards
                break;

            case StateMachine.ESCAPE:
                //Run opposite of slime at increased speed
                break;
        }
    }

    public Vector2 FindDirection()
    {
        Vector2 target = rb.position + Random.insideUnitCircle * radius;
        return target.normalized - rb.position.normalized;
    }

    void ChangeState(StateMachine state)
    {
        myState = state;
        lastStep = Time.time;
    }
}
