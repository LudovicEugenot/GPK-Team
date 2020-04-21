using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HookState
{
    [HideInInspector] public bool relived;

    public void Relive()
    {
        relived = true;
    }
}
