using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class SecurityBehaviour : NPCBehaviour
{

    public override void UpdateStateMachine()
    {
        switch (myState)
        {
            case StateMachine.IDLE:
                if (CheckForSlime() == true)
                {
                    GetComponent<ActivatePrompt>().ShowEmotion();
                    speed += 2.5f;
                    ChangeState(StateMachine.CHASE);
                }
                //Idle enough
                else if (Time.time >= lastStep + idleTime)
                {
                    dir = FindDirection();
                    ChangeState(StateMachine.WANDER);
                }
                break;

            case StateMachine.WANDER:
                MoveNPC(dir);
                if(CheckForSlime() == true)
                {
                    GetComponent<ActivatePrompt>().ShowEmotion();
                    speed += 2.5f;
                    ChangeState(StateMachine.CHASE);
                }
                else if (Time.time >= lastStep + wanderTime)
                {
                    dir = Vector2.zero;
                    rb.velocity = Vector2.zero;
                    ChangeState(StateMachine.IDLE);
                }
                break;

            case StateMachine.CHASE:
                // Have we seen the slime and is he not hidden
                if(CheckForSlime() == true)
                {
                    dir = (spottedSlime.transform.position - transform.position).normalized;
                    MoveNPC(dir);
                }
                else
                {
                    GetComponent<ActivatePrompt>().HideEmotion();
                    speed -= 2.5f;
                    ChangeState(StateMachine.IDLE);
                    rb.velocity = Vector2.zero;
                }
                break;
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.GetComponent<PlayerController>())
        {
            //Send off a call to game manager
            GameManager.instance.CapturedEndGame();
        }
    }
}

