using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum EnemyState
{
    NULL,
    Action,
    Defense,
    Vulnerable,
    Moving,
    Idle,
    Knockback,
    Triggered,
    Converted
}

public class EnemyBehaviour
{
    #region Personal
    private float necessaryTimeToAction = 1f;
    private bool vulnerable = false;
    #endregion

    public EnemyState State { get; private set; }
    public bool Vulnerable
    {
        get { return vulnerable; }
        private set { vulnerable = value; }
    }
    /// <summary>
    /// Temps en pourcentage nécessaire pour changer de behaviour en milieu de beat.
    /// 0 = le behaviour peut changer jusqu'à une frame avant le prochain beat,
    /// 1 = a besoin de la durée complète du beat pour changer.
    /// (Utile que pour les comportements triggered juste avant un beat.)
    /// </summary>
    public float NecessaryTimeToAction
    {
        get { return necessaryTimeToAction; }
        private set { necessaryTimeToAction = value; }
    }

    public Action behaviour = () => Debug.LogWarning("Je n'ai pas de behaviour.");

    #region Contructors
    public EnemyBehaviour(EnemyState _state)
    {
        State = _state;
    }

    public EnemyBehaviour(EnemyState _state, bool _vulnerable)
    {
        State = _state;
        vulnerable = _vulnerable;
    }

    public EnemyBehaviour(EnemyState _state, float _necessaryTimeToAction)
    {
        State = _state;
        necessaryTimeToAction = _necessaryTimeToAction;
    }

    public EnemyBehaviour(EnemyState _state, bool _vulnerable, float _necessaryTimeToAction)
    {
        State = _state;
        vulnerable = _vulnerable;
        necessaryTimeToAction = _necessaryTimeToAction;
    }
    #endregion

    public void SetBehaviour(Action _behaviour)
    {
        behaviour = _behaviour;
    }
}


