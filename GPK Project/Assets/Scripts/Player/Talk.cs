using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Talk
{
    public string pnjName;
    [TextArea(3, 10)]
    public string[] sentences;
}
