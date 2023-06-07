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
            case StateMachine.SIGHT:
                {
                    SightState(StateMachine.CHASE);
                    break;
                }
            case StateMachine.CHASE:
                {
                    ChaseState();
                    break;
                }
        }
    }

    public void OnCollisionEnter2D(Collision2D Collision)
    {
        PlayerController PlayerController;
        Collision.gameObject.TryGetComponent<PlayerController>(out PlayerController);
        if(PlayerController)
        {
            if (PlayerController.IsSlimeHidden() == false)
            {
                //Disconnect player from input system
                PlayerController.Cleanup();
                //Send off a call to game manager
                GameManager.instance.CapturedEndGame();
            }
        }
    }
}

