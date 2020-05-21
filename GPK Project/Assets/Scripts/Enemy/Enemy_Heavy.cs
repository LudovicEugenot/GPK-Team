using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Heavy : EnemyBase
{
    #region Initialization
    [SerializeField] [Range(0, 3)] private float movementDistance = 0.8f;
    public AnimationCurve movementCurve;
    public AnimationCurve jumpCurve;
    [SerializeField] private float jumpHeight = 2f;
    public int attackDamage;
    public int friendlyAttackDamage;
    public AnimationCurve aoeScaleCurve;
    public AnimationCurve knockbackCurve;
    [Header("Sounds")]
    public AudioClip jumpSound;
    public AudioClip attackSound;
    public AudioClip friendlyAttackSound;
    public AudioClip conversionSound;

    private bool hasAttacked;
    private GameObject attackParent;
    private CircleCollider2D attackCollider;
    private GameObject convertedAttackParent;
    private CircleCollider2D convertedAttackCollider;


    protected override EnemyBehaviour[] PassivePattern => passivePattern;
    protected override EnemyBehaviour[] TriggeredPattern => triggeredPattern;

    private float maxRadiusAttack;
    private float maxConvertedRadiusAttack;
    private int consecutiveConvertedBehaviourIndex = 0;

    private ContactFilter2D playerFilter = new ContactFilter2D();
    private ContactFilter2D enemyFilter = new ContactFilter2D();
    Vector2 endOfDash = Vector2.zero;
    #endregion


    private EnemyBehaviour[] passivePattern = new EnemyBehaviour[]
    {
        new EnemyBehaviour(EnemyState.Moving),
        new EnemyBehaviour(EnemyState.Moving),
        new EnemyBehaviour(EnemyState.Moving),
        new EnemyBehaviour(EnemyState.Moving),
        new EnemyBehaviour(EnemyState.Triggered),
        new EnemyBehaviour(EnemyState.Action)
    };

    private EnemyBehaviour[] triggeredPattern = new EnemyBehaviour[] // il a très potentiellement aucun triggeredPattern et tout dans le passive pattern
    {

    };


    protected override void Init()
    {
        attackParent = parent.Find("Attack").gameObject;
        attackCollider = parent.Find("Attack").GetComponentInChildren<CircleCollider2D>();
        maxRadiusAttack = attackParent.transform.localScale.x;
        attackParent.SetActive(false);

        convertedAttackParent = parent.Find("Converted Attack").gameObject;
        convertedAttackCollider = parent.Find("Converted Attack").GetComponent< CircleCollider2D>();
        maxConvertedRadiusAttack = convertedAttackParent.transform.localScale.x;
        convertedAttackParent.SetActive(false);

        hasAttacked = false;

        playerFilter.useTriggers = true;
        playerFilter.SetLayerMask(LayerMask.GetMask("Player"));
        enemyFilter.useTriggers = true;
        enemyFilter.SetLayerMask(LayerMask.GetMask("Enemy"));
    }

    protected override void ConvertedBehaviour()
    {
        attackParent.SetActive(false);
        if (GameManager.Instance.Beat.onBeatSingleFrame)
        {
            consecutiveConvertedBehaviourIndex++;
        }

        if (consecutiveConvertedBehaviourIndex > 6)
        {
            consecutiveConvertedBehaviourIndex = 0;
            animator.SetBool("InTheAir", false);
        }
        else if(consecutiveConvertedBehaviourIndex > 5)
        {
            animator.SetBool("InTheAir", true);
            HitAllies();
        }
        else
        {
            convertedAttackParent.SetActive(false);
        }

    }

    protected override void TriggeredBehaviour()
    {
        canBeDamaged = FalseDuringBeatProgression(0f, 0.95f);
        float progression = CurrentBeatProgressionAdjusted(1f, 0f);
        parent.position = Vector2.Lerp(positionStartOfBeat, positionStartOfBeat + new Vector2(0, jumpHeight), jumpCurve.Evaluate(progression));
        animator.SetBool("Attacking", true);
        //saute et touche le sol sur le prochain beat
    }

    protected override void ActionBehaviour()
    {
        animator.SetBool("Attacking", false);
        if (BeatManager.Instance.onBeatSingleFrame)
        {
            source.PlayOneShot(attackSound);
        }
        attackParent.SetActive(true);
        float attackScale = aoeScaleCurve.Evaluate(GameManager.Instance.Beat.currentBeatProgression) * maxRadiusAttack;
        attackParent.transform.localScale = new Vector3(attackScale, attackScale);
        if (GameManager.Instance.Beat.currentBeatProgression > 0.9f)
        {
            attackParent.SetActive(false);
        }

        List<Collider2D> colliders = new List<Collider2D>();
        if (!BeatManager.Instance.OnBeat(false, false, "-_-_-"))
        {
            Physics2D.OverlapCollider(attackCollider, playerFilter, colliders);
        }

        if (colliders.Count > 0)
        {
            GameManager.Instance.playerManager.TakeDamage(attackDamage);
        }
    }

    protected override void MovingBehaviour()
    {
        if (GameManager.Instance.Beat.onBeatSingleFrame)
        {
            source.PlayOneShot(jumpSound);
            Vector2 finalDirection = playerPositionStartOfBeat;
            while (!NoObstacleBetweenMeAndThere(finalDirection))
            {
                if (
                    !NoObstacleBetweenMeAndThere(positionStartOfBeat + Vector2.down) &&
                    !NoObstacleBetweenMeAndThere(positionStartOfBeat + Vector2.left) &&
                    !NoObstacleBetweenMeAndThere(positionStartOfBeat + Vector2.up) &&
                    !NoObstacleBetweenMeAndThere(positionStartOfBeat + Vector2.right))
                {
                    break;
                }
                finalDirection = positionStartOfBeat + new Vector2(Random.Range(-movementDistance, movementDistance), Random.Range(-movementDistance, movementDistance));
            }
            endOfDash = Vector2.ClampMagnitude(finalDirection - positionStartOfBeat, movementDistance);
        }
        canBeDamaged = FalseDuringBeatProgression(0.2f, 0.8f);

        animator.SetBool("InTheAir", !FalseDuringBeatProgression(0f, 0.5f));
        float progression = CurrentBeatProgressionAdjusted(2, 0);
        Jump(positionStartOfBeat + endOfDash, movementCurve.Evaluate(progression), jumpCurve.Evaluate(progression), 0.5f);
    }

    protected override void KnockbackBehaviour()
    {
        attackCollider.enabled = false;
        attackParent.SetActive(false);
        if ((Time.fixedTime - startKnockBackTime) < GameManager.Instance.Beat.BeatTime)
        {
            Vector2 nextKnockbackPos = (Vector3)Vector2.Lerp(knockbackStartPos, knockbackStartPos + knockback * 0.5f, knockbackCurve.Evaluate((Time.fixedTime - startKnockBackTime) / GameManager.Instance.Beat.BeatTime));
            if (!Physics2D.OverlapPoint(nextKnockbackPos + knockback.normalized * 0.5f, LayerMask.GetMask("Obstacle")))
            {
                parent.position = nextKnockbackPos;
            }
        }
    }

    protected override void OnConverted() //not done
    {
        source.PlayOneShot(conversionSound);
        if(animator != null)
        {
            animator.SetBool("Converted", true);
        }
    }

    protected override void VulnerableBehaviour() //done
    {
        // bouge pas et attends un coup
    }

    private void Jump(Vector2 destination, float translationLerp, float jumpLerp, float jumpHeightTweak)
    {
        //La hauteur du saut dépend déjà de la longueur du saut demandé donc jumpHeightTweak est juste un multiplicateur de cette valeur.
        float JumpHeight = Vector2.Distance(positionStartOfBeat, destination) / 3 * jumpHeightTweak;
        parent.position = Vector2.Lerp(positionStartOfBeat, destination, translationLerp) + Vector2.Lerp(Vector2.zero, new Vector2(0, JumpHeight), jumpLerp);
    }

    private void HitAllies()
    {
        if (BeatManager.Instance.onBeatSingleFrame)
        {
            source.PlayOneShot(friendlyAttackSound);
        }

        convertedAttackParent.SetActive(!FalseDuringBeatProgression(0, 0.9f));

        float attackScale = aoeScaleCurve.Evaluate(GameManager.Instance.Beat.currentBeatProgression) * maxConvertedRadiusAttack;
        convertedAttackParent.transform.localScale = new Vector3(attackScale, attackScale);

        List<Collider2D> colliders = new List<Collider2D>();
        if (GameManager.Instance.Beat.currentBeatProgression > 0.1f)
        {
            Physics2D.OverlapCollider(convertedAttackCollider, enemyFilter, colliders);
        }
        else
        {
            hasAttacked = false;
        }

        if (colliders.Count > 0 && !hasAttacked)
        {
            hasAttacked = true;
            foreach (Collider2D collider in colliders)
            {
                Vector2 direction = collider.transform.position - parent.position;
                direction.Normalize();
                collider.transform.parent.GetComponentInChildren<EnemyBase>().TakeDamage(friendlyAttackDamage, direction);
            }
        }
    }
}
