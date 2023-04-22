using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    [System.Serializable]
    public struct NPC
    {
        public NPCObject npc;
        public int amount;
    }

    public static NPCManager instance;

    public List<NPC> NPCList;
    List<GameObject> entities = new List<GameObject>();

    public GameObject NPCPrefab;

    [Range(0f, 2f)]
    public float NPCMoveSpeed = 1;

    [Range(1f, 10f)]
    public float orderOffset = 5;
    float lastOrderGiven = -10;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
        SpawnNPCs();
    }

    void Update()
    {  
        if(Time.time >= lastOrderGiven + orderOffset) 
        {
            lastOrderGiven = Time.time;
            for (int i = 0; i < entities.Count; i++)
            {
                StartCoroutine(Move(entities[i], NPCMoveSpeed));
            }
        }
    }

    public void SpawnNPCs()
    {
        for (int i = 0; i < NPCList.Count; i++)
        {           
            for (int j = 0; j < NPCList[i].amount; j++)
            {
                GameObject temp = Instantiate(NPCPrefab, transform);
                temp.GetComponent<SpriteRenderer>().sprite = NPCList[i].npc.sprite;
                temp.GetComponent<SpriteRenderer>().color = NPCList[i].npc.spriteColour;
                entities.Add(temp);
            }
        }
    }

    public Vector3 FindLocation()
    {
        return new Vector3(Random.insideUnitCircle.x, Random.insideUnitCircle.y, 0);
    }

    public IEnumerator Move(GameObject obj, float time)
    {
        Vector3 startingPos = obj.transform.position;
        Vector3 spot = 5*FindLocation() + startingPos;

        float elapsedTime = 0;

        while (elapsedTime < time)
        {
            obj.transform.position = Vector3.Lerp(startingPos, spot, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
