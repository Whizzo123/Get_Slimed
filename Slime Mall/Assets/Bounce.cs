using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Bounce : MonoBehaviour
{
    public void BounceIn(Vector3 size, float time)
    {
        this.gameObject.transform.localScale = new Vector3(0, 0, 0);
        LeanTween.scale(this.gameObject, size, time).setDelay(0.5f).setEase(LeanTweenType.easeOutElastic);
    }
    public void EZBouncIn()
    {
        BounceIn(new Vector3(2, 2, 2), 1.5f);
    }

}
