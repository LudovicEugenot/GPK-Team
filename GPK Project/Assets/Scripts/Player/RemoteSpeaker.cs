using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemoteSpeaker : MonoBehaviour
{
    [Header("Speaker options")]
    public int maximumSpeakerBeatTime;
    public int airBeatTime;
    public float attackDamageMultiplier;
    public float knockbackDistance;
    public AnimationCurve launchCurveVisual;
    public float curveVisualForce;
    public AnimationCurve launchCurveProgression;
    public GameObject speakerPrefab;
    public GameObject onTimeParticleEffectPrefab;
    public Image cooldownDisplay;
    public GameObject attackFx;
    public GameObject attackMissFx;
    public GameObject speakerDisparitionFx;
    public bool isHook;

    private GameObject remoteSpeakerO;
    private SpeakerHook speakerHook;
    private int speakerRemainingTime;
    [HideInInspector] public bool speakerPlaced;
    [HideInInspector] public Transform speakerAttackPreview;
    private Animator speakerAnimator;

    private void Start()
    {
        speakerRemainingTime = maximumSpeakerBeatTime;
        speakerPlaced = false;
    }

    private void Update()
    {
        if(GameManager.Instance.playerManager.ownSpeaker)
        {
            cooldownDisplay.transform.parent.gameObject.SetActive(true);
            UpdateSpeaker();
        }
        else
        {
            cooldownDisplay.transform.parent.gameObject.SetActive(false);
        }
    }

    private void UpdateSpeaker()
    {
        if (speakerPlaced)
        {
            if(!isHook)
            {
                speakerHook.isDisabled = true;
            }

            if (speakerRemainingTime > 0 && GameManager.Instance.Beat.onBeatFirstFrame)
            {
                speakerRemainingTime--;
            }
            else if (speakerRemainingTime == 0 && remoteSpeakerO != null)
            {
                StartCoroutine(PickupSpeaker());
            }
        }
        else
        {
            if (Input.GetButtonDown("Blink") && !GameManager.Instance.blink.IsSelecting() && !PlayerManager.IsMouseNearPlayer() && remoteSpeakerO == null && GameManager.Instance.Beat.CanAct() && !GameManager.Instance.paused && GameManager.Instance.playerManager.isInControl)
            {
                StartCoroutine(ThrowSpeaker());
            }
        }

        cooldownDisplay.fillAmount = (float)speakerRemainingTime / (float)maximumSpeakerBeatTime;
    }

    private IEnumerator ThrowSpeaker()
    {
        GameManager.playerAnimator.SetTrigger("Throw");
        speakerRemainingTime = maximumSpeakerBeatTime;
        Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 launchPos = transform.parent.position;
        float currentLaunchTime = 0;

        remoteSpeakerO = Instantiate(speakerPrefab, transform.position, Quaternion.identity);
        speakerHook = remoteSpeakerO.GetComponent<SpeakerHook>();
        speakerAnimator = remoteSpeakerO.GetComponent<Animator>();
        speakerHook.remoteSpeaker = this;
        speakerAnimator.SetBool("Placed", false);
        speakerAttackPreview = remoteSpeakerO.transform.GetChild(0);
        speakerAttackPreview.gameObject.SetActive(false);
        while (currentLaunchTime < GameManager.Instance.Beat.BeatTime * airBeatTime)
        {
            Vector2 realPos = Vector2.Lerp(launchPos, targetPos, launchCurveProgression.Evaluate(currentLaunchTime / (GameManager.Instance.Beat.BeatTime * airBeatTime)));
            remoteSpeakerO.transform.position = new Vector2(realPos.x, realPos.y + launchCurveVisual.Evaluate(launchCurveProgression.Evaluate(currentLaunchTime / (GameManager.Instance.Beat.BeatTime * airBeatTime))) * curveVisualForce);
            yield return new WaitForFixedUpdate();
            currentLaunchTime += Time.fixedDeltaTime;
            speakerHook.isDisabled = true;
        }
        speakerHook.isDisabled = false;
        speakerPlaced = true;
        remoteSpeakerO.transform.position = targetPos;
        speakerAnimator.SetBool("Placed", true);
        speakerAnimator.GetComponent<AnimSynchronizer>().Synchronize();
        if (GameManager.Instance.Beat.OnBeat(GameManager.Instance.playerManager.playerOffBeated ,false, ""))
        {
            StartCoroutine(SpeakerEffect());
        }
    }

    public IEnumerator PickupSpeaker()
    {
        Instantiate(speakerDisparitionFx, remoteSpeakerO.transform.position, Quaternion.identity);
        Destroy(remoteSpeakerO);
        speakerRemainingTime = maximumSpeakerBeatTime;
        remoteSpeakerO = null;
        speakerAttackPreview = null;
        speakerPlaced = false;
        yield return null;
    }

    private IEnumerator SpeakerEffect()
    {
        Instantiate(onTimeParticleEffectPrefab, remoteSpeakerO.transform.position, Quaternion.identity);
        speakerRemainingTime = maximumSpeakerBeatTime;
        yield return null;
    }

    public void Attack(bool onBeat, Vector2 range, int damage, GameObject attackFx, Color attackColor, ContactFilter2D enemyFilter)
    {
        float currentAttackLength = range.x;
        Vector2 attackDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - remoteSpeakerO.transform.position;
        attackDirection.Normalize();
        float attackDirectionAngle = Vector2.SignedAngle(Vector2.right, attackDirection);
        SpriteRenderer fxSprite = Instantiate(onBeat ? attackFx : attackMissFx, (Vector2)remoteSpeakerO.transform.position + attackDirection * currentAttackLength * 0.5f, Quaternion.Euler(new Vector3(0.0f, 0.0f, attackDirectionAngle))).GetComponent<SpriteRenderer>();
        fxSprite.color = attackColor;
        List<Collider2D> colliders = new List<Collider2D>();
        Physics2D.OverlapBox((Vector2)speakerHook.transform.position + attackDirection * currentAttackLength * 0.5f, new Vector2(currentAttackLength, range.y), attackDirectionAngle, enemyFilter, colliders);
        if (colliders.Count > 0)
        {
            foreach (Collider2D collider in colliders)
            {
                EnemyBase enemy = collider.transform.parent.GetComponentInChildren<EnemyBase>();
                enemy.TakeDamage(Mathf.FloorToInt(damage * attackDamageMultiplier), attackDirection * knockbackDistance);
            }
        }
    }

    public void RotateAttackPreview()
    {
        Vector2 attackDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - remoteSpeakerO.transform.position;
        attackDirection.Normalize();
        float attackDirectionAngle = Vector2.SignedAngle(Vector2.right, attackDirection);
        speakerAttackPreview.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, attackDirectionAngle - 90));
    }
}
