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
                    ChangeState(StateMachine.WANDER);
                }
                break;
        }
    }
}

