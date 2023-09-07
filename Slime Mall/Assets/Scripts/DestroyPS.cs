using CartoonFX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyPS : MonoBehaviour
{
    void Start()
    {
        Invoke("DestroyObject", GetComponent<ParticleSystem>().main.duration);
    }

    void DestroyObject()
    {
        Destroy(gameObject);
    }
}
