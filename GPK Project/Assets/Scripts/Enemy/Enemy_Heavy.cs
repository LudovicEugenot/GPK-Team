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

    private GameObject attackParent;
    private CircleCollider2D attackCollider;
    private GameObject convertedAttackParent;
    private CircleCollider2D convertedAttackCollider;


    protected override EnemyBehaviour[] PassivePattern => passivePattern;
    protected override EnemyBehaviour[] TriggeredPattern => triggeredPattern;

    private float maxRadiusAttack;
    private float maxConvertedRadiusAttack;
    private int consecutiveConvertedBehaviourIndex = 0;

    private bool hasAttacked;
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
        enemyFilter.SetLayerMask(LayerMask.GetMask("Enemi"));
    }

    protected override void ConvertedBehaviour()
    {
        // Tous les 6 beats, il frappe le sol comme son attaque
        if (consecutiveConvertedBehaviourIndex > 6)
        {
            consecutiveConvertedBehaviourIndex = 0;
        }
        else if(consecutiveConvertedBehaviourIndex>5)
        {
            HitAllies(); //pas testé
        }

        if (GameManager.Instance.Beat.onBeatSingleFrame)
        {
            consecutiveConvertedBehaviourIndex++;
        }
    }

    protected override void TriggeredBehaviour()
    {
        canBeDamaged = FalseDuringBeatProgression(0f, 0.95f);
        float progression = CurrentBeatProgressionAdjusted(1f, 0f);
        parent.position = Vector2.Lerp(positionStartOfBeat, positionStartOfBeat + new Vector2(0, jumpHeight), jumpCurve.Evaluate(progression));
        //saute et touche le sol sur le prochain beat
    }

    protected override void ActionBehaviour() //not done
    {
        attackParent.SetActive(true);
        float attackScale = Mathf.Lerp(maxRadiusAttack, 0, GameManager.Instance.Beat.currentBeatProgression);
        attackParent.transform.localScale = new Vector3(attackScale, attackScale);
        if (GameManager.Instance.Beat.currentBeatProgression > 0.9f)
        {
            attackParent.SetActive(false);
        }

        //zone dangeureuse autour de l'ennemi
        playerFilter.useTriggers = true;
        playerFilter.SetLayerMask(LayerMask.GetMask("Player"));
        List<Collider2D> colliders = new List<Collider2D>();
        if (GameManager.Instance.Beat.currentBeatProgression > 0.1f)
        {
            Physics2D.OverlapCollider(attackCollider, playerFilter, colliders);
        }
        else
        {
            hasAttacked = false;
        }

        if (colliders.Count > 0 && !hasAttacked)
        {
            hasAttacked = true;
            GameManager.Instance.playerManager.TakeDamage(attackDamage);
        }
    }

    protected override void MovingBehaviour() //not done
    {
        if (GameManager.Instance.Beat.onBeatSingleFrame)
        {
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
                finalDirection = new Vector2(Random.Range(-movementDistance, movementDistance), Random.Range(-movementDistance, movementDistance));
            }
            endOfDash = Vector2.ClampMagnitude(finalDirection - positionStartOfBeat, movementDistance);
        }
        canBeDamaged = FalseDuringBeatProgression(0.1f, 0.3f);
        float progression = CurrentBeatProgressionAdjusted(3, 0);
        Jump(positionStartOfBeat + endOfDash, movementCurve.Evaluate(progression), jumpCurve.Evaluate(progression), 0.5f);
    }

    protected override void OnConverted() //not done
    {
        animator.SetBool("Converted", true);
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
        convertedAttackParent.SetActive(!FalseDuringBeatProgression(0, 0.9f));

        float attackScale = Mathf.Lerp(maxConvertedRadiusAttack, 0, GameManager.Instance.Beat.currentBeatProgression);
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
                collider.GetComponentInParent<Transform>().GetComponentInChildren<EnemyBase>().TakeDamage(); /////////////////////////// pas testé
            }
        }
    }
}
