using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationFX : MonoBehaviour
{
    public AnimationClip fxAnimation;

    void Start()
    {
        Invoke("AutoDestroy", fxAnimation.length);
    }

    void AutoDestroy()
    {
        Destroy(gameObject);
    }

}
