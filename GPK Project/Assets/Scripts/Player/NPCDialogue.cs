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
            WorldManager.currentStoryStep = WorldManager.StoryStep.Tutorial1;
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            WorldManager.currentStoryStep = WorldManager.StoryStep.FirstFreedom2;
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            WorldManager.currentStoryStep = WorldManager.StoryStep.VillageArrival3;
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            WorldManager.currentStoryStep = WorldManager.StoryStep.VillageConverted4;
        }

        interactPressed = Input.GetButton("Blink");
    }

    void UpdateDialogueState()
    {
        if (Input.GetButtonDown("Blink") && GameManager.Instance.blink.currentHook == hookToTalk && !isTalking && !GameManager.Instance.paused)
        {
            currentDialogue = GetCurrentDialogue();
            if(currentDialogue != null)
            {
                isTalking = true;
                currentDialogueStep = 0;
                sentenceStarted = false;
                dialogueTextO = Instantiate(textPrefab, Camera.main.WorldToScreenPoint((Vector2)transform.position + bubbleOffset), Quaternion.identity, dialogueTransform);
                dialogueText = dialogueTextO.GetComponent<Text>();
            }
        }

        if (isTalking && currentDialogue != null)
        {
            if (!sentenceStarted)
            {
                canGoNext = false;
                StartCoroutine(DisplaySentence());
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
            if(!isTalking)
            {
                break;
            }
            dialogueText.text += letter;
            i++;
            if(i >= numberOfLetterByBeat)
            {
                i = 0;
                if(interactPressed)
                {
                    yield return new WaitForSeconds(GameManager.Instance.Beat.BeatTime / theVaribale.x);
                }
                else
                {
                    yield return new WaitForSeconds(GameManager.Instance.Beat.BeatTime / theVaribale.y);
                }
            }
        }
        canGoNext = true;
    }

    void EndDialogue()
    {
        Destroy(dialogueTextO);
        isTalking = false;
    }

    private Dialogue GetCurrentDialogue()
    {
        Dialogue selectedDia = null;
        int i = 0;
        while(selectedDia == null && i < dialogues.Length)
        {
            if (dialogues[i].progressionNeeded == WorldManager.currentStoryStep)
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
        public WorldManager.StoryStep progressionNeeded;
    }
}
