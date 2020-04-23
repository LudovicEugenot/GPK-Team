using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Basic : EnemyBase
{
    #region Initialization
    [SerializeField] [Range(0, 5)] private float movementDistance = 1f;
    public AnimationCurve movementCurve;
    public AnimationCurve jumpCurve;
    public int attackDamage;

    private GameObject attackParent;
    private CircleCollider2D attackCollider;
    private Animator animator;


    protected override EnemyBehaviour[] PassivePattern => passivePattern;
    protected override EnemyBehaviour[] TriggeredPattern => triggeredPattern;

    private float maxRadiusAttack;

    private bool hasAttacked;
    private ContactFilter2D playerFilter = new ContactFilter2D();
    Vector2 endOfDash = Vector2.zero;
    Vector2 playerPositionWhenTriggered = Vector2.zero;
    #endregion


    private EnemyBehaviour[] passivePattern = new EnemyBehaviour[]
    {
        new EnemyBehaviour(EnemyState.Moving)
    };

    private EnemyBehaviour[] triggeredPattern = new EnemyBehaviour[]
    {
        new EnemyBehaviour(EnemyState.Triggered, 0.4f),
        new EnemyBehaviour(EnemyState.Action),
        new EnemyBehaviour(EnemyState.Vulnerable, true)
    };


    protected override void Init()
    {
        attackParent = parent.Find("Attack").gameObject;
        attackParent.SetActive(true);
        attackCollider = parent.GetComponentInChildren<CircleCollider2D>();
        attackParent.SetActive(false);
        playerFilter.useTriggers = true;
        playerFilter.SetLayerMask(LayerMask.GetMask("Player"));
        maxRadiusAttack = attackParent.transform.localScale.x;
        hasAttacked = false;
        animator = parent.GetComponentInChildren<Animator>();
    }

    protected override void ConvertedBehaviour()
    {
        // juste un état passif où on s'assure que l'animator est au bon endroit toussa
        // (le fait de convertir se fait pas ici)
    }

    protected override void TriggeredBehaviour()
    {
        canBeDamaged = FalseDuringBeatProgression(0.6f, 0.95f);
        float progression = CurrentBeatProgressionAdjusted(2, 0.5f);
        Jump(playerPositionWhenTriggered, progression, jumpCurve.Evaluate(progression), 2f);
        //bouge et arrive sur le prochain beat vers le joueur
    }

    protected override void ActionBehaviour()
    {
        attackParent.SetActive(true);
        float attackScale = Mathf.Lerp(maxRadiusAttack, 0, GameManager.Instance.Beat.currentBeatProgression);
        attackParent.transform.localScale = new Vector3(attackScale, attackScale);
        if (GameManager.Instance.Beat.currentBeatProgression > 0.9f)
        {
            attackParent.SetActive(false);
        }

        //zone dangeureuse autour de l'ennemi
        if(attackCollider != null)
        {
            attackCollider.enabled = true;
            List<Collider2D> colliders = new List<Collider2D>();
            if (GameManager.Instance.Beat.currentBeatProgression > 0.1f)
            {
                Physics2D.OverlapCollider(attackCollider, playerFilter, colliders);
            }
            else
            {
                hasAttacked = false;
            }
            attackCollider.enabled = false;

            if (colliders.Count > 0 && !hasAttacked)
            {
                hasAttacked = true;
                GameManager.Instance.playerManager.TakeDamage(attackDamage);
            }
        }
    }

    protected override void MovingBehaviour()
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
                finalDirection = positionStartOfBeat + new Vector2(Random.Range(-movementDistance, movementDistance), Random.Range(-movementDistance, movementDistance));
            }
            endOfDash = Vector2.ClampMagnitude(finalDirection - positionStartOfBeat, movementDistance);
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

    protected override void OnConverted()
    {
        animator.SetBool("Converted", true);
        attackParent.SetActive(false);
        GameManager.Instance.playerManager.AddMusician();
    }

    protected override void VulnerableBehaviour()
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
