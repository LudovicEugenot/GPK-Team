using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Heavy : EnemyBase
{
    #region Initialization
    [SerializeField] [Range(0, 3)] private float movementDistance = 0.5f;
    public AnimationCurve movementCurve;
    public AnimationCurve jumpCurve;
    public int attackDamage;

    private GameObject attackParent;
    public CircleCollider2D attackCollider;
    public Animator animator;


    protected override EnemyBehaviour[] PassivePattern => passivePattern;
    protected override EnemyBehaviour[] TriggeredPattern => triggeredPattern;

    private float maxRadiusAttack;
    private int consecutiveConvertedBehaviourIndex = 0;

    private bool hasAttacked;
    private ContactFilter2D playerFilter = new ContactFilter2D();
    Vector2 endOfDash = Vector2.zero;
    #endregion


    private EnemyBehaviour[] passivePattern = new EnemyBehaviour[]
    {
        new EnemyBehaviour(EnemyState.Moving)
    };

    private EnemyBehaviour[] triggeredPattern = new EnemyBehaviour[] // il a très potentiellement aucun triggeredPattern et tout dans le passive pattern
    {
        new EnemyBehaviour(EnemyState.Triggered,0.6f),
        new EnemyBehaviour(EnemyState.Action),
        new EnemyBehaviour(EnemyState.Action),
        new EnemyBehaviour(EnemyState.Vulnerable, true)
    };


    protected override void Init() //done
    {
        attackParent = parent.Find("Attack").gameObject;
        attackCollider = FindComponentInHierarchy<CircleCollider2D>();
        maxRadiusAttack = attackParent.transform.localScale.x;
        attackParent.SetActive(false);
        hasAttacked = false;
    }

    protected override void ConvertedBehaviour() //almost done
    {
        // Tous les 6 beats, il frappe le sol comme son attaque
        if (consecutiveConvertedBehaviourIndex > 6)
        {
            consecutiveConvertedBehaviourIndex = 0;
        }
        else if(consecutiveConvertedBehaviourIndex>5)
        {
            attackParent.SetActive(!FalseDuringBeatProgression(0, 0.5f));
        }

        if (GameManager.Instance.Beat.onBeatSingleFrame)
        {
            consecutiveConvertedBehaviourIndex++;
        }
    }

    protected override void TriggeredBehaviour() //not done
    {
        canBeDamaged = FalseDuringBeatProgression(0.6f, 0.95f);
        float progression = CurrentBeatProgressionAdjusted(2, 0.5f);
        Jump(playerPositionWhenTriggered, progression, jumpCurve.Evaluate(progression), 2f);
        //bouge et arrive sur le prochain beat vers le joueur
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
            endOfDash = Vector2.ClampMagnitude(playerPositionStartOfBeat - positionStartOfBeat, movementDistance);
            while (!NoObstacleBetweenMeAndThere(endOfDash))
            {
                if (
                    !NoObstacleBetweenMeAndThere(Vector2.down) &&
                    !NoObstacleBetweenMeAndThere(Vector2.left) &&
                    !NoObstacleBetweenMeAndThere(Vector2.up) &&
                    !NoObstacleBetweenMeAndThere(Vector2.right))
                {
                    break;
                }
                endOfDash = new Vector2(Random.Range(-movementDistance, movementDistance), Random.Range(-movementDistance, movementDistance));
            }
        }
        canBeDamaged = FalseDuringBeatProgression(0.2f, 0.8f);
        float progression = CurrentBeatProgressionAdjusted(2, 0);
        Jump(positionStartOfBeat + endOfDash, movementCurve.Evaluate(progression), jumpCurve.Evaluate(progression), 0.5f);

        if (PlayerIsInAggroRange())
        {
            playerPositionWhenTriggered = player.position;
            GetTriggered();
        }
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
}
