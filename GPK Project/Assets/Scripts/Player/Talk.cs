using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Talk
{
    public string pnjName;
    public float pitch = 1;
    [TextArea(3, 10)]
    public string[] sentences;
}
