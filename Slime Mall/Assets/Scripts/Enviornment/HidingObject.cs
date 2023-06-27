using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/*
*AUTHOR: Tanapat Somrid
*EDITORS: Antonio Villalta
*DATEOFCREATION: 05/06/2023
*DESCRIPTION: The HidingObject is any GameObject in which the player can hide inside. 
*   If there are no arrows provided, it is a still hiding object and the player cannot teleport. 
*   If arrows are provided they will show and the player can press them to teleport to a different HidingObject.
*/
public class HidingObject : MonoBehaviour
{
    [SerializeField] List<TeleportArrow> TeleportingArrows = new List<TeleportArrow>();
    [SerializeField] string ObjectAnimationIdentifier = "Bin";

    public List<TeleportArrow> GetTeleportArrows()
    {
        return TeleportingArrows;
    }

    public string GetAnimationTrigger() 
    { 
        return ("Enter" + ObjectAnimationIdentifier); 
    }

    public string GetAnimationBool() 
    { 
        return ObjectAnimationIdentifier; 
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (TeleportArrow Arrow in TeleportingArrows)
        {
            Arrow.gameObject.SetActive(!Arrow.gameObject.activeInHierarchy);
        }
    }

    public void EnteredObject(PlayerController Player)
    {
        foreach (TeleportArrow Arrow in TeleportingArrows)
        {
            Arrow.gameObject.SetActive(true);
            Arrow.SubscribeToArrows(Player);
        }
    }

    public void ExitedObject()
    {
        foreach (TeleportArrow Arrow in TeleportingArrows)
        {
            Arrow.UnsubscribeToArrows();
            Arrow.gameObject.SetActive(false);
        }
    }

    [ContextMenu("Test Adding Player")]
    void TestAddingPlayer()
    {
        EnteredObject(PlayerController.instance);
    }
}
