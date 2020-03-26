using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemoteSpeaker : MonoBehaviour
{
    [Header("Temporary references")]
    public BeatManager beatManager;
    [Header("Speaker options")]
    public int initialBeatCooldown;
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
            if(beatCooldownRemaining > 0 && beatManager.onBeatFirstFrame)
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
            if(beatCooldownRemaining < initialBeatCooldown && beatManager.onBeatFirstFrame)
            {
                beatCooldownRemaining++;
            }
            else if (beatCooldownRemaining == initialBeatCooldown && Input.GetButtonDown("SecondAbility") && remoteSpeakerO == null)
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
        speakerHook.blink = GetComponent<Blink>();
        speakerHook.remoteSpeaker = this;
        while (currentLaunchTime < beatManager.beatTime)
        {
            Vector2 realPos = Vector2.Lerp(transform.position, targetPos, launchCurveProgression.Evaluate(currentLaunchTime / beatManager.beatTime));
            remoteSpeakerO.transform.position = new Vector2(realPos.x, realPos.y + launchCurveVisual.Evaluate(launchCurveProgression.Evaluate(currentLaunchTime / beatManager.beatTime)) * curveVisualForce);
            yield return new WaitForFixedUpdate();
            currentLaunchTime += Time.fixedDeltaTime;
            speakerHook.isDisabled = true;
        }
        speakerHook.isDisabled = false;
        speakerPlaced = true;
        remoteSpeakerO.transform.position = targetPos;
        if (beatManager.OnBeat())
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
        // conversion ennemi dans un cercle
        yield return null;
    }
}
