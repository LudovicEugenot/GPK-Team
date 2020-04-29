using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialogueBoxO;
    public float initialWrittingSpeed;
    public float acceleratedWrittingSpeed;
    [Tooltip("Put writting Speeds to 1 to accurate use")] public float numberOfLetterByBeat;

    private Text dialogueText;
    private int currentDialogueStep;
    private bool isTalking;
    private bool sentenceStarted;
    private Talk currentDialogue;
    private bool canGoNext;
    private bool interactPressed;

    void Start()
    {
        isTalking = false;
        sentenceStarted = false;
        canGoNext = false;
        dialogueBoxO.SetActive(false);
        dialogueText = dialogueBoxO.GetComponentInChildren<Text>();
    }
    void Update()
    {
        UpdateDialogueState();
        interactPressed = Input.GetButton("Interact");
    }

    public void StartTalk(Talk dialogue, Transform camFocusPoint, float zoom)
    {
        if(!isTalking)
        {
            isTalking = true;
            currentDialogueStep = 0;
            sentenceStarted = false;
            dialogueBoxO.SetActive(true);
            dialogueText.text = "";
            currentDialogue = dialogue;
            StartCoroutine(GameManager.Instance.cameraHandler.StartCinematicLook(camFocusPoint.position, zoom, false));
            GameManager.Instance.playerManager.isInControl = false;
        }
    }

    void UpdateDialogueState()
    {
        if (isTalking && currentDialogue != null)
        {
            if (!sentenceStarted)
            {
                canGoNext = false;
                StartCoroutine(DisplaySentence());
                sentenceStarted = true;
            }

            if (canGoNext && Input.GetButtonDown("Interact"))
            {
                if (currentDialogueStep < currentDialogue.sentences.Length - 1)
                {
                    currentDialogueStep++;
                    sentenceStarted = false;
                }
                else
                {
                    Invoke("EndDialogue", 0.1f);
                }
            }
        }
    }
    private IEnumerator DisplaySentence()
    {
        dialogueText.text = "";
        int i = 0;
        foreach (char letter in currentDialogue.sentences[currentDialogueStep].ToCharArray())
        {
            if (!isTalking)
            {
                break;
            }
            dialogueText.text += letter;
            i++;
            if (i >= numberOfLetterByBeat)
            {
                i = 0;
                if (interactPressed)
                {
                    yield return new WaitForSeconds(GameManager.Instance.Beat.BeatTime / acceleratedWrittingSpeed);
                }
                else
                {
                    yield return new WaitForSeconds(GameManager.Instance.Beat.BeatTime / initialWrittingSpeed);
                }
            }
        }
        canGoNext = true;
    }

    private void EndDialogue()
    {
        dialogueBoxO.SetActive(false);
        isTalking = false;
        StartCoroutine(GameManager.Instance.cameraHandler.StopCinematicLook());
        GameManager.Instance.playerManager.isInControl = true;
    }
}
