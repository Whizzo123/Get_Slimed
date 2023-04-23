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
    [Range(-10f, 10f)]
    public int yOffset = 0;

    void Awake()
    {
        if (!sprite)          
        {
            if (TryGetComponent<SpriteRenderer>(out SpriteRenderer SR)) sprite = SR;
        }
        sprite.sortingOrder = (int)Mathf.Round(sprite.transform.position.y + yOffset);
    }

    void Update()
    {
        if (isSortingUpdating)
            sprite.sortingOrder = (int)Mathf.Round(sprite.transform.position.y + yOffset);
        else
            DestroyImmediate(this);
    }
}