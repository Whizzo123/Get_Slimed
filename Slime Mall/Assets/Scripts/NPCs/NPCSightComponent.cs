using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class NPCSightComponent : MonoBehaviour
{
    float sightRadius;
    bool seenSlime = false;

    private float TimeToWaitToSee = 0.1f;
    private float CurrentWaitingTimeToSee = 0.0f;
    private Vector3 debugWireSpherePosition = Vector3.zero;

    public void UpdateSight()
    {
        if(CurrentWaitingTimeToSee >= TimeToWaitToSee)
        {
            seenSlime = ScanSurroundings();
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

    public float GetSightRadius() { return sightRadius; }

    private bool ScanSurroundings()
    {
        debugWireSpherePosition = transform.position;

        // Any new objects add them to the list of seen things
        if(Vector3.Distance(transform.position, PlayerController.instance.GetPosition())<=sightRadius)
        {
            Debug.Log("Seen");
            return true;
        }
        return false;
    }

    public bool IsSlimeInRange()
    {
        return seenSlime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(255, 0, 200);
        Gizmos.DrawWireSphere(debugWireSpherePosition, sightRadius);
    }
}

