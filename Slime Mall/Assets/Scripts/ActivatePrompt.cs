using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatePrompt : MonoBehaviour
{
    public GameObject prompt;

    void Start()
    {
        if (!prompt) prompt = transform.GetChild(0).gameObject;

        prompt.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.CompareTag("PlayerArea"))
        {
            prompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("PlayerArea"))
        {
            prompt.SetActive(false);
        }
    }
}
