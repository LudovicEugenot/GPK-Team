using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    #region Initialization
    private int bossPhaseIndex = 0;
    public BossPhaseInfo[] bossPhases;
    private bool canBeDamaged;
    private bool amThrowingAttacks = true;
    private int beatsUntilAttack;

    private List<GameObject> currentAttacks = new List<GameObject>();
    
    
    private int lastAttackIndex;
    
    #endregion

    private void Start()
    {
        //Initialisation du boss, (cinématique) et début de phase 1
    }

    void Update()
    {
        if (GameManager.Instance.Beat.onBeatSingleFrame)
        {
            if (beatsUntilAttack <= 0)
            {
            currentAttacks.Add(Instantiate(NextMove()));
            }
            else
            {
                beatsUntilAttack--;
            }
        }
    }

    private GameObject NextMove()
    {
        if (amThrowingAttacks)
        {
        int attackChosen = Random.Range(0, bossPhases[bossPhaseIndex].allDifferentAOE.Length);
        if (!bossPhases[bossPhaseIndex].sameAttackTwicePossible && attackChosen == lastAttackIndex)
        {
            while (attackChosen == lastAttackIndex)
            {
                attackChosen = Random.Range(0, bossPhases[bossPhaseIndex].allDifferentAOE.Length);
            }
        }

        return bossPhases[bossPhaseIndex].allDifferentAOE[attackChosen];
                    }
        else
        {
            int bubbleChosen = Random.Range(0, bossPhases[bossPhaseIndex].allDifferentBubbleThrows.Length);
            if (!bossPhases[bossPhaseIndex].sameAttackTwicePossible && bubbleChosen == lastAttackIndex)
            {
                while (bubbleChosen == lastAttackIndex)
                {
                    bubbleChosen = Random.Range(0, bossPhases[bossPhaseIndex].allDifferentAOE.Length);
                }
            }

            return bossPhases[bossPhaseIndex].allDifferentAOE[bubbleChosen];
        }
    }

    [System.Serializable]
    public class BossPhaseInfo
    {
        public bool sameAttackTwicePossible = false;
        public GameObject[] allDifferentAOE; //////////////////////////////// à changer en le nom du script histoire d'avoir accès direct aux infos de ce qu'il y a dedans etc...
        public GameObject[] allDifferentBubbleThrows;
    }
}
