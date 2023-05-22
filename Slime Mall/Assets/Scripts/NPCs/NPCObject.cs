using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public float radius = 5f;

    [Range(1f, 10f)]
    public float idleTime = 5f;
    [Range(1f, 10f)]
    public float wanderTime = 5f;

    public Sprite sprite;
    public Color spriteColour;
    public AnimatorOverrideController animator;
    public string spotSoundIdentifier;
    public GameObject npcPrefab;
    public bool killed = false;
}
