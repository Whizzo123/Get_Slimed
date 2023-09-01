using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
/*
*AUTHOR: Tanapat Somrid
*EDITORS: Antonio Villalta
*DATEOFCREATION: 05/06/2023
*DESCRIPTION: The Teleport arrow will contain the corresponding HidingObject that, once this arrow is pressed, the player will teleport to.
*   It does this via callback functions with UnityEvents.
*   TODO: Add in interaction with the actual gameoject and once pressed call InteractedWith();
*/

public class TeleportArrow : MonoBehaviour
{
    HidingObject HS;

    public void SetHidingSpot(HidingObject hs)
    {
        HS = hs;
        Vector3 target = hs.transform.position;
        target.y = 0f;

        target.x = target.x - transform.position.x;
        target.z = target.z - transform.position.z;

        float angle = Mathf.Atan2(target.z, target.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(90, 0, angle));

        Vector3 dir = (hs.transform.position - transform.position).normalized;
        dir.y = 1;
        transform.position += dir;
    }

    public HidingObject GetHidingSpot()
    {
        return HS;
    }
}
