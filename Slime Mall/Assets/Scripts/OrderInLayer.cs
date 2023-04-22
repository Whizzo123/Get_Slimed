using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class OrderInLayer : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer sprite;
    [SerializeField]
    bool isSortingUpdating = true;

    void Awake()
    {
        if (!sprite)          
        {
            if (TryGetComponent<SpriteRenderer>(out SpriteRenderer SR)) sprite = SR;
        }
        sprite.sortingOrder = (int)Mathf.Round(sprite.transform.position.y);
    }

    void Update()
    {
        if (isSortingUpdating)
            sprite.sortingOrder = (int)Mathf.Round(sprite.transform.position.y);
        else
            DestroyImmediate(this);
    }
}