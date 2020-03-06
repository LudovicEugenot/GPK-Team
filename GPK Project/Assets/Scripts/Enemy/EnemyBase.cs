using System.Collections;
using UnityEngine;

public enum EnemyState
{
    Action,
    Defense,
    Vulnerable,
    Moving,
    NULL
}

/// <summary>
/// EnemyBase contient la base de fonctionnement de tous les ennemis. Faire hériter le nouveau script d'ennemi de ce script.
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    #region Initialization

    # region Stats
    protected abstract int enemyMaxHP { get; }
    protected int enemyCurrentHP;
    protected abstract float aggroRange { get; }
    #endregion

    # region Behaviour
    /// <summary>
    /// Est true quand je joueur est dans le même écran que l'ennemi, false si le joueur est dans un écran voisin.
    /// </summary>
    protected bool activated;
    protected bool triggered;
    protected int currentStateIndex;
    protected EnemyState currentState;
    protected EnemyState nextState;

    /// <summary>
    /// Suite des comportements qu'a l'ennemi quand le joueur est hors de sa range d'aggro.
    /// </summary>
    protected abstract EnemyState[] passivePattern { get; set; }

    /// <summary>
    /// Suite des comportements qu'a l'ennemi dès que le joueur est rentré dans sa range d'aggro.
    /// </summary>
    protected abstract EnemyState[] triggeredPattern { get; set; }
    #endregion

    #endregion


    protected abstract void Init();
    protected abstract void ActionBehaviour();
    protected abstract void DefenseBehaviour();
    protected abstract void VulnerableBehaviour();
    protected abstract void MovingBehaviour();

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        activated = false;
        triggered = false;
    }

    private void Update()
    {
        NextBehaviour();
    }

    private EnemyState NextBehaviour()
    {
        if (activated)
        {
            currentStateIndex++;
            if (triggered)
            {
                if (currentStateIndex <= triggeredPattern.Length)
                {
                    return triggeredPattern[currentStateIndex];
                }
                else
                {
                    currentStateIndex = 0;
                    if (PlayerIsClose())
                    {
                        return triggeredPattern[currentStateIndex];
                    }
                    else
                    {
                        return passivePattern[currentStateIndex];
                    }
                }
            }
            else
            {
                if (currentStateIndex <= passivePattern.Length)
                {
                    return passivePattern[currentStateIndex];
                }
                else
                {
                    currentStateIndex = 0;
                    return passivePattern[currentStateIndex];
                }
            }
        }
        else
        {
            return EnemyState.NULL;
        }

    }

    protected bool PlayerIsClose()
    {
        return true;
    }

    protected void TakeDamage()
    {
        if (enemyCurrentHP > 1)
        {
            enemyCurrentHP--;
        }
        else
        {
            Die();
        }
    }

    protected void Die()
    {
        Destroy(gameObject);
    }
}
