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
        triggered = false;
        converted = false;
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
            if (NoObjectBetweenMeAndThere(lastSeenPlayerPosition[i]))
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

    /// <summary>
    /// Sends a raycast to positionToGetTo according to objectLayers. If objectLayers is empty, checks "Obstacle" per default.
    /// </summary>
    /// <param name="positionToGetTo">Absolute position to get to.</param>
    /// <param name="objectLayers">Name of layers to check. If empty, fills in "Obstacle" per default.</param>
    /// <returns></returns>
    protected bool NoObjectBetweenMeAndThere(Vector2 positionToGetTo, params string[] objectLayers)
    {
        if (objectLayers.Length == 0)
        {
            objectLayers = new string[1];
            objectLayers[0] = "Obstacle";
        }
        RaycastHit2D travelPathHitObject = Physics2D.Raycast
            (
                parent.transform.position,
                positionToGetTo - (Vector2)parent.transform.position,
                Vector2.Distance(positionToGetTo, parent.transform.position),
                LayerMask.GetMask(objectLayers)
            );

        if (travelPathHitObject)
            return false;
        else
            return true;
    }

    #region Init de PositionDependingOnObjectsOnTheWay
    float raycastGap = 0.3f;
    float raycastsPercentage = 0.5f; //Pourcentage de raycasts nécessaires pour valider une path (en pourcentage)

    int numberOfSideRays;
    int numberOfMiddleRays;

    int numberOfRaycastLeftSideHit;
    float averageLeftDistance;

    int numberOfRaycastMiddleHit;
    float averageMiddleDistance;

    int numberOfRaycastRightSideHit;
    float averageRightDistance;

    int totalNumberOfRays;
    int totalNumberOfRaysHit;

    float[] allRays;

    AdditionalDirections mainDirection;
    List<AdditionalDirections> additionalDirectionsList = new List<AdditionalDirections> { AdditionalDirections.behind };
    AdditionalDirections[] additionalDirections;
    float movementReduced;
    #endregion

    /// <summary>
    /// Renvoie une position selon les objets détectés sur le chemin. Le chemin en question est divisé en 3 paths : gauche, milieu et droite.
    /// </summary>
    /// <param name="positionToGetTo">Position vers laquelle cet ennemi essaie d'aller.</param>
    /// <param name="includingEnemies">Si false, détecte les murs. Si true, détecte aussi les ennemis.</param>
    /// <param name="resultingMovementRandomness">0 = va précisément aux positions de PositionAccordingToAdditionalDirection. 1 = a de la liberté de mouvement autour de la position visée.</param>
    /// <param name="pathWidth">Largeur du chemin à "scanner" (un ennemi basique fait 0.4 de large).</param>
    /// <param name="movementDistance">Insérer ici la variable de vitesse de déplacement propre à l'ennemi.</param>
    /// <returns></returns>
    protected Vector2 PositionDependingOnObjectsOnTheWay(Vector2 positionToGetTo, bool includingEnemies, float resultingMovementRandomness, float pathWidth, float movementDistance)
    {
        #region Init
        Vector2 myPosition = parent.transform.position;
        float maxDistance = Vector2.Distance(positionToGetTo, myPosition);
        //Stats
        float longDistance = maxDistance * 0.8f;
        float middleDistance = maxDistance * 0.5f;
        float shortDistance = maxDistance * 0.3f;

        //Set les Variables
        numberOfRaycastLeftSideHit = 0;
        averageLeftDistance = 100f;

        numberOfRaycastMiddleHit = 0;
        averageMiddleDistance = 100f;

        numberOfRaycastRightSideHit = 0;
        averageRightDistance = 100f;

        additionalDirectionsList.Clear();
        movementReduced = 1f;
        #endregion

        #region Set up des raycasts à envoyer
        //la voie est checkée par différents raycasts envoyés tous les 0.3 de distance + les bords de la voie
        if (pathWidth < raycastGap)
        {
            allRays = new float[] { -pathWidth * 0.5f, 0f, pathWidth * 0.5f };
        }
        else
        {
            int numberOfRays = Mathf.FloorToInt(pathWidth / raycastGap) + 2;

            allRays = new float[numberOfRays];

            allRays[0] = -pathWidth * 0.5f;
            allRays[numberOfRays - 1] = pathWidth * 0.5f;

            float centerOffset = allRays.Length % 2 == 0 ? raycastGap * 0.5f : 0f;
            float firstRay = centerOffset - Mathf.FloorToInt((numberOfRays - 2) * 0.5f) * raycastGap;

            for (int i = 1; i < numberOfRays - 1; i++)
            {
                allRays[i] = firstRay;
                firstRay += raycastGap;
            }
        }

        if (allRays.Length % 2 == 0)
        {
            numberOfMiddleRays = 2;
            numberOfSideRays = (int)(allRays.Length * 0.5f) - 1;
        }
        else
        {
            numberOfMiddleRays = 1;
            numberOfSideRays = Mathf.FloorToInt(allRays.Length * 0.5f);
        }
        totalNumberOfRays = numberOfSideRays * 2 + numberOfMiddleRays;
        #endregion

        #region Calcul de l'angle pour l'offset des raycasts à venir
        Vector2 baseRaycast = positionToGetTo - myPosition;

        float calculationAdjustment = 0f;
        if (baseRaycast.x < 0 || baseRaycast.y < 0)
        {
            if (baseRaycast.x > 0)
                calculationAdjustment = Mathf.PI * 2;
            else
                calculationAdjustment = Mathf.PI;
        }
        float angle = Mathf.Atan(baseRaycast.y / baseRaycast.x) + calculationAdjustment - Mathf.PI * 0.5f;
        #endregion

        #region Envois de raycasts
        for (int i = 0; i < allRays.Length; i++)
        {
            RaycastHit2D raycast;
            Vector2 offset = new Vector2(allRays[i] * Mathf.Cos(angle), allRays[i] * Mathf.Sin(angle));
            if (includingEnemies)
            {
                raycast = Physics2D.Raycast
                    (
                        myPosition + offset,
                        baseRaycast,
                        baseRaycast.magnitude,
                        LayerMask.GetMask("Obstacle", "Enemy")
                    );
            }
            else
            {
                raycast = Physics2D.Raycast
                    (
                        myPosition + offset,
                        baseRaycast,
                        baseRaycast.magnitude,
                        LayerMask.GetMask("Obstacle")
                    );
            }

            if (raycast)
            {
                float distance = raycast.distance;
                if (i <= numberOfSideRays - 1)
                {
                    numberOfRaycastLeftSideHit++;
                    averageLeftDistance = averageLeftDistance * (numberOfRaycastLeftSideHit - 1) + distance / numberOfRaycastLeftSideHit;
                }
                else if (i > numberOfSideRays + numberOfMiddleRays - 1)
                {
                    numberOfRaycastRightSideHit++;
                    averageRightDistance = averageRightDistance * (numberOfRaycastRightSideHit - 1) + distance / numberOfRaycastRightSideHit;
                }
                else
                {
                    numberOfRaycastMiddleHit++;
                    averageMiddleDistance = numberOfRaycastMiddleHit == 1 ? distance : (distance + averageMiddleDistance) * 0.5f;
                }
            }
        }
        totalNumberOfRaysHit = numberOfRaycastLeftSideHit + numberOfRaycastRightSideHit + numberOfRaycastMiddleHit;
        if (numberOfRaycastLeftSideHit < numberOfSideRays * raycastsPercentage)
        {
            averageLeftDistance = 100f;
        }

        if (numberOfRaycastRightSideHit < numberOfSideRays * raycastsPercentage)
        {
            averageRightDistance = 100f;
        }
        #endregion

        #region Position obtenue selon les raycasts envoyés
        if (totalNumberOfRaysHit == 0)
        {
            return positionToGetTo;
        }

        if (averageMiddleDistance < longDistance)
        {
            if (averageMiddleDistance < middleDistance)
            {
                if (averageMiddleDistance < shortDistance)
                {
                    if (averageLeftDistance < shortDistance)
                    {
                        if (averageRightDistance < shortDistance)
                        {
                            mainDirection = AdditionalDirections.behind;
                        }
                        else
                        {
                            movementReduced = 0.4f;
                            mainDirection = AdditionalDirections.shortRight;
                            additionalDirectionsList.Add(AdditionalDirections.behind);
                        }
                    }
                    else
                    {
                        if (averageRightDistance < shortDistance)
                        {
                            movementReduced = 0.4f;
                            mainDirection = AdditionalDirections.shortLeft;
                            additionalDirectionsList.Add(AdditionalDirections.behind);
                        }
                        else
                        {
                            mainDirection = AdditionalDirections.shortMiddle;
                            additionalDirectionsList.AddRange(new List<AdditionalDirections>() {
                                AdditionalDirections.shortLeft,
                                AdditionalDirections.shortRight,
                                AdditionalDirections.behind });
                        }
                    }
                }
                else
                {
                    if (averageLeftDistance > shortDistance)
                    {
                        if (averageRightDistance > shortDistance)
                        {
                            movementReduced = 0.7f;
                            mainDirection = AdditionalDirections.middleMiddle;
                            additionalDirectionsList.AddRange(new List<AdditionalDirections>() {
                                AdditionalDirections.middleRight,
                                AdditionalDirections.middleLeft,
                                AdditionalDirections.shortLeft,
                                AdditionalDirections.shortRight,
                                AdditionalDirections.shortMiddle,
                                AdditionalDirections.behind });
                        }
                        else
                        {
                            movementReduced = 0.5f;
                            mainDirection = AdditionalDirections.middleLeft;
                            additionalDirectionsList.AddRange(new List<AdditionalDirections>() {
                                AdditionalDirections.shortLeft,
                                AdditionalDirections.shortMiddle,
                                AdditionalDirections.behind });
                        }
                    }
                    else
                    {
                        if (averageRightDistance > shortDistance)
                        {
                            movementReduced = 0.5f;
                            mainDirection = AdditionalDirections.middleRight;
                            additionalDirectionsList.AddRange(new List<AdditionalDirections>() {
                                AdditionalDirections.shortRight,
                                AdditionalDirections.shortMiddle,
                                AdditionalDirections.behind });
                        }
                        else
                        {
                            mainDirection = AdditionalDirections.behind;
                            additionalDirectionsList.Add(AdditionalDirections.shortMiddle);
                        }
                    }
                }
            }
            else if (averageLeftDistance < middleDistance)
            {
                if (averageRightDistance < middleDistance)
                {
                    movementReduced = 0.6f;
                    mainDirection = AdditionalDirections.shortMiddle;
                    additionalDirectionsList.AddRange(new List<AdditionalDirections>() {
                        AdditionalDirections.shortRight,
                        AdditionalDirections.shortLeft,
                        AdditionalDirections.behind });
                }
                else if (averageRightDistance > longDistance)
                {
                    movementReduced = 0.7f;
                    mainDirection = AdditionalDirections.longRight;
                    additionalDirectionsList.AddRange(new List<AdditionalDirections>() {
                        AdditionalDirections.middleRight,
                        AdditionalDirections.shortMiddle,
                        AdditionalDirections.behind });
                }
                else
                {
                    movementReduced = 0.5f;
                    mainDirection = AdditionalDirections.middleRight;
                    additionalDirectionsList.AddRange(new List<AdditionalDirections>() {
                        AdditionalDirections.shortRight,
                        AdditionalDirections.shortMiddle,
                        AdditionalDirections.behind });
                }
            }
            else
            {
                if (averageRightDistance < middleDistance)
                {
                    movementReduced = 0.5f;
                    mainDirection = AdditionalDirections.middleLeft;
                    additionalDirectionsList.AddRange(new List<AdditionalDirections>() {
                        AdditionalDirections.middleRight,
                        AdditionalDirections.shortMiddle,
                        AdditionalDirections.behind });
                }
                else if (averageRightDistance > longDistance)
                {
                    movementReduced = 0.7f;
                    mainDirection = AdditionalDirections.longRight;
                    additionalDirectionsList.AddRange(new List<AdditionalDirections>() {
                        AdditionalDirections.longLeft,
                        AdditionalDirections.middleLeft,
                        AdditionalDirections.middleMiddle,
                        AdditionalDirections.middleRight,
                        AdditionalDirections.behind });
                }
                else
                {
                    movementReduced = 0.5f;
                    mainDirection = AdditionalDirections.middleLeft;
                    additionalDirectionsList.AddRange(new List<AdditionalDirections>() {
                        AdditionalDirections.middleRight,
                        AdditionalDirections.middleMiddle,
                        AdditionalDirections.behind });
                }
            }
        }
        else
        {
            if (averageLeftDistance > longDistance)
            {
                if (averageRightDistance > longDistance)
                {
                    movementReduced = 0.8f;
                    mainDirection = AdditionalDirections.longMiddle;
                    additionalDirectionsList.AddRange(new List<AdditionalDirections>() {
                        AdditionalDirections.longLeft,
                        AdditionalDirections.longRight,
                        AdditionalDirections.longMiddle,
                        AdditionalDirections.longLeft,
                        AdditionalDirections.longRight,
                        AdditionalDirections.behind });
                }
                else if (averageRightDistance > shortDistance)
                {
                    movementReduced = 0.7f;
                    mainDirection = AdditionalDirections.longLeft;
                    additionalDirectionsList.AddRange(new List<AdditionalDirections>() {
                        AdditionalDirections.longMiddle,
                        AdditionalDirections.middleLeft,
                        AdditionalDirections.middleMiddle,
                        AdditionalDirections.middleRight,
                        AdditionalDirections.behind });
                }
                else
                {
                    movementReduced = 0.5f;
                    mainDirection = AdditionalDirections.middleLeft;
                    additionalDirectionsList.AddRange(new List<AdditionalDirections>() {
                        AdditionalDirections.middleLeft,
                        AdditionalDirections.middleMiddle,
                        AdditionalDirections.behind });
                }
            }
            else
            {
                if (averageLeftDistance > shortDistance)
                {
                    if (averageRightDistance > longDistance)
                    {
                        movementReduced = 0.7f;
                        mainDirection = AdditionalDirections.longRight;
                        additionalDirectionsList.AddRange(new List<AdditionalDirections>() {
                            AdditionalDirections.longMiddle,
                            AdditionalDirections.middleLeft,
                            AdditionalDirections.middleMiddle,
                            AdditionalDirections.middleRight,
                            AdditionalDirections.behind });
                    }
                    else if (averageRightDistance > shortDistance)
                    {
                        movementReduced = 0.5f;
                        mainDirection = AdditionalDirections.middleMiddle;
                        additionalDirectionsList.AddRange(new List<AdditionalDirections>() {
                            AdditionalDirections.middleRight,
                            AdditionalDirections.middleLeft,
                            AdditionalDirections.behind });
                    }
                    else
                    {
                        movementReduced = 0.5f;
                        mainDirection = AdditionalDirections.middleLeft;
                        additionalDirectionsList.AddRange(new List<AdditionalDirections>() {
                            AdditionalDirections.middleRight,
                            AdditionalDirections.middleLeft,
                            AdditionalDirections.behind });
                    }
                }
                else
                {
                    if (averageRightDistance > middleDistance)
                    {
                        movementReduced = 0.5f;
                        mainDirection = AdditionalDirections.middleRight;
                        additionalDirectionsList.AddRange(new List<AdditionalDirections>() {
                            AdditionalDirections.middleMiddle,
                            AdditionalDirections.shortRight,
                            AdditionalDirections.behind });
                    }
                    else if (averageRightDistance > shortDistance)
                    {
                        movementReduced = 0.5f;
                        mainDirection = AdditionalDirections.shortRight;
                        additionalDirectionsList.AddRange(new List<AdditionalDirections>() {
                            AdditionalDirections.shortMiddle,
                            AdditionalDirections.behind });
                    }
                    else
                    {
                        mainDirection = AdditionalDirections.behind;
                    }
                }
            }
        }

        additionalDirections = new AdditionalDirections[additionalDirectionsList.Count];
        for (int i = 0; i < additionalDirectionsList.Count; i++)
        {
            additionalDirections[i] = additionalDirectionsList[i];
        }

        Vector2 direction = PositionAccordingToAdditionalDirection(mainDirection, positionToGetTo, pathWidth) + new Vector2(
            UnityEngine.Random.Range(-movementDistance * movementReduced * resultingMovementRandomness, movementDistance * movementReduced * resultingMovementRandomness),
            UnityEngine.Random.Range(-movementDistance * movementReduced * resultingMovementRandomness, movementDistance * movementReduced * resultingMovementRandomness));
        direction = myPosition + Vector2.ClampMagnitude(direction - myPosition, movementDistance);

        direction = WhileObjectBetweenMeAndThere(myPosition, direction, resultingMovementRandomness, movementDistance, pathWidth, additionalDirections);

        return direction;
        #endregion
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

            if (animator != null)
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

        if (!initialize)
        {
            OnConverted();
            // Convertir l'ennemi
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
        if (NoObjectBetweenMeAndThere(player.position))
        {
            if (lastSeenPlayerPosition.Contains(player.position))
            {
                lastSeenPlayerPosition.Remove(player.position);
            }
            lastSeenPlayerPosition.Insert(0, player.position);

            alreadyGotToLastPosition = false;
        }
    }

    //Méthodes pour alléger PositionDependingOnObjectsOnTheWay (les additionalDirections sont toujours censées terminer avec behind)
    private Vector2 WhileObjectBetweenMeAndThere(Vector2 myPosition, Vector2 destination, float movementRandomness, float movementDistance, float pathWidth, params AdditionalDirections[] additionalDirections)
    {
        Vector2 firstDirection = destination;
        while (!NoObjectBetweenMeAndThere(destination))
        {
            if (
                !NoObjectBetweenMeAndThere(myPosition + Vector2.down) &&
                !NoObjectBetweenMeAndThere(myPosition + Vector2.left) &&
                !NoObjectBetweenMeAndThere(myPosition + Vector2.up) &&
                !NoObjectBetweenMeAndThere(myPosition + Vector2.right))
            {
                break;
            }
            if (additionalDirections.Length == 0)
            {
                destination = firstDirection + new Vector2(
                        UnityEngine.Random.Range(-movementDistance * movementRandomness, movementDistance * movementRandomness),
                        UnityEngine.Random.Range(-movementDistance * movementRandomness, movementDistance * movementRandomness));
                destination = Vector2.ClampMagnitude(destination, movementDistance);
            }
            else
            {
                int directionChosen = UnityEngine.Random.Range(0, additionalDirections.Length * 2 + 1);
                if (directionChosen >= additionalDirections.Length * 2 - 1)
                {
                    destination = firstDirection + new Vector2(
                            UnityEngine.Random.Range(-movementDistance * movementRandomness, movementDistance * movementRandomness),
                            UnityEngine.Random.Range(-movementDistance * movementRandomness, movementDistance * movementRandomness));
                    destination = Vector2.ClampMagnitude(destination, movementDistance);
                }
                else
                {
                    destination = PositionAccordingToAdditionalDirection(additionalDirections[Mathf.FloorToInt(directionChosen * 0.5f)], destination, pathWidth);
                    destination += new Vector2(
                            UnityEngine.Random.Range(-movementDistance * movementRandomness, movementDistance * movementRandomness),
                            UnityEngine.Random.Range(-movementDistance * movementRandomness, movementDistance * movementRandomness));
                    destination = Vector2.ClampMagnitude(destination, movementDistance);
                }
            }
        }
        return destination;
    }
    private enum AdditionalDirections
    {
        behind, longLeft, longMiddle, longRight, middleLeft, middleMiddle, middleRight, shortLeft, shortMiddle, shortRight
    }
    private Vector2 PositionAccordingToAdditionalDirection(AdditionalDirections direction, Vector2 positionToGetTo, float pathWidth)
    {
        Vector2 myPosition = transform.position;
        float maxDistance = Vector2.Distance(positionToGetTo, myPosition);
        float longDistance = maxDistance * 0.8f;
        float middleDistance = maxDistance * 0.5f;
        float shortDistance = maxDistance * 0.2f;

        #region Angle
        Vector2 baseRaycast = positionToGetTo - myPosition;

        float calculationAdjustment = 0f;
        if (baseRaycast.x < 0 || baseRaycast.y < 0)
        {
            if (baseRaycast.x > 0)
                calculationAdjustment = Mathf.PI * 2;
            else
                calculationAdjustment = Mathf.PI;
        }
        float angle = Mathf.Atan(baseRaycast.y / baseRaycast.x) + calculationAdjustment - Mathf.PI * 0.5f;
        #endregion
        Vector2 leftOffset = new Vector2(-pathWidth * 0.5f * Mathf.Cos(angle), -pathWidth * 0.5f * Mathf.Sin(angle));
        Vector2 rightOffset = new Vector2(pathWidth * 0.5f * Mathf.Cos(angle), pathWidth * 0.5f * Mathf.Sin(angle));

        switch (direction)
        {
            case AdditionalDirections.behind:
                Vector2 behindMe = -(positionToGetTo - myPosition) * 0.5f + myPosition;
                if (UnityEngine.Random.Range(0, 3) == 0)
                {
                    return behindMe;
                }
                else
                {
                    return UnityEngine.Random.Range(0, 2) == 0 ? behindMe + leftOffset : behindMe + rightOffset;
                }
            case AdditionalDirections.longLeft:
                return Vector2.ClampMagnitude(positionToGetTo - myPosition, longDistance) + myPosition + leftOffset;
            case AdditionalDirections.longMiddle:
                return Vector2.ClampMagnitude(positionToGetTo - myPosition, longDistance) + myPosition;
            case AdditionalDirections.longRight:
                return Vector2.ClampMagnitude(positionToGetTo - myPosition, longDistance) + myPosition + rightOffset;
            case AdditionalDirections.middleLeft:
                return Vector2.ClampMagnitude(positionToGetTo - myPosition, middleDistance) + myPosition + leftOffset;
            case AdditionalDirections.middleMiddle:
                return Vector2.ClampMagnitude(positionToGetTo - myPosition, middleDistance) + myPosition;
            case AdditionalDirections.middleRight:
                return Vector2.ClampMagnitude(positionToGetTo - myPosition, middleDistance) + myPosition + rightOffset;
            case AdditionalDirections.shortLeft:
                return Vector2.ClampMagnitude(positionToGetTo - myPosition, shortDistance) + myPosition + leftOffset;
            case AdditionalDirections.shortMiddle:
                return Vector2.ClampMagnitude(positionToGetTo - myPosition, shortDistance) + myPosition;
            case AdditionalDirections.shortRight:
                return Vector2.ClampMagnitude(positionToGetTo - myPosition, shortDistance) + myPosition + rightOffset;
            default:
                return Vector2.zero;
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
