using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Range_Enemy : EnemyBase
{
    public Transform alternateSpotTransform;
    public int beamDamage;
    public int barrierDamage;
    public float barrierLength;
    public BoxCollider2D barrierCollider;
    public LineRenderer beamLine;
    public float[] beamLineWidths;

    public AnimationCurve knockbackCurve;
    public GameObject apparitionFx;
    public GameObject disparitionFx;

    protected override EnemyBehaviour[] PassivePattern => passivePattern;
    protected override EnemyBehaviour[] TriggeredPattern => triggeredPattern;

    private ContactFilter2D playerFilter = new ContactFilter2D();
    private Vector2 beamTarget;
    private Vector2 initialSpot;
    private Vector2 alternateSpot;
    private Vector2 currentSpot;
    private bool moveFlag;
    private bool triggeredFlag;
    private SpriteRenderer spriteRenderer;


    private EnemyBehaviour[] passivePattern = new EnemyBehaviour[]
    {
        new EnemyBehaviour(EnemyState.Idle),
        new EnemyBehaviour(EnemyState.Idle),
        new EnemyBehaviour(EnemyState.Idle),
        new EnemyBehaviour(EnemyState.Defense),
        new EnemyBehaviour(EnemyState.Defense),
        new EnemyBehaviour(EnemyState.Action),
        new EnemyBehaviour(EnemyState.Vulnerable),
        new EnemyBehaviour(EnemyState.Vulnerable),
        new EnemyBehaviour(EnemyState.Vulnerable),
        new EnemyBehaviour(EnemyState.Vulnerable)
    };

    private EnemyBehaviour[] triggeredPattern = new EnemyBehaviour[]
    {
        new EnemyBehaviour(EnemyState.Triggered),
        new EnemyBehaviour(EnemyState.Moving)
    };


    protected override void Init()
    {
        playerFilter.useTriggers = true;
        playerFilter.SetLayerMask(LayerMask.GetMask("Player"));
        initialSpot = parent.position;
        alternateSpot = alternateSpotTransform.position;
        barrierCollider.gameObject.SetActive(false);
        spriteRenderer = parent.GetComponentInChildren<SpriteRenderer>();
        currentSpot = initialSpot;
    }

    protected override void IdleBehaviour()
    {
        barrierCollider.gameObject.SetActive(false);
        canBeDamaged = true;
        beamLine.enabled = false;

        moveFlag = true;
        triggeredFlag = true;
        if (PlayerIsInAggroRange())
        {
            GetTriggered();
        }

        if(BeatManager.Instance.currentBeatProgression > 0.9f)
        {
            beamTarget = GetLastSeenPlayerPosition();
        }
    }

    protected override void DefenseBehaviour()
    {
        barrierCollider.gameObject.SetActive(false);
        canBeDamaged = true;
        Vector2 playerDirection = beamTarget - (Vector2)parent.position;
        playerDirection.Normalize();
        parent.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, playerDirection));

        beamLine.enabled = true;
        Vector3[] beamLinePos = new Vector3[2];
        beamLinePos[0] = parent.position;
        beamLinePos[1] = (Vector2)parent.position + playerDirection * 30;
        beamLine.SetPositions(beamLinePos);
        beamLine.startWidth = beamLineWidths[0];
        beamLine.endWidth = beamLineWidths[0];

        moveFlag = true;
        triggeredFlag = true;
        if (PlayerIsInAggroRange())
        {
            GetTriggered();
        }
    }

    protected override void ActionBehaviour()
    {
        canBeDamaged = true;
        Vector2 playerDirection = beamTarget - (Vector2)parent.position;
        playerDirection.Normalize();
        beamLine.enabled = true;
        if(BeatManager.Instance.currentBeatProgression < 0.2f)
        {
            beamLine.startWidth = beamLineWidths[1];
            beamLine.endWidth = beamLineWidths[1];

            RaycastHit2D hit = Physics2D.Raycast(parent.position, playerDirection, 30, LayerMask.GetMask("Player"));
            if(hit)
            {
                GameManager.Instance.playerManager.TakeDamage(2);
            }
        }
        else
        {
            beamLine.startWidth = beamLineWidths[2];
            beamLine.endWidth = beamLineWidths[2];
            barrierCollider.gameObject.SetActive(true);
            barrierCollider.transform.localScale = new Vector2(barrierLength, 0.3f);
            barrierCollider.transform.position = (Vector2)parent.position + playerDirection * barrierLength * 0.5f;
            barrierCollider.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, playerDirection));
        }
    }

    protected override void VulnerableBehaviour()
    {
        barrierCollider.gameObject.SetActive(true);
        canBeDamaged = true;
        beamLine.enabled = true;
        beamLine.startWidth = beamLineWidths[2];
        beamLine.endWidth = beamLineWidths[2];
    }

    protected override void TriggeredBehaviour()
    {
        if(triggeredFlag)
        {
            triggeredFlag = false;
            Instantiate(disparitionFx, parent.position, Quaternion.identity);
        }
        barrierCollider.gameObject.SetActive(false);
        canBeDamaged = false;
        beamLine.enabled = false;
        spriteRenderer.enabled = false;
    }

    protected override void MovingBehaviour()
    {
        spriteRenderer.enabled = true;
        canBeDamaged = false;
        beamLine.enabled = false;
        if(moveFlag)
        {
            moveFlag = false;
            if(currentSpot == initialSpot)
            {
                parent.position = alternateSpot;
                currentSpot = alternateSpot;
            }
            else
            {
                parent.position = initialSpot;
                currentSpot = initialSpot;
            }
            Instantiate(apparitionFx, transform.position, Quaternion.identity);
        }
    }

    protected override void KnockbackBehaviour()
    {
        beamLine.enabled = false;
    }

    protected override void ConvertedBehaviour()
    {
        beamLine.enabled = false;
        spriteRenderer.enabled = true;
        barrierCollider.gameObject.SetActive(false);
        // juste un état passif où on s'assure que l'animator est au bon endroit toussa
        // (le fait de convertir se fait pas ici)
    }

    protected override void OnConverted()
    {
        beamLine.enabled = false;
        spriteRenderer.enabled = true;
        barrierCollider.gameObject.SetActive(false);
        animator.SetBool("Converted", true);
    }

}
