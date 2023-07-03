using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
*AUTHOR: Tanapat Somrid
*EDITORS: Antonio Villalta
*DATEOFCREATION: 05/06/2023
*DESCRIPTION: The HidingObject is any GameObject in which the player can hide inside. 
*/
public class HidingObject : MonoBehaviour
{
    [SerializeField, Tooltip("0=Vent, 1=Bin")]
    int typeID = 0;

    public List<TeleportArrow> arrowList;

    public void ArrowToggle()
    {
        //for each turn off sprite and collision
        foreach (TeleportArrow ta in arrowList)
        {
            ta.Toggle();
        }
    }

    public int GetTypeID()
    {
        return typeID;
    }
}
