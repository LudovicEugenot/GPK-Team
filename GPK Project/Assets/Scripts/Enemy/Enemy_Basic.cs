using System.Collections;
using UnityEngine;

public class Enemy_Basic : EnemyBase
{
    #region Initialization
    [SerializeField] [Range(0, 5)] private float movementDistance = 1f;
    public AnimationCurve movementCurve;
    public AnimationCurve jumpCurve;

    private GameObject attackParent;
    private CircleCollider2D attackCollider;
    private SpriteRenderer attackRenderer;


    protected override EnemyBehaviour[] PassivePattern => passivePattern;
    protected override EnemyBehaviour[] TriggeredPattern => triggeredPattern;

    private float maxRadiusAttack;
    #endregion


    private EnemyBehaviour[] passivePattern = new EnemyBehaviour[]
    {
        new EnemyBehaviour(EnemyState.Moving)
    };

    private EnemyBehaviour[] triggeredPattern =  new EnemyBehaviour[]
    {
        new EnemyBehaviour(EnemyState.Triggered, 0.4f),
        new EnemyBehaviour(EnemyState.Action),
        new EnemyBehaviour(EnemyState.Vulnerable, true)
    };


    protected override void Init()
    {
        attackParent = parent.Find("Attack").gameObject;
        attackCollider = FindComponentInHierarchy<CircleCollider2D>();
        attackRenderer = FindComponentInHierarchy<SpriteRenderer>("Attack");
        attackParent.SetActive(false);
        maxRadiusAttack = attackParent.transform.localScale.x;
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
        Jump(playerPositionStartOfBeat, progression, jumpCurve.Evaluate(progression), 2f);
        //bouge et arrive sur le prochain beat vers le joueur
    }

    protected override void ActionBehaviour()
    {
        attackParent.SetActive(true);
        float attackScale= Mathf.Lerp(maxRadiusAttack, 0, GameManager.Instance.Beat.currentBeatProgression);
        attackParent.transform.localScale = new Vector3(attackScale,attackScale);
        if (GameManager.Instance.Beat.currentBeatProgression > 0.9)
        {
            attackParent.SetActive(false);
        }

        //zone dangeureuse autour de l'ennemi
    }

    protected override void MovingBehaviour()
    {
        canBeDamaged = FalseDuringBeatProgression(0.2f, 0.8f);
        Vector2 endOfDash = Vector2.ClampMagnitude(playerPositionStartOfBeat- positionStartOfBeat, movementDistance);
        while (!NoObstacleBetweenMeAndThere(endOfDash))
        {
            endOfDash = endOfDash + new Vector2(Random.Range(-movementDistance, movementDistance), Random.Range(-movementDistance, movementDistance));
        }
        float progression = CurrentBeatProgressionAdjusted(2, 0);
        Jump((Vector2)positionStartOfBeat + endOfDash, movementCurve.Evaluate(progression), jumpCurve.Evaluate(progression), 0.5f);

        if (PlayerIsInAggroRange())
        {
            GetTriggered();
        }
    }

    protected override void VulnerableBehaviour()
    {
        // bouge pas et attends un coup
    }

    private void Jump(Vector2 destination, float translationLerp, float jumpLerp, float jumpHeightTweak)
    {
        //La hauteur du saut dépend déjà de la longueur du saut demandé donc jumpHeightTweak est juste un multiplicateur de cette valeur.
        float JumpHeight = Vector2.Distance(positionStartOfBeat, destination) / 2 * jumpHeightTweak;
        parent.position = Vector2.Lerp(positionStartOfBeat, destination, translationLerp) + Vector2.Lerp(Vector2.zero, new Vector2(0, JumpHeight), jumpLerp);
    }
}
