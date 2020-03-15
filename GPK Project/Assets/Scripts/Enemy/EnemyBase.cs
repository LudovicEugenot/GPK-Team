using System.Collections;
using UnityEngine;

public enum EnemyState
{
    NULL,
    Action,
    Defense,
    Vulnerable,
    Moving,
    Idle,
    Triggered,
    Converted
}


public class EnemyBehaviour
{
    public EnemyState state { get; private set; }
    public bool vulnerable
    {
        get { return false; }
        private set { vulnerable = value; }//////////////Stack Overflow, il set necessary vulnerable et en essayant de le set il le reset etc etc...
    }
    /// <summary>
    /// Temps en pourcentage nécessaire pour changer de behaviour en milieu de beat.
    /// 0 = le behaviour peut changer jusqu'à une frame avant le prochain beat,
    /// 1 = a besoin de la durée complète du beat pour changer.
    /// (Utile que pour les comportements triggered juste avant un beat.)
    /// </summary>
    public float necessaryTimeToAction
    {
        get { return 1f; }
        private set { necessaryTimeToAction = value; } //////////////Stack Overflow, il set necessary time to action et en essayant de le set il le reset etc etc...
    }

    public System.Action behaviour;

    #region Contructors
    public EnemyBehaviour(EnemyState _state)
    {
        state = _state;
    }

    public EnemyBehaviour(EnemyState _state, bool _vulnerable)
    {
        state = _state;
        vulnerable = _vulnerable;
    }

    public EnemyBehaviour(EnemyState _state, float _necessaryTimeToAction)
    {
        state = _state;
        necessaryTimeToAction = _necessaryTimeToAction;
    }

    public EnemyBehaviour(EnemyState _state, bool _vulnerable, float _necessaryTimeToAction)
    {
        state = _state;
        vulnerable = _vulnerable;
        necessaryTimeToAction = _necessaryTimeToAction;
    }
    #endregion
}


