using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemoteSpeaker : MonoBehaviour
{
    [Header("Speaker options")]
    public int initialBeatCooldown;
    public int minimumBeatCooldown;
    public AnimationCurve launchCurveVisual;
    public float curveVisualForce;
    public AnimationCurve launchCurveProgression;
    public GameObject speakerPrefab;
    public GameObject onTimeParticleEffectPrefab;
    public Image cooldownDisplay;

    private GameObject remoteSpeakerO;
    private SpeakerHook speakerHook;
    private int beatCooldownRemaining;
    private bool speakerPlaced;

    private void Start()
    {
        beatCooldownRemaining = initialBeatCooldown;
        speakerPlaced = false;
    }

    private void Update()
    {
        if(speakerPlaced)
        {
            if(beatCooldownRemaining > 0 && GameManager.Instance.Beat.onBeatFirstFrame)
            {
                beatCooldownRemaining--;
            }
            else if(beatCooldownRemaining == 0 && remoteSpeakerO != null)
            {
                StartCoroutine(PickupSpeaker());
            }
        }
        else
        {
            if(beatCooldownRemaining < initialBeatCooldown && GameManager.Instance.Beat.onBeatFirstFrame)
            {
                beatCooldownRemaining++;
            }
            else if (beatCooldownRemaining == initialBeatCooldown && Input.GetButtonDown("SecondAbility") && remoteSpeakerO == null && GameManager.Instance.Beat.CanAct() && !GameManager.Instance.paused)
            {
                StartCoroutine(ThrowSpeaker());
            }
        }

        cooldownDisplay.fillAmount = (float)beatCooldownRemaining / (float)initialBeatCooldown;
    }

    private IEnumerator ThrowSpeaker()
    {
        beatCooldownRemaining = initialBeatCooldown;
        Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float currentLaunchTime = 0;

        remoteSpeakerO = Instantiate(speakerPrefab, transform.position, Quaternion.identity);
        speakerHook = remoteSpeakerO.GetComponent<SpeakerHook>();
        speakerHook.remoteSpeaker = this;
        while (currentLaunchTime < GameManager.Instance.Beat.beatTime)
        {
            Vector2 realPos = Vector2.Lerp(transform.position, targetPos, launchCurveProgression.Evaluate(currentLaunchTime / GameManager.Instance.Beat.beatTime));
            remoteSpeakerO.transform.position = new Vector2(realPos.x, realPos.y + launchCurveVisual.Evaluate(launchCurveProgression.Evaluate(currentLaunchTime / GameManager.Instance.Beat.beatTime)) * curveVisualForce);
            yield return new WaitForFixedUpdate();
            currentLaunchTime += Time.fixedDeltaTime;
            speakerHook.isDisabled = true;
        }
        speakerHook.isDisabled = false;
        speakerPlaced = true;
        remoteSpeakerO.transform.position = targetPos;
        if (GameManager.Instance.Beat.OnBeat(false))
        {
            StartCoroutine(SpeakerEffect());
        }
    }

    public IEnumerator PickupSpeaker()
    {
        Destroy(remoteSpeakerO);
        remoteSpeakerO = null;
        speakerPlaced = false;
        yield return null;
    }

    private IEnumerator SpeakerEffect()
    {
        Instantiate(onTimeParticleEffectPrefab, remoteSpeakerO.transform.position, Quaternion.identity);
        StartCoroutine(speakerHook.CreateMusicArea());
        beatCooldownRemaining = initialBeatCooldown - minimumBeatCooldown;
        yield return null;
    }
}
