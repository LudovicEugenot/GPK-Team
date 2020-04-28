using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoloTalk : MonoBehaviour
{
    public GameObject dialogueBoxO;

    private Text monologText;

    void Start()
    {
        monologText = dialogueBoxO.GetComponentInChildren<Text>();
    }

    //public IEnumerator Talk(Dialogue monologText)
}
