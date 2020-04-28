using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCDialogue : MonoBehaviour
{
    public Hook hookToTalk;
    public Transform cinematicLookPos;
    [Range(0.0f, 10.0f)] public float cinematicLookZoom;
    public Dialogue[] dialogues;
    public GameObject dialogueBoxO;
    public int numberOfLetterByBeat;
    public Vector2 accelerations;

    private int currentDialogueStep;
    private bool isTalking;
    private bool sentenceStarted;
    private Dialogue currentDialogue;
    private Text dialogueText;
    private bool canGoNext;
    private bool interactPressed;

    void Start()
    {
        isTalking = false;
        sentenceStarted = false;
        canGoNext = false;
        dialogueText = dialogueBoxO.GetComponentInChildren<Text>();
    }

    void Update()
    {
        UpdateDialogueState();

        interactPressed = Input.GetButton("Interact");
    }

    void UpdateDialogueState()
    {
        if (Input.GetButtonDown("Interact") && GameManager.Instance.blink.currentHook == hookToTalk && !isTalking && !GameManager.Instance.paused)
        {
            currentDialogue = GetCurrentDialogue();
            if(currentDialogue != null)
            {
                StartDialogue();
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

            if(canGoNext && Input.GetButtonDown("Interact"))
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
                    yield return new WaitForSeconds(GameManager.Instance.Beat.BeatTime / accelerations.x);
                }
                else
                {
                    yield return new WaitForSeconds(GameManager.Instance.Beat.BeatTime / accelerations.y);
                }
            }
        }
        canGoNext = true;
    }

    void StartDialogue()
    {
        isTalking = true;
        currentDialogueStep = 0;
        sentenceStarted = false;
        dialogueBoxO.SetActive(true);
        dialogueText.text = "";
        StartCoroutine(GameManager.Instance.cameraHandler.StartCinematicLook(cinematicLookPos.position, cinematicLookZoom, false));
        GameManager.Instance.playerManager.isInControl = false;
    }

    void EndDialogue()
    {
        dialogueBoxO.SetActive(false);
        isTalking = false;
        StartCoroutine(GameManager.Instance.cameraHandler.StopCinematicLook());
        GameManager.Instance.playerManager.isInControl = true;
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
}
