using System.Collections;
using UnityEngine;

// faut créer une classe state pour pouvoir accéder aux infos de tel état de n'importe où 
// (ex : savoir pendant l'état précédent si on sera vulnérable dans l'état suivant...)

public enum EnemyState
{
    Action,
    Defense,
    Vulnerable,
    Moving,
    Idle,
    Triggered,
    Converted,
    NULL
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
    /// Est true quand je joueur est dans le même écran que l'ennemi, false si le joueur est dans un écran voisin.
    /// </summary>
    protected bool activated;
    protected bool triggered;
    protected bool converted;
    protected bool canBeDamaged; /// <summary>
    /// ////////////////////////////////////////////////
    /// </summary>
    protected int currentStateIndex;
    protected EnemyState currentState;
    protected EnemyState nextState;

    /// <summary>
    /// Suite des comportements qu'a l'ennemi quand le joueur est hors de sa range d'aggro.
    /// </summary>
    protected abstract EnemyState[] passivePattern { get; }

    /// <summary>
    /// Suite des comportements qu'a l'ennemi dès que le joueur est rentré dans sa range d'aggro.
    /// </summary>
    protected abstract EnemyState[] triggeredPattern { get; }
    #endregion

    #region Code Related
    protected Transform player; // à transformer en script plus précis si nécessaire
    #endregion

    #endregion


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

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        activated = false;
        triggered = false;
        converted = false;
        currentState = EnemyState.NULL;
        player = GameManager.Instance.player.transform;

        enemyCurrentHP = enemyMaxHP;
    }

    private void Update()
    {
        if (activated)
        {
            if (currentState != EnemyState.Converted)
            {
                if (GameManager.Instance.Beat.onBeatSingleFrame)
                {
                    currentState = nextState;
                    nextState = NextBehaviour();
                }

                CurrentBehaviour();
            }
            else
            {
                ConvertedBehaviour();
            }
        }
        else
        {
            currentState = EnemyState.NULL;
        }
    }

    private void CurrentBehaviour()
    {
        switch (currentState)
        {
            case EnemyState.Action:
                ActionBehaviour();
                break;
            case EnemyState.Defense:
                DefenseBehaviour();
                break;
            case EnemyState.Vulnerable:
                VulnerableBehaviour();
                break;
            case EnemyState.Moving:
                MovingBehaviour();
                break;
            case EnemyState.Idle:
                IdleBehaviour();
                break;
            case EnemyState.Triggered:
                TriggeredBehaviour();
                break;
            case EnemyState.NULL:
                Debug.LogWarning("Enemy " + name + " deactivated.");
                break;
            default:
                break;
        }
    }

    private EnemyState NextBehaviour()
    {
        int nextStateIndex = currentStateIndex + 1;
        if (triggered)
        {
            if (nextStateIndex <= triggeredPattern.Length)
            {
                return triggeredPattern[nextStateIndex];
            }
            else
            {
                nextStateIndex = 0;
                triggered = false;
                return passivePattern[nextStateIndex];
            }
        }
        else
        {
            if (nextStateIndex <= passivePattern.Length)
            {
                return passivePattern[nextStateIndex];
            }
            else
            {
                nextStateIndex = 0;
                return passivePattern[nextStateIndex];
            }
        }

    }

    protected bool PlayerIsInAggroRange()
    {
        if (Vector2.Distance(player.position, transform.position) < aggroRange)
        {
            nextState = NextBehaviour();
            return true;
        }
        else
        {
            return false;
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
        currentState = EnemyState.Converted;
        converted = true;
        // Convertir l'ennemi
        // Ennemi devient un hook (active un bool dans un autre script "ennemi hook")
        Debug.LogWarning("<color=green> I AM CONVERTED OH NO.");
    }
}
