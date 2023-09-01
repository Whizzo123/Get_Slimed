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

    [Tooltip("Arrow Prefab")]
    public GameObject arrowObject;

    [Tooltip("Other spots to hide")]
    public List<HidingObject> accesibleSpots;
    List<GameObject> arrowList = new List<GameObject>();

    void Start()
    {
        if(accesibleSpots != null) 
        {
            //Spawn arrows
            foreach (HidingObject arrow in accesibleSpots)
            {
                GameObject newArrow = Instantiate(arrowObject, transform.position, Quaternion.identity, transform);
                newArrow.GetComponent<TeleportArrow>().SetHidingSpot(arrow);
                arrowList.Add(newArrow);
            }
        }
    }

    public void ArrowToggle()
    {
        //for each turn off sprite and collision
        foreach (GameObject arrow in arrowList)
        {
            arrow.SetActive(!arrow.activeSelf);
        }
    }

    public int GetTypeID()
    {
        return typeID;
    }
}
