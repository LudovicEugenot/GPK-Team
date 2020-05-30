using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeakerObtention : MonoBehaviour
{
    public Hook nearbyHook;
    public Talk triggeredTalk;

    private bool obtained;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (WorldManager.currentStoryStep >= WorldManager.StoryStep.SpeakerObtained)
        {
            obtained = true;
            spriteRenderer.enabled = false;
        }
    }

    void Update()
    {
        if(GameManager.Instance.blink.currentHook == nearbyHook && !obtained)
        {
            StartCoroutine(ObtainSpeaker());
        }
    }

    private IEnumerator ObtainSpeaker()
    {
        obtained = true;
        GameManager.Instance.playerManager.ownSpeaker = true;
        StartCoroutine(GameManager.Instance.cameraHandler.StartCinematicLook(GameManager.Instance.player.transform.position, 2, true));
        yield return new WaitForSeconds(2.0f);
        StartCoroutine(GameManager.Instance.cameraHandler.StopCinematicLook());
        GameManager.Instance.dialogueManager.StartTalk(triggeredTalk, GameManager.Instance.player.transform.position, 3);
        WorldManager.currentStoryStep = WorldManager.StoryStep.SpeakerObtained;
        spriteRenderer.enabled = false;
    }
}
