using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialogueBoxO;
    public float initialWrittingSpeed;
    public float acceleratedWrittingSpeed;
    public AudioClip[] letterClips;
    public int letterNumberBySound;
    [Tooltip("Put writting Speeds to 1 to accurate use")] public float numberOfLetterByBeat;

    private Text dialogueText;
    private Text pnjNameText;
    private int currentDialogueStep;
    [HideInInspector] public bool isTalking;
    private bool sentenceStarted;
    private Talk currentDialogue;
    private bool canGoNext;
    private bool interactPressed;
    private AudioSource voiceSource;
    private bool isCommentary;
    private float autoSkipTime;
    private bool autoSkipFlag;
    private Vector2 initialBoxPos;
    private RectTransform boxPos;

    void Start()
    {
        isTalking = false;
        sentenceStarted = false;
        canGoNext = false;
        dialogueBoxO.SetActive(false);
        dialogueText = dialogueBoxO.transform.GetChild(0).GetComponentInChildren<Text>();
        pnjNameText = dialogueBoxO.transform.GetChild(1).GetComponentInChildren<Text>();
        voiceSource = GetComponent<AudioSource>();
        boxPos = dialogueBoxO.GetComponent<RectTransform>();
        initialBoxPos = boxPos.anchoredPosition;
    }
    void Update()
    {
        UpdateDialogueState();
        interactPressed = Input.GetButton("Blink");
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
            pnjNameText.text = dialogue.pnjName;
            currentDialogue = dialogue;
            StartCoroutine(GameManager.Instance.cameraHandler.StartCinematicLook(camFocusPoint.position, zoom, false));
            GameManager.Instance.playerManager.isInControl = false;
            GameManager.Instance.PauseEnemyBehaviour();
            isCommentary = false;
            autoSkipFlag = true;
            boxPos.anchoredPosition = initialBoxPos;
        }
    }

    public void StartCommentary(Talk talk, float timeBeforeNextSentence, Vector2 altBoxPos)
    {
        if (!isTalking)
        {
            isTalking = true;
            currentDialogueStep = 0;
            sentenceStarted = false;
            dialogueBoxO.SetActive(true);
            dialogueText.text = "";
            pnjNameText.text = talk.pnjName;
            currentDialogue = talk;
            isCommentary = true;
            autoSkipTime = timeBeforeNextSentence;
            autoSkipFlag = true;
            if(altBoxPos == Vector2.zero)
            {
                boxPos.anchoredPosition = initialBoxPos;
            }
            else
            {
                boxPos.anchoredPosition = altBoxPos;
            }
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

            if (canGoNext && autoSkipFlag)
            {
                if (Input.GetButtonDown("Blink"))
                {
                    NextSentence();
                }
                else if(isCommentary)
                {
                    autoSkipFlag = false;
                    Invoke("NextSentence", autoSkipTime);
                }
            }
        }
    }
    private IEnumerator DisplaySentence()
    {
        dialogueText.text = "";
        int i = 0;
        int l = 0;
        foreach (char letter in currentDialogue.sentences[currentDialogueStep].ToCharArray())
        {
            if (!isTalking)
            {
                break;
            }

            if(l <= 0)
            {
                voiceSource.PlayOneShot(letterClips[Random.Range(0,letterClips.Length - 1)]);
                l = letterNumberBySound;
            }
            else
            {
                l--;
            }

            dialogueText.text += letter;
            i++;
            if (i >= numberOfLetterByBeat)
            {
                i = 0;
                if (interactPressed && !isCommentary)
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
        GameManager.Instance.UnpauseEnemyBehaviour();
    }

    private void NextSentence()
    {
        autoSkipFlag = true;
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
