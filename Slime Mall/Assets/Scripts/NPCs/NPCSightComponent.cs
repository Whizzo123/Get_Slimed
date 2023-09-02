using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class NPCSightComponent : MonoBehaviour
{
    float sightRadius;
    List<GameObject> seenGameObjects;

    public LayerMask seeableObjectsLayer;
    private float TimeToWaitToSee = 0.1f;
    private float CurrentWaitingTimeToSee = 0.0f;
    private Vector3 debugWireSpherePosition = Vector3.zero;

    private void Start()
    {
        seenGameObjects = new List<GameObject>();
    }

    public void UpdateSight(Vector3 dir)
    {
        if(CurrentWaitingTimeToSee >= TimeToWaitToSee)
        {
            ScanSurroundings(dir);
            CurrentWaitingTimeToSee = 0.0f;
        }
        else
        {
            CurrentWaitingTimeToSee += Time.deltaTime;
        }
    }

    public void SetSightRadius(float newSightRadius)
    {
        sightRadius = newSightRadius;
    }

    public GameObject PollForSeenObjectOfType<T>()
    {
        foreach(GameObject seenObject in seenGameObjects)
        {
            if (seenObject != null)
            {
                T component;
                seenObject.TryGetComponent<T>(out component);
                if (component != null)
                {
                    return seenObject;
                }
            }
        }
        return null;
    }

    private void ScanSurroundings(Vector3 facingDirection)
    {
        Debug.Log("Scanning");
        seenGameObjects.Clear();
        debugWireSpherePosition = transform.position;

        // Any new objects add them to the list of seen things
        if(Vector3.Distance(transform.position, PlayerController.instance.GetPosition())<=sightRadius)
        {
            Debug.Log("Seen");
            seenGameObjects.Add(PlayerController.instance.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(255, 0, 200);
        Gizmos.DrawWireSphere(debugWireSpherePosition, sightRadius);
    }
}