/// <summary>
/// EnemyBase contient la base de fonctionnement de tous les ennemis. Faire hériter le nouveau script d'ennemi de ce script.
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    #region Initialization

    # region Stats
    [Header("Stats")]
    [SerializeField] [Range(1, 5)] protected int enemyMaxHP = 1;
    [SerializeField] [Range(0, 20)] protected float aggroRange = 5;
    protected int enemyCurrentHP;
    #endregion

    # region Behaviour
    /// <summary>
    /// Est true quand je joueur est dans le même écran que l'ennemi, false si le joueur est dans un autre écran.
    /// </summary>
    protected bool activated;
    protected bool triggered;
    protected bool converted;
    protected bool canBeDamaged; /////////////////////////////////////////////////////
    protected int currentBehaviourIndex;
    protected int nextBehaviourIndex;
    protected EnemyBehaviour currentBehaviour = null;
    protected EnemyBehaviour nextBehaviour = null;

    /// <summary>
    /// Suite des comportements qu'a l'ennemi quand le joueur est hors de sa range d'aggro.
    /// </summary>
    protected abstract EnemyBehaviour[] passivePattern { get; }

    /// <summary>
    /// Suite des comportements qu'a l'ennemi dès que le joueur est rentré dans sa range d'aggro.
    /// </summary>
    protected abstract EnemyBehaviour[] triggeredPattern { get; }
    #endregion

    #region Code Related
    protected Transform player;
    protected Transform playerTransformStartOfBeat;
    protected Transform transformStartOfBeat;

    protected EnemyBehaviour nullBehaviour = new EnemyBehaviour(EnemyState.NULL);
    protected EnemyBehaviour convertedBehaviour = new EnemyBehaviour(EnemyState.Converted);

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
    protected virtual void TriggeredBehaviour()
    {
        Debug.LogWarning("Ce Behaviour n'a pas été override et l'ennemi essaie de l'utiliser.");
    }
    protected abstract void ConvertedBehaviour();
    #endregion

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>() != null ? GetComponent<Rigidbody2D>() : GetComponentInChildren<Rigidbody2D>();
        if (rb2D == null)
        {
            Debug.LogError("le rigidbody n'a pas été trouvé");
        }

        BehavioursSetUp();

        Init();
    }

    private void Start()
    {
        activated = false;
        triggered = false;
        converted = false;
        player = GameManager.Instance.player.transform;

        enemyCurrentHP = enemyMaxHP;
    }

    private void Update()
    {
        if (activated)
        {
            if(currentBehaviour == nullBehaviour)
            {
                NextBehaviour();
            }

            if (GameManager.Instance.Beat.onBeatSingleFrame)
            {
                Changebehaviour();
            }

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
            return passivePattern[nextBehaviourIndex];
        }
        else
        {
            nextBehaviourIndex = currentBehaviourIndex + 1;
            if (triggered)
            {
                if (nextBehaviourIndex <= triggeredPattern.Length)
                {
                    return triggeredPattern[nextBehaviourIndex];
                }
                else
                {
                    nextBehaviourIndex = 0;
                    triggered = false;
                    return passivePattern[nextBehaviourIndex];
                }
            }
            else
            {
                if (nextBehaviourIndex <= passivePattern.Length)
                {
                    return passivePattern[nextBehaviourIndex];
                }
                else
                {
                    nextBehaviourIndex = 0;
                    return passivePattern[nextBehaviourIndex];
                }
            }
        }

    }

    private void Changebehaviour()
    {
        currentBehaviour = nextBehaviour;
        currentBehaviourIndex = nextBehaviourIndex;
        nextBehaviour = NextBehaviour();
        playerTransformStartOfBeat = player;
        transformStartOfBeat = transform;
    }

    private void BehavioursSetUp()
    {
        AssignBehaviourToEnemyBehaviourClass(passivePattern);
        AssignBehaviourToEnemyBehaviourClass(triggeredPattern);

        convertedBehaviour.behaviour = ConvertedBehaviour;
    }

    private void AssignBehaviourToEnemyBehaviourClass(EnemyBehaviour[] pattern)
    {
        foreach (EnemyBehaviour enemyBehaviour in pattern)
        {
            switch (enemyBehaviour.state)
            {
                case EnemyState.Action:
                    enemyBehaviour.behaviour = ActionBehaviour;
                    break;
                case EnemyState.Defense:
                    enemyBehaviour.behaviour = DefenseBehaviour;
                    break;
                case EnemyState.Vulnerable:
                    enemyBehaviour.behaviour = VulnerableBehaviour;
                    break;
                case EnemyState.Moving:
                    enemyBehaviour.behaviour = MovingBehaviour;
                    break;
                case EnemyState.Idle:
                    enemyBehaviour.behaviour = IdleBehaviour;
                    break;
                case EnemyState.Triggered:
                    enemyBehaviour.behaviour = TriggeredBehaviour;
                    break;
                case EnemyState.Converted:
                    enemyBehaviour.behaviour = ConvertedBehaviour;
                    break;
                default:
                    break;
            }
        }
    }

    private bool IHaveTimeToChangeBehaviour(EnemyBehaviour desiredBehaviour)
    {
        if (desiredBehaviour.necessaryTimeToAction < GameManager.Instance.Beat.currentBeatProgression)
            return true;
        else
            return false;
    }
    #endregion

    #region Méthodes utiles à la création de behaviours
    protected bool PlayerIsInAggroRange()
    {
        if (Vector2.Distance(player.position, transform.position) < aggroRange)
        {
            nextBehaviour = NextBehaviour();
            return true;
        }
        else
        {
            return false;
        }
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

    protected void DealDamage()
    {
        // Player.life --;
    }

    protected void TakeDamage()
    {
        if (enemyCurrentHP > 1)
        {
            enemyCurrentHP--;
        }
        else
        {
            Convert();
        }
    }

    /// <summary>
    /// I get converted.
    /// </summary>
    protected void Convert()
    {
        currentBehaviour = convertedBehaviour;
        converted = true;
        // Convertir l'ennemi
        // Ennemi devient un hook (active un bool dans un autre script "ennemi hook")
        Debug.LogWarning("<color=green> I AM CONVERTED OH NO.");
    }
    #endregion
}
