using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    List<NPCBehaviour> entities = new List<NPCBehaviour>();

    [Range(0f, 2f)]
    public float NPCMoveSpeed = 1;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
        SpawnNPCs();
    }

    public void SpawnNPCs()
    {
        for (int i = 0; i < NPCList.Count; i++)
        {           
            for (int j = 0; j < NPCList[i].amount; j++)
            {
                GameObject temp = Instantiate(NPCList[i].npc.npcPrefab, new Vector3(Random.insideUnitCircle.x * 5, Random.insideUnitCircle.y * 5, 0), Quaternion.identity, transform);
                temp.GetComponent<NPCBehaviour>().GetSettings(NPCList[i].npc);
                entities.Add(temp.GetComponent<NPCBehaviour>());
            }
        }
    }

    private void Update()
    {
        if(entities.Count < 14)
        {
            for (int i = 0; i < 6; i++)
            {
                int npcIndex = Random.Range(1, 2);
                GameObject temp = Instantiate(NPCList[npcIndex].npc.npcPrefab, new Vector3(Random.insideUnitCircle.x * 5, Random.insideUnitCircle.y * 5, 0), Quaternion.identity, transform);
                temp.GetComponent<NPCBehaviour>().GetSettings(NPCList[npcIndex].npc);
                entities.Add(temp.GetComponent<NPCBehaviour>());
            }
        }
    }

    public void KillNPC(GameObject npc)
    {
        foreach(NPCBehaviour obj in entities)
        {
            //NPC we want to kill
            if(obj.gameObject == npc)
            {
                entities.Remove(obj);
                Destroy(obj.gameObject);
                return;
            }
        }
    }
}
