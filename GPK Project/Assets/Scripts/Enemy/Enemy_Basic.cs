using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Basic : EnemyBase
{
    #region Initialization
    [SerializeField] [Range(0, 5)] private float movementDistance = 1f;
    public AnimationCurve movementCurve;
    public int attackDamage;

    private GameObject attackParent;
    public CircleCollider2D attackCollider;
    public SpriteRenderer attackRenderer;
    public Animator animator;

    private float maxRadiusAttack;

    private bool hasAttacked;
    private ContactFilter2D playerFilter = new ContactFilter2D();
    #endregion

    protected override EnemyBehaviour[] PassivePattern => passivePattern;

    private EnemyBehaviour[] passivePattern = new EnemyBehaviour[]
    {
        new EnemyBehaviour(EnemyState.Moving)
    };
    protected override EnemyBehaviour[] TriggeredPattern => triggeredPattern;

    private EnemyBehaviour[] triggeredPattern =  new EnemyBehaviour[]
    {
        new EnemyBehaviour(EnemyState.Triggered, 0.5f),
        new EnemyBehaviour(EnemyState.Action),
        new EnemyBehaviour(EnemyState.Vulnerable, true)
    };

    protected override void Init()
    {
        attackParent = parent.Find("Attack").gameObject;
        //attackCollider = FindComponentInHierarchy<CircleCollider2D>();
        //attackRenderer = FindComponentInHierarchy<SpriteRenderer>("Attack");
        attackParent.SetActive(false);
        maxRadiusAttack = attackParent.transform.localScale.x;
        hasAttacked = false;
    }

    protected override void ConvertedBehaviour()
    {
        // juste un état passif où on s'assure que l'animator est au bon endroit toussa
        // (le fait de convertir se fait pas ici)
        animator.SetBool("Converted", true);
        attackParent.SetActive(false);
    }

    protected override void TriggeredBehaviour()
    {
        canBeDamaged = FalseDuringBeatProgression(0.6f, 0.95f);
        float progression = CurrentBeatProgressionAdjusted(2, 0.5f);
        parent.position = Vector2.Lerp(positionStartOfBeat, playerPositionStartOfBeat, progression);
        //bouge et arrive sur le prochain beat vers le joueur
    }

    protected override void ActionBehaviour()
    {
        attackParent.SetActive(true);
        float attackScale= Mathf.Lerp(maxRadiusAttack, 0, GameManager.Instance.Beat.currentBeatProgression);
        attackParent.transform.localScale = new Vector3(attackScale,attackScale);
        if (GameManager.Instance.Beat.currentBeatProgression > 0.9f)
        {
            attackParent.SetActive(false);
        }

        //zone dangeureuse autour de l'ennemi
        playerFilter.useTriggers = true;
        playerFilter.SetLayerMask(LayerMask.GetMask("Player"));
        List<Collider2D> colliders = new List<Collider2D>();
        if(GameManager.Instance.Beat.currentBeatProgression > 0.1f)
        {
            Physics2D.OverlapCollider(attackCollider, playerFilter, colliders);
        }
        else
        {
            hasAttacked = false;
        }

        if(colliders.Count > 0 && !hasAttacked)
        {
            hasAttacked = true;
            GameManager.Instance.playerManager.TakeDamage(attackDamage);
        }
    }

    protected override void MovingBehaviour()
    {
        canBeDamaged = FalseDuringBeatProgression(0.2f, 0.8f);
        Vector2 endOfDash = Vector2.ClampMagnitude(playerPositionStartOfBeat- positionStartOfBeat, movementDistance);
        parent.position = Vector2.Lerp(positionStartOfBeat, (Vector2)positionStartOfBeat + endOfDash, movementCurve.Evaluate(CurrentBeatProgressionAdjusted(2, 0)));
        if (PlayerIsInAggroRange())
        {
            GetTriggered();
        }
    }

    protected override void VulnerableBehaviour()
    {
        // bouge pas et attends un coup
    }
}
