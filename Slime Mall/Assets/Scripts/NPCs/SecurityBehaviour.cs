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
                //Idle enough
                if (Time.time >= lastStep + idleTime)
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
}