/// <summary>
/// EnemyBase contient la base de fonctionnement de tous les ennemis. Faire hériter le nouveau script d'ennemi de ce script.
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    #region Initialization

    # region Stats
    [Header("Stats")]
    [SerializeField] [Range(1, 20)] protected int enemyMaxHP = 1;
    [SerializeField] [Range(0, 20)] protected float aggroRange = 5;
    protected int enemyCurrentHP;
    protected Vector2 knockback;
    protected float startKnockBackTime;
    protected Vector2 knockbackStartPos;
    #endregion

    # region Behaviour
    /// <summary>
    /// Est true quand je joueur est dans le même écran que l'ennemi, false si le joueur est dans un autre écran.
    /// </summary>
    protected bool activated;
    protected bool triggered;
    protected bool converted;
    protected bool canBeDamaged;

    /// <summary>
    /// Sert à séquencer un mouvement s'il a lieu sur plusieurs temps.
    /// </summary>
    protected int sameBehaviourIndex = 0;

    protected EnemyBehaviour currentBehaviour = null;
    protected EnemyBehaviour nextBehaviour = null;

    /// <summary>
    /// Suite des comportements qu'a l'ennemi quand le joueur est hors de sa range d'aggro.
    /// </summary>
	protected abstract EnemyBehaviour[] PassivePattern { get; }

    /// <summary>
    /// Suite des comportements qu'a l'ennemi dès que le joueur est rentré dans sa range d'aggro.
    /// </summary>
    protected abstract EnemyBehaviour[] TriggeredPattern { get; }
    #endregion

    #region Code Related
    protected Transform player;
    protected Vector2 playerPositionStartOfBeat;
    protected Transform parent;
    protected Vector2 positionStartOfBeat;
    protected Animator animator;
    protected AudioSource source;

    protected List<Vector2> lastSeenPlayerPosition = new List<Vector2>();
    protected bool alreadyGotToLastPosition;

    /// <summary>
    /// Suit la progression du tableau de patterns actuel.
    /// </summary>
    protected int currentBehaviourIndex;
    protected int nextBehaviourIndex;

    protected void NullBehaviour()
    {
        //does nothing
    }
    protected EnemyBehaviour nullBehaviour = new EnemyBehaviour(EnemyState.NULL);
    protected EnemyBehaviour convertedBehaviour = new EnemyBehaviour(EnemyState.Converted);
    protected EnemyBehaviour knockbackBehaviour = new EnemyBehaviour(EnemyState.Knockback, false);

    protected Rigidbody2D rb2D;
    #endregion

    #endregion

    #region Méthodes à override
    protected abstract void Init();

    /* Pour se servir des behaviour il faut les override comme ça :
    protected override void ActionBehaviour()
    {
        //Le code du behaviour
    }
     */
    protected virtual void ActionBehaviour()
    {
        Debug.LogWarning("Ce Behaviour n'a pas été override et l'ennemi essaie de l'utiliser.");
    }
    protected virtual void DefenseBehaviour()
    {
        Debug.LogWarning("Ce Behaviour n'a pas été override et l'ennemi essaie de l'utiliser.");
    }
    protected virtual void VulnerableBehaviour()
    {
        Debug.LogWarning("Ce Behaviour n'a pas été override et l'ennemi essaie de l'utiliser.");
    }
    protected virtual void MovingBehaviour()
    {
        Debug.LogWarning("Ce Behaviour n'a pas été override et l'ennemi essaie de l'utiliser.");
    }
    protected virtual void IdleBehaviour()
    {
        Debug.LogWarning("Ce Behaviour n'a pas été override et l'ennemi essaie de l'utiliser.");
    }
    protected virtual void KnockbackBehaviour()
    {
        Debug.LogWarning("Ce Behaviour n'a pas été override et l'ennemi essaie de l'utiliser.");
    }
    protected virtual void TriggeredBehaviour()
    {
        Debug.LogWarning("Ce Behaviour n'a pas été override et l'ennemi essaie de l'utiliser.");
    }
    protected abstract void ConvertedBehaviour();
    protected virtual void OnConverted()
    {
        Debug.LogWarning("OnConverted n'a pas été override et il est appelé.");
    }
    #endregion

    private void Awake()
    {
        parent = transform.parent;
        source = GetComponent<AudioSource>();
        animator = parent.GetComponentInChildren<Animator>();
        rb2D = parent.GetComponent<Rigidbody2D>() != null ? GetComponent<Rigidbody2D>() : GetComponentInChildren<Rigidbody2D>();
        if (rb2D == null)
        {
            Debug.LogError("Le rigidbody n'a pas été trouvé");
        }
        BehavioursSetUp();

        Init();
    }

    private void Start()
    {
        activated = true; ///////////////////
        player = GameManager.Instance.player.transform;

        currentBehaviour = nullBehaviour;
        enemyCurrentHP = enemyMaxHP;
    }

    private void Update()
    {
        if (activated)
        {
            if (currentBehaviour == nullBehaviour)
            {
                nextBehaviour = NextBehaviour();
            }

            if (GameManager.Instance.Beat.onBeatSingleFrame)
            {
                Changebehaviour();
            }
            UpdateLastSeenPosition();

            CurrentBehaviour();
        }
        else
        {
            currentBehaviour = nullBehaviour;
        }
    }

    #region Méthodes uniques au fonctionnement général des ennemis
    private void CurrentBehaviour()
    {
        //Debug.Log(currentBehaviour.State.ToString());
        currentBehaviour.behaviour.Invoke();
    }

    private EnemyBehaviour NextBehaviour()
    {
        if (converted)
        {
            return convertedBehaviour;
        }
        else if (currentBehaviour == nullBehaviour)
        {
            nextBehaviourIndex = 0;
            return PassivePattern[nextBehaviourIndex];
        }
        else
        {
            nextBehaviourIndex = currentBehaviourIndex + 1;
            if (triggered)
            {
                if (nextBehaviourIndex < TriggeredPattern.Length)
                {
                    if (TriggeredPattern[nextBehaviourIndex] == currentBehaviour)
                    {
                        sameBehaviourIndex++;
                    }
                    return TriggeredPattern[nextBehaviourIndex];
                }
                else
                {
                    nextBehaviourIndex = 0;
                    sameBehaviourIndex = 0;
                    triggered = false;
                    return PassivePattern[nextBehaviourIndex];
                }
            }
            else
            {
                if (nextBehaviourIndex < PassivePattern.Length)
                {
                    if (PassivePattern[nextBehaviourIndex] == currentBehaviour)
                    {
                        sameBehaviourIndex++;
                    }
                    return PassivePattern[nextBehaviourIndex];
                }
                else
                {
                    nextBehaviourIndex = 0;
                    sameBehaviourIndex = 0;
                    return PassivePattern[nextBehaviourIndex];
                }
            }
        }

    }

    private void Changebehaviour()
    {
        currentBehaviour = nextBehaviour;
        currentBehaviourIndex = nextBehaviourIndex;
        nextBehaviour = NextBehaviour();
        playerPositionStartOfBeat = GetLastSeenPlayerPosition();
        positionStartOfBeat = parent.transform.position;
    }

    private void BehavioursSetUp()
    {
        AssignBehaviourToEnemyBehaviourClass(PassivePattern);
        AssignBehaviourToEnemyBehaviourClass(TriggeredPattern);

        convertedBehaviour.SetBehaviour(ConvertedBehaviour);
        knockbackBehaviour.SetBehaviour(KnockbackBehaviour);
        nullBehaviour.SetBehaviour(NullBehaviour);
    }

    private void AssignBehaviourToEnemyBehaviourClass(EnemyBehaviour[] pattern)
    {
        foreach (EnemyBehaviour enemyBehaviour in pattern)
        {
            switch (enemyBehaviour.State)
            {
                case EnemyState.Action:
                    enemyBehaviour.SetBehaviour(ActionBehaviour);
                    break;
                case EnemyState.Defense:
                    enemyBehaviour.SetBehaviour(DefenseBehaviour);
                    break;
                case EnemyState.Vulnerable:
                    enemyBehaviour.SetBehaviour(VulnerableBehaviour);
                    break;
                case EnemyState.Moving:
                    enemyBehaviour.SetBehaviour(MovingBehaviour);
                    break;
                case EnemyState.Idle:
                    enemyBehaviour.SetBehaviour(IdleBehaviour);
                    break;
                case EnemyState.Triggered:
                    enemyBehaviour.SetBehaviour(TriggeredBehaviour);
                    break;
                case EnemyState.Converted:
                    enemyBehaviour.SetBehaviour(ConvertedBehaviour);
                    break;
                case EnemyState.Knockback:
                    Debug.LogWarning("Le behaviour knockback ne peut pas être mit dans les patterns");
                    break;
                default:
                    break;
            }
        }
    }

    private bool IHaveTimeToChangeBehaviour(EnemyBehaviour desiredBehaviour)
    {
        if (desiredBehaviour.NecessaryTimeToAction > GameManager.Instance.Beat.currentBeatProgression)
            return true;
        else
            return false;
    }
    #endregion

    #region Méthodes utiles à la création de behaviours
    protected bool PlayerIsInAggroRange()
    {
        if (Vector2.Distance(player.position, parent.position) < aggroRange)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    protected Vector2 GetLastSeenPlayerPosition()
    {
        for (int i = 0; i < lastSeenPlayerPosition.Count; i++)
        {
            if (NoObstacleBetweenMeAndThere(lastSeenPlayerPosition[i]))
            {
                if (Vector2.Distance(lastSeenPlayerPosition[i], parent.position) < 0.5f)
                {
                    alreadyGotToLastPosition = true;
                    break;
                }
                else if (!alreadyGotToLastPosition)
                    return lastSeenPlayerPosition[i];
            }
        }

        float distance = aggroRange > 1f ? aggroRange : 1f;
        return (Vector2)parent.transform.position + new Vector2(UnityEngine.Random.Range(-distance, distance), UnityEngine.Random.Range(-distance, distance));
    }

    protected bool NoObstacleBetweenMeAndThere(Vector2 positionToGetTo)
    {
        RaycastHit2D travelPathHitObject = Physics2D.Raycast
            (
                parent.transform.position,
                positionToGetTo - (Vector2)parent.transform.position,
                Vector2.Distance(positionToGetTo, parent.transform.position),
                LayerMask.GetMask("Obstacle")
            );

        if (travelPathHitObject)
            return false;
        else
            return true;
    }

    protected void GetTriggered()
    {
        triggered = true;
        currentBehaviourIndex = -1;
        nextBehaviour = NextBehaviour();
        if (IHaveTimeToChangeBehaviour(nextBehaviour))
        {
            Changebehaviour();
        }
    }

    public void TakeDamage()
    {
        if (!converted && canBeDamaged)
        {
            enemyCurrentHP--;
            if (enemyCurrentHP <= 0)
            {
                GetConverted(false);
            }
            animator.SetTrigger("Hurt");
        }
    }

    public void TakeDamage(int damageTaken, Vector2 _knockback)
    {
        if (!converted && canBeDamaged)
        {
            enemyCurrentHP -= damageTaken;
            if (enemyCurrentHP <= 0)
            {
                GetConverted(false);
            }

            if(animator != null)
            {
                animator.SetTrigger("Hurt");
            }

            knockback = _knockback;
            Knockback();
        }
    }

    /// <summary>
    /// Permet d'ajuster "currentBeatProgression" pour accélérer, décélérer, et décaler la plage d'action.
    /// </summary>
    /// <param name="multiplier">Multiplie la vitesse de déroulement : 2 accélère par 2 la vitesse de déroulement de l'action, 0,5 ralentit le temps par 2.</param>
    /// <param name="offset">0 le rend actif dès la première frame du beat, 1 le rend actif à la dernière frame.</param>
    /// <returns></returns>
    protected float CurrentBeatProgressionAdjusted(float multiplier, float offset)
    {
        float progression = GameManager.Instance.Beat.currentBeatProgression;

        float progressionExpected = (progression - offset) * multiplier;

        progressionExpected = Mathf.Clamp(progressionExpected, 0, 1);

        return progressionExpected;
    }

    /// <summary>
    /// Returns false between deactivationStart and deactivationEnd. Returns true before and after those times.
    /// </summary>
    /// <param name="deactivationStart">0 returns false from the first frame, 1 returns true until the last frame.</param>
    /// <param name="deactivationEnd">0 returns true from the first frame, 1 returns false until the last frame.</param>
    /// <returns></returns>
    protected bool FalseDuringBeatProgression(float deactivationStart, float deactivationEnd)
    {
        if (deactivationEnd <= deactivationStart)
        {
            Debug.LogWarning("Cette fonction n'est pas utilisée correctement.");
        }
        float beatProgression = GameManager.Instance.Beat.currentBeatProgression;
        if (beatProgression < deactivationStart)
        {
            return true;
        }
        else if (beatProgression > deactivationEnd)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// I get converted.
    /// </summary>
    public void GetConverted(bool initialize)
    {
        currentBehaviour = convertedBehaviour;
        converted = true;
        if(animator != null)
        {
            animator.SetTrigger("Conversion");
        }

        if(!initialize)
        {
            OnConverted();
        }
    }

    public void Knockback()
    {
        knockbackStartPos = parent.position;
        startKnockBackTime = Time.fixedTime;
        currentBehaviour = knockbackBehaviour;
        nextBehaviour = knockbackBehaviour;
        triggered = false;
        currentBehaviourIndex = 0;
    }

    //D'autres méthodes utiles pour autre chose que les behaviours :
    private void UpdateLastSeenPosition()
    {
        if (NoObstacleBetweenMeAndThere(player.position))
        {
            if (lastSeenPlayerPosition.Contains(player.position))
            {
                lastSeenPlayerPosition.Remove(player.position);
            }
            lastSeenPlayerPosition.Insert(0, player.position);

            alreadyGotToLastPosition = false;
        }
    }
    #endregion


    public bool IsConverted()
    {
        return converted;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
}
