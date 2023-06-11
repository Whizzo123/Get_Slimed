using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/*
*AUTHOR: Tanapat Somrid
*EDITORS:
*DATEOFCREATION: 05/06/2023
*DESCRIPTION: The Teleport arrow will contain the corresponding HidingObject that, once this arrow is pressed, the player will teleport to.
*   It does this via callback functions with UnityEvents.
*   TODO: Add in interaction with the actual gameoject and once pressed call InteractedWith();
*/

public class TeleportArrow : MonoBehaviour
{
    [SerializeField][Tooltip("Object that the arrow is pointing to")] HidingObject NextHidingObject;

    //Should call when the sprite has been pressed
    [ContextMenu("Has been pressed")]
    public void InteractedWith()
    {
        TeleportEvent.Invoke(NextHidingObject);
    }

    public class TeleportToNewHidingObject : UnityEvent<HidingObject>
    {
    }
    TeleportToNewHidingObject TeleportEvent;
    public void SubscribeToArrows(PlayerController playerController)
    {
        if (TeleportEvent == null)
        {
            TeleportEvent = new TeleportToNewHidingObject();
        }

        //TeleportEvent.AddListener(playerController.TeleportToNextHidingObject);
    }
    public void UnsubscribeToArrows()
    {
        TeleportEvent.RemoveAllListeners();
    }

}
