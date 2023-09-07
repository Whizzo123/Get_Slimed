using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
*AUTHOR: Unknown
*EDITORS: Tanapat Somrid
*DATEOFCREATION: dd/mm/yyyy
*DESCRIPTION: Description of file and usage
*/

public enum IntelligenceScore
{
    DUMB,NORMAL,SMART
}

[CreateAssetMenu(fileName = "NPC", menuName = "NPC")]
public class NPCObject : ScriptableObject
{
    public IntelligenceScore intScore;
    
    [Range(1f,100f)]
    public float Speed = 5f;
    [Range(1f, 10f)]
    public float Radius = 5f;

    [Range(1f, 10f)]
    public float IdleTime = 5f;
    [Range(1f, 10f)]
    public float WanderTime = 5f;

    public Sprite Sprite;
    public Color SpriteColour;
    public AnimatorOverrideController Animator;
    public string SpotSoundIdentifier;
    public GameObject NpcPrefab;
    public bool Killed = false;
    public string audioTrackName;
}
