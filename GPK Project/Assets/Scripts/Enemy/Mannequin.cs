using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mannequin : EnemyBase
{
    public AnimationCurve knockbackCurve;

    protected override EnemyBehaviour[] PassivePattern => passivePattern;
    protected override EnemyBehaviour[] TriggeredPattern => triggeredPattern;


    private EnemyBehaviour[] passivePattern = new EnemyBehaviour[]
    {
        new EnemyBehaviour(EnemyState.Vulnerable, true)
    };

    private EnemyBehaviour[] triggeredPattern = new EnemyBehaviour[]
    {

    };

    protected override void Init()
    {

    }

    protected override void ConvertedBehaviour()
    {
        enemyMaxHP = 100;
        enemyCurrentHP = 100;
    }

    protected override void VulnerableBehaviour()
    {
        canBeDamaged = true;
        enemyMaxHP = 100;
        enemyCurrentHP = 100;
    }

    protected override void KnockbackBehaviour()
    {
        canBeDamaged = false;
        if ((Time.fixedTime - startKnockBackTime) < GameManager.Instance.Beat.BeatTime)
        {
            Vector2 nextKnockbackPos = (Vector3)Vector2.Lerp(knockbackStartPos, knockbackStartPos + knockback, knockbackCurve.Evaluate((Time.fixedTime - startKnockBackTime) / GameManager.Instance.Beat.BeatTime));
            if (!Physics2D.OverlapPoint(nextKnockbackPos + knockback.normalized * 0.5f, LayerMask.GetMask("Obstacle")))
            {
                parent.position = nextKnockbackPos;
            }
        }
    }
}
