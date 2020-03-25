using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCDialogue : MonoBehaviour
{
    public Hook hookToTalk;
    public Dialogue[] dialogues;
    public GameObject textPrefab;
    public Vector2 bubbleOffset;
    public RectTransform dialogueTransform;
    public int numberOfLetterByBeat;
    public Vector2 theVaribale;

    private int currentDialogueStep;
    private bool isTalking;
    private bool sentenceStarted;
    private Dialogue currentDialogue;
    private Text dialogueText;
    private GameObject dialogueTextO;
    private bool canGoNext;
    private bool interactPressed;

    void Start()
    {
        isTalking = false;
        sentenceStarted = false;
        canGoNext = false;
    }

    void Update()
    {
        UpdateDialogueState();

        if(Input.GetKeyDown(KeyCode.J))
        {
            ProgressionManager.currentProgression = ProgressionManager.ProgressionState.Tutorial1;
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            ProgressionManager.currentProgression = ProgressionManager.ProgressionState.FirstFreedom2;
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            ProgressionManager.currentProgression = ProgressionManager.ProgressionState.VillageArrival3;
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            ProgressionManager.currentProgression = ProgressionManager.ProgressionState.VillageConverted4;
        }

        interactPressed = Input.GetButton("Blink");
    }

    void UpdateDialogueState()
    {
        if (Input.GetButtonDown("Blink") && GameManager.Instance.blink.currentHook == hookToTalk && !isTalking)
        {
            isTalking = true;
            currentDialogueStep = 0;
            sentenceStarted = false;
            currentDialogue = GetCurrentDialogue();
            dialogueTextO = Instantiate(textPrefab, Camera.main.WorldToScreenPoint((Vector2)transform.position + bubbleOffset), Quaternion.identity, dialogueTransform);
            dialogueText = dialogueTextO.GetComponent<Text>();
            Debug.Log("Dialogue started");
        }

        if (isTalking && currentDialogue != null)
        {
            if (!sentenceStarted)
            {
                canGoNext = false;
                StartCoroutine(DisplaySentence());
                Debug.Log(currentDialogue.sentences[currentDialogueStep]);
                sentenceStarted = true;
            }

            if(canGoNext && Input.GetButtonDown("Blink"))
            {
                if (currentDialogueStep < currentDialogue.sentences.Length - 1)
                {
                    currentDialogueStep++;
                    sentenceStarted = false;
                }
                else
                {
                    EndDialogue();
                }
            }
        }

        if(isTalking && GameManager.Instance.blink.currentHook != hookToTalk)
        {
            EndDialogue();
        }
    }

    private IEnumerator DisplaySentence()
    {
        dialogueText.text = "";
        int i = 0;
        foreach(char letter in currentDialogue.sentences[currentDialogueStep].ToCharArray())
        {
            dialogueText.text += letter;
            i++;
            if(i >= numberOfLetterByBeat)
            {
                i = 0;
                if(interactPressed)
                {
                    yield return new WaitForSeconds(GameManager.Instance.Beat.beatTime / theVaribale.x);
                }
                else
                {
                    yield return new WaitForSeconds(GameManager.Instance.Beat.beatTime / theVaribale.y);
                }
            }
        }
        canGoNext = true;
    }

    void EndDialogue()
    {
        Destroy(dialogueTextO);
        isTalking = false;
        Debug.Log("Dialogue ended");
    }

    private Dialogue GetCurrentDialogue()
    {
        Dialogue selectedDia = null;
        int i = 0;
        while(selectedDia == null && i < dialogues.Length)
        {
            if (dialogues[i].progressionNeeded == ProgressionManager.currentProgression)
            {
                selectedDia = dialogues[i];
            }
            i++;
        }
        return selectedDia;
    }

    [System.Serializable]
    public class Dialogue
    {
        [TextArea(3,10)]
        public string[] sentences;
        public ProgressionManager.ProgressionState progressionNeeded;
    }
}
