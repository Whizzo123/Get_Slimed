using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
/*
*AUTHOR: Unknown
*EDITORS: Tanapat Somrid
*DATEOFCREATION: dd/mm/yyyy
*DESCRIPTION: Description of file and usage
*/


public class SecurityBehaviour : NPCBehaviour
{
    protected override void UpdateStateMachine()
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

    protected override void IdleState()
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

    protected override void WanderState() 
    {
        SpeedMultiplier = BaseSpeed;
        if (CheckForSlime() == true)
        {
            FoundSlime();
        }
        if (Time.time >= LastStep + WanderTime || agent.isStopped)
        {
            agent.destination = transform.position;
            ChangeState(StateMachine.IDLE);
        }
    }

    protected void ChaseState()
    {
        //Chase the slime at high speeds or stop chasing and go back to idle
        if (CheckForSlime() == true)
        {
            SpeedMultiplier = RunSpeed;
            agent.SetDestination(PlayerController.instance.GetPosition());
            if (Vector3.Distance(PlayerController.instance.GetPosition(), transform.position) <= EntitySight.GetSightRadius() / 4)
            {
                //KILL SLIME!!!
                GameManager.Instance.CapturedEndGame();
            }
        }
        else
        {
            //GetComponent<ActivatePrompt>().HideEmotion();
            agent.destination = transform.position;
            ChangeState(StateMachine.IDLE);
        }
    }

    protected override void FoundSlime()
    {
        //Stop current destination
        agent.destination = transform.position;
        AudioManager.instance.PlaySound("GuardSpot");
        ChangeState(StateMachine.CHASE);
    }

    protected override Vector3 FindDirection()
    {
        return NPCManager.Instance.FindPointForMap(this);
    }

    public void OnCollisionEnter2D(Collision2D Collision)
    {
        PlayerController PlayerController;
        Collision.gameObject.TryGetComponent<PlayerController>(out PlayerController);
        if(PlayerController)
        {
            if (!PlayerController.IsSlimeHidden() && !GameManager.Instance.IsGameFinished)
            {
                //Disconnect player from input system
                PlayerController.Cleanup();
                //Send off a call to game manager
                GameManager.Instance.CapturedEndGame();
            }
        }
    }
}

