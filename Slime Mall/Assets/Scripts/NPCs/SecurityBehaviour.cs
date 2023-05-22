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
                    ChangeState(StateMachine.SIGHT);
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
                    ChangeState(StateMachine.SIGHT);
                }
                else if (Time.time >= lastStep + wanderTime)
                {
                    dir = Vector2.zero;
                    rb.velocity = Vector2.zero;
                    ChangeState(StateMachine.IDLE);
                }
                break;
            case StateMachine.SIGHT:
                    speed = 5.0f;
                AudioManager.instance.PlaySoundFromSource(spotSoundIdentifier, audioSource);
                ChangeState(StateMachine.CHASE);

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
                    speed = 2.0f;
                    ChangeState(StateMachine.IDLE);
                    rb.velocity = Vector2.zero;
                }
                break;
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerController playerController;
        collision.gameObject.TryGetComponent<PlayerController>(out playerController);
        if(playerController)
        {
            if (playerController.IsSlimeHidden() == false)
            {
                //Disconnect player from input system
                playerController.Cleanup();
                //Send off a call to game manager
                GameManager.instance.CapturedEndGame();
            }
        }
    }
}

