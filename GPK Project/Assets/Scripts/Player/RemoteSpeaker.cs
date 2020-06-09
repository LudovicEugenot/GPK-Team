using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemoteSpeaker : MonoBehaviour
{
    [Header("Speaker options")]
    public int initialBeatCooldown;
    public int minimumBeatCooldown;
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

    private GameObject remoteSpeakerO;
    private SpeakerHook speakerHook;
    private int beatCooldownRemaining;
    [HideInInspector] public bool speakerPlaced;
    //private Animator animator;
    private Animator speakerAnimator;

    private void Start()
    {
        //animator = transform.parent.GetComponentInChildren<Animator>();
        beatCooldownRemaining = initialBeatCooldown;
        speakerPlaced = false;
    }

    private void Update()
    {
        if(GameManager.Instance.playerManager.ownSpeaker)
        {
            UpdateSpeaker();
        }
    }

    private void UpdateSpeaker()
    {
        if (speakerPlaced)
        {
            if (beatCooldownRemaining > 0 && GameManager.Instance.Beat.onBeatFirstFrame)
            {
                beatCooldownRemaining--;
            }
            else if (beatCooldownRemaining == 0 && remoteSpeakerO != null)
            {
                StartCoroutine(PickupSpeaker());
            }
        }
        else
        {
            if (beatCooldownRemaining < initialBeatCooldown && GameManager.Instance.Beat.onBeatFirstFrame)
            {
                beatCooldownRemaining++;
            }
            else if (beatCooldownRemaining == initialBeatCooldown && Input.GetButtonDown("Blink") && !GameManager.Instance.blink.IsSelecting() && !PlayerManager.IsMouseNearPlayer() && remoteSpeakerO == null && GameManager.Instance.Beat.CanAct() && !GameManager.Instance.paused && GameManager.Instance.playerManager.isInControl)
            {
                Debug.Log("Thrown");
                StartCoroutine(ThrowSpeaker());
            }
        }

        cooldownDisplay.fillAmount = (float)beatCooldownRemaining / (float)initialBeatCooldown;
    }

    private IEnumerator ThrowSpeaker()
    {
        GameManager.playerAnimator.SetTrigger("Throw");
        beatCooldownRemaining = initialBeatCooldown;
        Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 launchPos = transform.parent.position;
        float currentLaunchTime = 0;

        remoteSpeakerO = Instantiate(speakerPrefab, transform.position, Quaternion.identity);
        speakerHook = remoteSpeakerO.GetComponent<SpeakerHook>();
        speakerAnimator = remoteSpeakerO.GetComponent<Animator>();
        speakerHook.remoteSpeaker = this;
        speakerAnimator.SetBool("Placed", false);
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
        Destroy(remoteSpeakerO);
        remoteSpeakerO = null;
        speakerPlaced = false;
        yield return null;
    }

    private IEnumerator SpeakerEffect()
    {
        Instantiate(onTimeParticleEffectPrefab, remoteSpeakerO.transform.position, Quaternion.identity);
        beatCooldownRemaining = initialBeatCooldown - minimumBeatCooldown;
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
}
