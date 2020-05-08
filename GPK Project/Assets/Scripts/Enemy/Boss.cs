using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    #region Initialization
    private float Recoloration { get { return _recoloration; } set { _recoloration = Mathf.Clamp(value, 0, 1); } }

    private int bossPhaseIndex = 0;
    public BossPhaseInfo[] bossPhases;
    private bool canBeDamaged = false;
    private bool amThrowingAttacks = true;
    private int attacksBeforeBubble = 0;
    private int bubblesBeforeAttack = 0;
    private int beatsUntilAction;
    private bool tryingToSwitchAction = false;
    private bool isPassive;

    private List<GameObject> currentActions = new List<GameObject>();


    private float _recoloration;
    private int lastAttackIndex;
    private int lastBubbleIndex;

    #endregion

    private void Start()
    {
        //Initialisation du boss, (cinématique) et début de phase 1
        BossPhaseInit();
    }

    void Update()
    {
        if (GameManager.Instance.Beat.onBeatSingleFrame)
        {
            if (!isPassive)
            {
                if (beatsUntilAction <= 0)
                {
                    if (tryingToSwitchAction)
                    {
                        if (CanSwitchActions())
                        {
                            Action();
                        }
                    }
                    else
                    {
                        Action();
                    }
                }
                else
                {
                    beatsUntilAction--;
                }

                if(Recoloration >= 1)
                {
                    canBeDamaged = true;
                }
            }
        }
    }

    private void Action()
    {
        if (amThrowingAttacks)
        {
            int attackChosen = Random.Range(0, bossPhases[bossPhaseIndex].allDifferentAOE.Length);
            if (!bossPhases[bossPhaseIndex].sameActionTwicePossible && attackChosen == lastAttackIndex)
            {
                while (attackChosen == lastAttackIndex)
                {
                    attackChosen = Random.Range(0, bossPhases[bossPhaseIndex].allDifferentAOE.Length);
                }
            }
            beatsUntilAction = Mathf.RoundToInt(bossPhases[bossPhaseIndex].allDifferentAOE[attackChosen].PatternTime() / BeatManager.Instance.BeatTime);
            lastAttackIndex = attackChosen;
            attacksBeforeBubble--;
            if (attacksBeforeBubble <= 0)
            {
                amThrowingAttacks = false;
                attacksBeforeBubble = bossPhases[bossPhaseIndex].numberOfAttacksBeforeBubble;
                tryingToSwitchAction = true;
            }

            currentActions.Add(Instantiate(bossPhases[bossPhaseIndex].allDifferentAOE[attackChosen].gameObject));
        }
        else
        {
            int bubbleChosen = Random.Range(0, bossPhases[bossPhaseIndex].allDifferentBubbleThrows.Length);
            if (!bossPhases[bossPhaseIndex].sameActionTwicePossible && bubbleChosen == lastBubbleIndex)
            {
                while (bubbleChosen == lastBubbleIndex)
                {
                    bubbleChosen = Random.Range(0, bossPhases[bossPhaseIndex].allDifferentBubbleThrows.Length);
                }
            }
            beatsUntilAction = bossPhases[bossPhaseIndex].beatsBetweenBubbles;
            lastBubbleIndex = bubbleChosen;
            bubblesBeforeAttack--;
            if (bubblesBeforeAttack <= 0)
            {
                amThrowingAttacks = true;
                bubblesBeforeAttack = bossPhases[bossPhaseIndex].numberOfBubblesBeforeAttack;
                tryingToSwitchAction = true;
            }

            GameObject currentBubble = Instantiate(bossPhases[bossPhaseIndex].allDifferentBubbleThrows[bubbleChosen].gameObject);
            currentActions.Add(currentBubble);
            currentBubble.GetComponent<InkBubble>().boss = this;
        }
    }

    private void BossPhaseInit() //appelé quand on commence une nouvelle suite d'attaques / bulles
    {
        bossPhaseIndex++;
        BossPhaseInfo currentPhaseInfo = bossPhases[bossPhaseIndex];
        attacksBeforeBubble = currentPhaseInfo.numberOfAttacksBeforeBubble;
        bubblesBeforeAttack = currentPhaseInfo.numberOfBubblesBeforeAttack;
        isPassive = false;
        beatsUntilAction = 0;
        Recoloration = 0;
    }

    private bool CanSwitchActions()
    {
        foreach(GameObject action in currentActions)
        {
            if(action != null)
            {
                return false;
            }
        }
        currentActions.Clear(); //à vérifier
        return true;
    }

    public void TakeDamage()
    {
        if(isPassive && canBeDamaged)
        {
            canBeDamaged = false;
            Recoloration = 0.9999f;
            StartCoroutine(Transition());
        }
    }

    public void AddRecoloration()
    {
        Recoloration += 1 / (float)bossPhases[bossPhaseIndex].numberOfBubblesNeededToRecolorEverything;
    }

    public void LoseRecoloration()
    {
        Recoloration -= 1 / (float)bossPhases[bossPhaseIndex].numberOfBubblesNeededToRecolorEverything;
    }

    private IEnumerator Transition()
    {
        //animator setBool de la transition
        yield return new WaitForSeconds(bossPhases[bossPhaseIndex].endingPhaseAnimation.length);
        BossPhaseInit();
    }

    [System.Serializable]
    public class BossPhaseInfo
    {
        public bool sameActionTwicePossible = false;
        [Header("Attack related")]
        public BossAOEPattern[] allDifferentAOE;
        [Range(0,10)] public int numberOfAttacksBeforeBubble;
        [Header("Bubble related")]
        public InkBubble[] allDifferentBubbleThrows;
        [Range(0,10)] public int beatsBetweenBubbles;
        [Range(0,10)] public int numberOfBubblesBeforeAttack;
        [Range(0,10)] public int numberOfBubblesNeededToRecolorEverything;

        [Header("Other Infos")]
        public AnimationClip endingPhaseAnimation;
    }
}
