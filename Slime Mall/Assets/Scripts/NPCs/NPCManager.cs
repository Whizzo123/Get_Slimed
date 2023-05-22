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
    Dictionary<NPCBehaviour, int> npcToZone = new Dictionary<NPCBehaviour, int>();
    public BoxCollider2D[] spawnAreas = new BoxCollider2D[3];
    [Range(0f, 2f)]
    public float NPCMoveSpeed = 1;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
        SpawnNPCs();
    }

    public Vector2 FindPointForMyZone(NPCBehaviour npc)
    {
        int zone = npcToZone[npc];
        float movePointX = Random.Range(spawnAreas[zone].bounds.min.x, spawnAreas[zone].bounds.max.x);
        float movePointY = Random.Range(spawnAreas[zone].bounds.min.y, spawnAreas[zone].bounds.max.y);
        return new Vector2(movePointX, movePointY);
    }

    public void SpawnNPCs()
    {
        for (int i = 0; i < NPCList.Count; i++)
        {           
            for (int j = 0; j < NPCList[i].amount; j++)
            {
                int spawnZone = Random.Range(0, spawnAreas.Length);
                float spawnPointX = Random.Range(spawnAreas[spawnZone].bounds.min.x, spawnAreas[spawnZone].bounds.max.x);
                float spawnPointY = Random.Range(spawnAreas[spawnZone].bounds.min.y, spawnAreas[spawnZone].bounds.max.y);
                GameObject temp = Instantiate(NPCList[i].npc.npcPrefab, new Vector3(spawnPointX, spawnPointY, 0), Quaternion.identity, transform);
                temp.GetComponent<NPCBehaviour>().GetSettings(NPCList[i].npc);
                entities.Add(temp.GetComponent<NPCBehaviour>());
                npcToZone.Add(temp.GetComponent<NPCBehaviour>(), spawnZone);
            }
        }
    }

    private void Update()
    {
        if(entities.Count < 14)
        {
            for (int i = 0; i < 6; i++)
            {
                int spawnZone = Random.Range(0, 2);
                float spawnPointX = Random.Range(spawnAreas[spawnZone].bounds.min.x, spawnAreas[spawnZone].bounds.max.x);
                float spawnPointY = Random.Range(spawnAreas[spawnZone].bounds.min.y, spawnAreas[spawnZone].bounds.max.y);
                int npcIndex = Random.Range(1, 3);
                GameObject temp = Instantiate(NPCList[npcIndex].npc.npcPrefab, new Vector3(spawnPointX, spawnPointY, 0), Quaternion.identity, transform);
                temp.GetComponent<NPCBehaviour>().GetSettings(NPCList[npcIndex].npc);
                entities.Add(temp.GetComponent<NPCBehaviour>());
                npcToZone.Add(temp.GetComponent<NPCBehaviour>(), spawnZone);
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
                npcToZone.Remove(obj);
                Destroy(obj.gameObject);
                return;
            }
        }
    }
}
