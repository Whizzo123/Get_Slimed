using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
/*
*AUTHOR: Antonio Villalta Isidro
*EDITORS: Tanapat Somrid
*DATEOFCREATION: dd/mm/yyyy
*DESCRIPTION: Description of file and usage
*/
//TODO: We need a way to ensure that Guards are spawned in every zone OR that they can roam to every zone

public class NPCManager : MonoBehaviour
{
    [System.Serializable]
    public struct NPC
    {
        public NPCObject NPCType;
        public int Amount;
    }

    public static NPCManager Instance;

    public List<NPC> NPCList;
    Dictionary<NPCBehaviour, int> NPCInZone = new Dictionary<NPCBehaviour, int>();
    public BoxCollider[] SpawnZones = new BoxCollider[3];

    public NPCObject guardPrefab;
    public int guardsPerZone = 1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        SpawnNPCs();
    }

    public Vector3 FindPointForMyZone(NPCBehaviour npc)
    {
        int zone = NPCInZone[npc];
        float movePointX = Random.Range(SpawnZones[zone].bounds.min.x, SpawnZones[zone].bounds.max.x)/* + SpawnZones[zone].transform.position.x + SpawnZones[zone].center.x*/;
        float movePointZ = Random.Range(SpawnZones[zone].bounds.min.z, SpawnZones[zone].bounds.max.z)/* + SpawnZones[zone].transform.position.z + SpawnZones[zone].center.y*/;
        return new Vector3(movePointX, 0, movePointZ);
    }

    public Vector3 FindPointForMap(NPCBehaviour npc) 
    {
        float movePointX = Random.Range(-25, 25);
        float movePointZ = Random.Range(10, 40);
        return new Vector3(movePointX, 0, movePointZ);
    }

    public void SpawnNPCs()
    {
        //Spawn based on NPCList types and how many of the types there are
        for (int i = 0; i < NPCList.Count; i++)
        {           
            for (int j = 0; j < NPCList[i].Amount; j++)
            {
                //Get random points and zones
                int RandomSpawn = Random.Range(0, SpawnZones.Length);
                Vector3 SpawnPoint = GetRandomPointInZone(RandomSpawn);

                //Spawn new NPC
                GameObject NewNPC = Instantiate(NPCList[i].NPCType.NpcPrefab, SpawnPoint, Quaternion.identity, transform);
                NewNPC.GetComponent<NPCBehaviour>().GetSettings(NPCList[i].NPCType);

                NPCInZone.Add(NewNPC.GetComponent<NPCBehaviour>(), RandomSpawn);
            }
        }

        //Spawn guards in each zone
        for (int i = 0; i < SpawnZones.Length; i++)
        {
            for (int j = 0; j < guardsPerZone; j++)
            {
                //Get random points in current zone
                Vector3 SpawnPoint = GetRandomPointInZone(i);

                //Spawn new guards
                GameObject NewNPC = Instantiate(guardPrefab.NpcPrefab, SpawnPoint, Quaternion.identity, transform);
                NewNPC.GetComponent<NPCBehaviour>().GetSettings(guardPrefab);
            }
        }
    }

    public void KillNPC(NPCBehaviour KilledNPC)
    {
        if (NPCInZone.ContainsKey(KilledNPC))
        {
            MoveNPCToAlternateZone(KilledNPC, NPCInZone[KilledNPC]);
        }
        else
        {
            Destroy(KilledNPC);
            Debug.LogError("NPC Does not exist within a zone");
        }
    }

    private void MoveNPCToAlternateZone(NPCBehaviour NPCToMove, int OriginalZone)
    {
        //Get random other spawn
        int NewSpawnZone = GetRandomSpawnZone(OriginalZone);
        Vector3 NewPoint = GetRandomPointInZone(NewSpawnZone);

        //Move NPC and ready it
        NPCToMove.transform.position = NewPoint;
        NPCToMove.ResetNPC();

    }
    private int GetRandomSpawnZone(int SpawnPointToNotUse)
    {
        Debug.Log("Not using:" + SpawnPointToNotUse);
        //Populate list with all spawnpoint indexes and then remove the one we do not want to use
        List<int> SpawnZoneIndex = new List<int>(SpawnZones.Length);
        for (int i = 0; i < SpawnZones.Length; i++)
        {
            SpawnZoneIndex.Add(i);
        }
        SpawnZoneIndex.RemoveAt(SpawnPointToNotUse);

        //Choose a random index out of the ones left and grab the index from spawnzones left
        int NewSpawnZone = Random.Range(0, SpawnZoneIndex.Count);
        NewSpawnZone = SpawnZoneIndex[NewSpawnZone];
        Debug.Log("Returned:" + NewSpawnZone);
        return NewSpawnZone;
    }
    private Vector3 GetRandomPointInZone(int Zone)
    {
        float SpawnPointX = Random.Range(SpawnZones[Zone].bounds.min.x, SpawnZones[Zone].bounds.max.x);
        float SpawnPointZ = Random.Range(SpawnZones[Zone].bounds.min.z, SpawnZones[Zone].bounds.max.z);
        return new Vector3(SpawnPointX, 0, SpawnPointZ);
    }
}
