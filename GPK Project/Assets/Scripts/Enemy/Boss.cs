using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    #region Initialization
    private float Recoloration { get { return _recoloration; } set { _recoloration = Mathf.Clamp(value, 0, 1); } }

    private int bossPhaseIndex = -1;
    public Talk firstTalk;
    public Talk bubbleExplanation;
    public BossPhase[] bossPhases;
    public WorldManager.EventName triggeredEventWhenDefeated;
    public Hook hookToMoveTo;
    public GameObject destroyHookEffect;
    public AudioClip hookDestructionSound;

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

    private WorldManager.WorldEvent triggeredWolrdEventWhenDefeated;
    private Animator animator;
    private bool init;
    private AudioSource source;

    #endregion

    private void Start()
    {
        triggeredWolrdEventWhenDefeated = WorldManager.GetWorldEvent(triggeredEventWhenDefeated);
        source = GetComponent<AudioSource>();
        isPassive = true;
        animator = GetComponent<Animator>();
        Invoke("LateStart", 0.5f);
    }


    private void LateStart()
    {
        ZoneHandler.Instance.reliveRemotlyChanged = true;
        GameManager.Instance.blink.BlinkMove(hookToMoveTo.transform.position);
        GameManager.Instance.dialogueManager.StartTalk(firstTalk, transform.position, 4);
        init = true;
    }
    void Update()
    {
        if(init && GameManager.Instance.playerManager.isInControl)
        {
            init = false;
            BossPhaseInit();
        }

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
                    isPassive = true;
                    canBeDamaged = true;
                    for (int i = 0; i < currentActions.Count; i++)
                    {
                        if(currentActions[i] != null)
                        {
                            Destroy(currentActions[i]);
                        }
                    }
                    currentActions.Clear();
                }
            }
        }

        ZoneHandler.Instance.currentReliveProgression = Recoloration;
    }

    private void Action()
    {
        animator.SetTrigger("Attack");
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
                GameManager.Instance.dialogueManager.StartCommentary(bubbleExplanation, 2.0f, Vector2.zero);
            }

            GameObject currentBubble = Instantiate(bossPhases[bossPhaseIndex].allDifferentBubbleThrows[bubbleChosen].gameObject);
            currentActions.Add(currentBubble);
            currentBubble.GetComponent<InkBubble>().boss = this;
        }
    }

    private void BossPhaseInit()
    {
        bossPhaseIndex++;
        if(bossPhaseIndex < bossPhases.Length)
        {
            BossPhase currentPhaseInfo = bossPhases[bossPhaseIndex];
            attacksBeforeBubble = currentPhaseInfo.numberOfAttacksBeforeBubble;
            bubblesBeforeAttack = currentPhaseInfo.numberOfBubblesBeforeAttack;
            isPassive = false;
            amThrowingAttacks = true;
            beatsUntilAction = 0;
            Recoloration = 0;
        }
        else
        {
            StartCoroutine(EndBoss());
        }
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
        currentActions.Clear();
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

    private IEnumerator EndBoss()
    {
        triggeredWolrdEventWhenDefeated.occured = true;
        Debug.Log("Bravo, tu as ramené la couleur dans le monde !");
        yield return new WaitForSeconds(2.0f);
        ZoneHandler.Instance.reliveRemotlyChanged = false;
    }

    private IEnumerator Transition()
    {
        animator.SetInteger("BossPhaseIndex", bossPhaseIndex + 1);
        yield return new WaitForSeconds(bossPhases[bossPhaseIndex].endingPhaseAnimation.length);
        GameManager.Instance.dialogueManager.StartTalk(bossPhases[bossPhaseIndex].endPhaseTalk, transform.position, 4);
        while(!GameManager.Instance.playerManager.isInControl)
        {
            yield return new WaitForEndOfFrame();
        }

        if (bossPhases[bossPhaseIndex].hooksDestroyedEndOfPhase.Length > 0)
        {
            StartCoroutine(GameManager.Instance.cameraHandler.StartCinematicLook(Vector2.zero, 5.625f, true));
            yield return new WaitForSeconds(2.0f);
            foreach (Hook hook in bossPhases[bossPhaseIndex].hooksDestroyedEndOfPhase)
            {
                source.pitch = 1.3f;
                source.PlayOneShot(hookDestructionSound);
                Instantiate(destroyHookEffect, hook.transform.position, Quaternion.identity);
                Destroy(hook.gameObject);
            }
            yield return new WaitForSeconds(1.0f);
            StartCoroutine(GameManager.Instance.cameraHandler.StopCinematicLook());
        }

        BossPhaseInit();
    }

    [System.Serializable]
    public class BossPhase
    {
        public bool sameActionTwicePossible = false;
        [Header("Attack related")]
        public BossAOEPattern[] allDifferentAOE;
        [Range(0,30)] public int numberOfAttacksBeforeBubble;
        [Header("Bubble related")]
        public InkBubble[] allDifferentBubbleThrows;
        [Range(0,10)] public int beatsBetweenBubbles;
        [Range(0,20)] public int numberOfBubblesBeforeAttack;
        [Range(0,20)] public int numberOfBubblesNeededToRecolorEverything;
        public Talk endPhaseTalk;
        public Hook[] hooksDestroyedEndOfPhase;

        [Header("Other Infos")]
        public AnimationClip endingPhaseAnimation;
    }
}
