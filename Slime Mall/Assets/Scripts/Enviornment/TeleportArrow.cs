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
    [SerializeField][Tooltip("Object that the arrow is pointing to")] HidingObject HS;
    SpriteRenderer sr;
    Collider col;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = false;
        col = GetComponent<Collider>();
        col.enabled = false;

        //Calculate rotation
    }

    public void Toggle()
    {
        sr.enabled = !sr.enabled;
        col.enabled = !col.enabled;
    }
}
