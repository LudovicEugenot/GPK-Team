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
    public Talk finalTalk;
    public Hook hookToMoveTo;
    public GameObject destroyHookEffect;
    public AudioClip hookDestructionSound;
    public Transform bossCinematicPos;

    private bool canBeDamaged = false;
    private bool throwingAOE = true;
    private int attacksBeforeBubble = 0;
    private int bubblesBeforeAttack = 0;
    private int beatsUntilAction;
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
        if (!WorldManager.GetWorldEvent(WorldManager.EventName.BossBeaten).occured)
        {
            Invoke("LateStart", 0.5f);
        }
        source = GetComponent<AudioSource>();
        isPassive = true;
        animator = GetComponent<Animator>();
    }


    private void LateStart()
    {
        ZoneHandler.Instance.reliveRemotlyChanged = true;
        GameManager.Instance.blink.BlinkMove(hookToMoveTo.transform.position);
        GameManager.Instance.dialogueManager.StartTalk(firstTalk, bossCinematicPos.position, 4);
        init = true;
    }
    void Update()
    {
        if(init && GameManager.Instance.playerManager.isInControl)
        {
            init = false;
            BossPhaseInit();
        }

        if(GameManager.Instance.playerManager.dying)
        {
            isPassive = true;
        }

        if (GameManager.Instance.Beat.onBeatSingleFrame)
        {
            if (!isPassive)
            {
                if(attacksBeforeBubble <= 0)
                {
                    throwingAOE = false;
                }

                if(throwingAOE)
                {
                    ZoneHandler.Instance.bossState = 1;
                    if (beatsUntilAction <= 0)
                    {
                        ThrowAOE();
                    }
                    else
                    {
                        beatsUntilAction--;
                    }
                }
                else
                {
                    ZoneHandler.Instance.bossState = 2;

                    if (beatsUntilAction <= 0)
                    {
                        ThrowBubble();
                    }
                    else
                    {
                        beatsUntilAction--;
                    }
                }

                if(Recoloration >= 1)
                {
                    animator.SetTrigger("Fatigued");
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
            else
            {
                ZoneHandler.Instance.bossState = 2;
            }
        }

        ZoneHandler.Instance.currentReliveProgression = Recoloration;
    }

    private void ThrowAOE()
    {
        animator.SetTrigger("Attack");
        int attackChosen;
        BossAOEPattern patternChosen;

        do
        {
            do
            {
                attackChosen = Random.Range(0, bossPhases[bossPhaseIndex].allDifferentAOE.Length);
            }
            while (!bossPhases[bossPhaseIndex].sameActionTwicePossible && attackChosen == lastAttackIndex);

            lastAttackIndex = attackChosen;
            patternChosen = bossPhases[bossPhaseIndex].allDifferentAOE[attackChosen];
            attacksBeforeBubble--;

            if (attacksBeforeBubble <= 0)
            {
                throwingAOE = false;
                attacksBeforeBubble = bossPhases[bossPhaseIndex].numberOfAttacksBeforeBubble;
            }

            currentActions.Add(Instantiate(patternChosen.gameObject));
        }
        while (patternChosen.startNextAoeDirectly && patternChosen.patternAoes[patternChosen.patternAoes.Count - 1].beatTimeBeforeNextAOE == 0);

        beatsUntilAction = patternChosen.startNextAoeDirectly ?
            (int)patternChosen.patternAoes[patternChosen.patternAoes.Count - 1].beatTimeBeforeNextAOE
            : Mathf.RoundToInt(patternChosen.PatternTime() / BeatManager.Instance.BeatTime);
    }

    private void ThrowBubble()
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
            throwingAOE = true;
            bubblesBeforeAttack = bossPhases[bossPhaseIndex].numberOfBubblesBeforeAttack;
            if(bossPhaseIndex < bossPhases.Length - 1)
            {
                GameManager.Instance.dialogueManager.StartCommentary(bubbleExplanation, 2.0f, Vector2.zero);
            }
        }

        GameObject currentBubble = Instantiate(bossPhases[bossPhaseIndex].allDifferentBubbleThrows[bubbleChosen].gameObject);
        currentActions.Add(currentBubble);
        currentBubble.GetComponent<InkBubble>().boss = this;
    }

    private void BossPhaseInit()
    {
        bossPhaseIndex++;
        GameManager.Instance.blink.BlinkMove(hookToMoveTo.transform.position);
        if (bossPhaseIndex < bossPhases.Length)
        {
            BossPhase currentPhaseInfo = bossPhases[bossPhaseIndex];
            attacksBeforeBubble = currentPhaseInfo.numberOfAttacksBeforeBubble;
            bubblesBeforeAttack = currentPhaseInfo.numberOfBubblesBeforeAttack;
            isPassive = false;
            throwingAOE = true;
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
        while(GameManager.Instance.dialogueManager.isTalking)
        {
            yield return new WaitForEndOfFrame();
        }
        triggeredWolrdEventWhenDefeated.occured = true;
        yield return new WaitForSeconds(2.0f);
        ZoneHandler.Instance.reliveRemotlyChanged = false;
        GameManager.Instance.dialogueManager.StartTalk(finalTalk, GameManager.Instance.player.transform.position, 3);
    }

    private IEnumerator Transition()
    {
        animator.SetInteger("BossPhaseIndex", bossPhaseIndex + 1);
        yield return new WaitForSeconds(bossPhases[bossPhaseIndex].endingPhaseAnimation.length);
        GameManager.Instance.dialogueManager.StartTalk(bossPhases[bossPhaseIndex].endPhaseTalk, bossCinematicPos.position, 4);
        while(!GameManager.Instance.playerManager.isInControl)
        {
            yield return new WaitForEndOfFrame();
        }

        if (bossPhases[bossPhaseIndex].hooksDestroyedEndOfPhase.Length > 0)
        {
            StartCoroutine(GameManager.Instance.cameraHandler.StartCinematicLook(Vector2.zero, 5.625f, true));
            yield return new WaitForSeconds(2.0f);
            source.pitch = 1.3f;
            source.PlayOneShot(hookDestructionSound);
            foreach (Hook hook in bossPhases[bossPhaseIndex].hooksDestroyedEndOfPhase)
            {
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
