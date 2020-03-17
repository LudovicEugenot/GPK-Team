using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteSpeaker : MonoBehaviour
{
    [Header("Temporary references")]
    public BeatManager beatManager;
    [Header("Speaker options")]
    public float initialCooldown;
    public AnimationCurve launchCurveVisual;
    public float curveVisualForce;
    public AnimationCurve launchCurveProgression;
    public GameObject speakerPrefab;

    private GameObject remoteSpeakerO;
    private ClassicHook speakerHook;
    private float cooldownRemaining;
    private bool speakerPlaced;

    private void Start()
    {
        cooldownRemaining = 0;
        speakerPlaced = false;
    }

    private void Update()
    {
        if (cooldownRemaining > 0)
        {
            cooldownRemaining -= Time.deltaTime;
        }
        else if (Input.GetButtonDown("SecondAbility"))
        {
            StartCoroutine(ThrowSpeaker());
        }
    }

    private IEnumerator ThrowSpeaker()
    {
        cooldownRemaining = initialCooldown;
        Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float currentLaunchTime = 0;

        remoteSpeakerO = Instantiate(speakerPrefab, transform.position, Quaternion.identity);
        speakerHook = remoteSpeakerO.GetComponent<ClassicHook>();
        speakerHook.blink = GetComponent<Blink>();
        speakerHook.beatManager = beatManager;
        while (currentLaunchTime < beatManager.beatTime)
        {
            Vector2 realPos = Vector2.Lerp(transform.position, targetPos, launchCurveProgression.Evaluate(currentLaunchTime / beatManager.beatTime));
            remoteSpeakerO.transform.position = new Vector2(realPos.x, realPos.y + launchCurveVisual.Evaluate(launchCurveProgression.Evaluate(currentLaunchTime / beatManager.beatTime)) * curveVisualForce);
            yield return new WaitForFixedUpdate();
            currentLaunchTime += Time.fixedDeltaTime;
        }
        speakerPlaced = true;
        speakerHook.converted = true;
        remoteSpeakerO.transform.position = targetPos;
    }
}
