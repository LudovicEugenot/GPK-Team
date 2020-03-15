using System.Collections;
using UnityEngine;

public class Enemy_Basic : EnemyBase
{
    #region Initialization
    [SerializeField] [Range(1, 5)] private float movementDistance = 2f;
    public AnimationCurve movementCurve;

    [Tooltip("Quand l'ennemi est triggered, le temps minimum pour qu'il effectue son behaviour \"Triggered\" avant la fin du beat.")]
    [SerializeField] [Range(0, 1)] private float necessaryTimeToTriggerOnNextBeat = 0.5f;

    private CircleCollider2D attackCollider;
    //private SpriteRenderer attackRenderer;

    private float maxRadiusAttack;
    #endregion

    protected override EnemyBehaviour[] passivePattern => new EnemyBehaviour[]
    {
        new EnemyBehaviour(EnemyState.Moving)
    };
    protected override EnemyBehaviour[] triggeredPattern => new EnemyBehaviour[]
    {
        new EnemyBehaviour(EnemyState.Triggered, necessaryTimeToTriggerOnNextBeat),
        new EnemyBehaviour(EnemyState.Action),
        new EnemyBehaviour(EnemyState.Vulnerable, true)
    };

    protected override void Init()
    {
        attackCollider = GetComponent<CircleCollider2D>();
        attackCollider.enabled = false;
    }

    protected override void ConvertedBehaviour()
    {
        // juste un état passif où on s'assure que l'animator est au bon endroit toussa
        // (le fait de convertir se fait pas ici)
    }

    protected override void TriggeredBehaviour()
    {
        float progression = GameManager.Instance.Beat.currentBeatProgression > 0.5f ? (GameManager.Instance.Beat.currentBeatProgression - 0.5f) * 2 : 0;
        Vector2.Lerp(transformStartOfBeat.position, playerTransformStartOfBeat.position, progression);
        //bouge et arrive sur le prochain beat vers le joueur
    }

    protected override void ActionBehaviour()
    {
        attackCollider.enabled = true;
        attackCollider.radius = Mathf.Lerp(maxRadiusAttack, 0, GameManager.Instance.Beat.currentBeatProgression);
        if (GameManager.Instance.Beat.currentBeatProgression > 0.9)
        {
            attackCollider.enabled = false;
        }
        //zone dangeureuse autour de l'ennemi
    }

    protected override void MovingBehaviour()
    {
        Vector2 endOfDash = Vector2.ClampMagnitude(playerTransformStartOfBeat.position - transformStartOfBeat.position, movementDistance);
        transform.position = Vector2.Lerp(player.position, endOfDash, movementCurve.Evaluate(GameManager.Instance.Beat.currentBeatProgression * 2));

        if (PlayerIsInAggroRange())
        {
            GetTriggered();
        }
    }

    protected override void VulnerableBehaviour()
    {
        canBeDamaged = true;
        if (GameManager.Instance.Beat.currentBeatProgression > 0.9)
        {
            canBeDamaged = false;
        }
        // bouge pas et attends un coup
    }
}
