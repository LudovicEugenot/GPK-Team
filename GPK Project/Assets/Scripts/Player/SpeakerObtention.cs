using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeakerObtention : MonoBehaviour
{
    public Hook nearbyHook;
    public Talk triggeredTalk;
    public float pickupSpeed;

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
        GameManager.Instance.playerManager.isInControl = false;
        obtained = true;
        GameManager.Instance.playerManager.ownSpeaker = true;
        StartCoroutine(GameManager.Instance.cameraHandler.StartCinematicLook(GameManager.Instance.player.transform.position, 2, true));
        yield return new WaitForSeconds(1.0f);
        Vector2 playerDirection = GameManager.Instance.player.transform.position - transform.position;
        while (Vector2.Distance(transform.position, GameManager.Instance.player.transform.position) > 0.1f)
        {
            transform.position += (Vector3)playerDirection.normalized * pickupSpeed * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        GameManager.playerAnimator.SetTrigger("Throw");
        GameManager.Instance.dialogueManager.StartTalk(triggeredTalk, GameManager.Instance.player.transform.position, 3);
        WorldManager.currentStoryStep = WorldManager.StoryStep.SpeakerObtained;
        spriteRenderer.enabled = false;
        GameManager.Instance.playerManager.isInControl = true;
    }
}
