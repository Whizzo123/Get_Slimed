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
        }
    }

    protected override void IdleState()
    {
        SpeedMultiplier = BaseSpeed;
        if (CheckForSlime() == true)
        {
            GetComponent<ActivatePrompt>().ShowEmotion();
            AudioManager.instance.PlaySoundFromSource(SpotSoundIdentifier, AudioSource);
            ChangeState(StateMachine.CHASE);
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
            GetComponent<ActivatePrompt>().ShowEmotion();
            AudioManager.instance.PlaySoundFromSource(SpotSoundIdentifier, AudioSource);
            ChangeState(StateMachine.CHASE);
        }
        if (Time.time >= LastStep + WanderTime || agent.isStopped)
        {
            agent.destination = transform.position;
            ChangeState(StateMachine.IDLE);
        }
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

