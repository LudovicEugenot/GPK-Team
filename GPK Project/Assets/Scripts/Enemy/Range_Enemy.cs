using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Range_Enemy : EnemyBase
{
    public Transform alternateSpotTransform;
    public int beamDamage;
    [Range(0.1f, 1.0f)] public float beatProgressionForbeam;
    public int barrierDamage;
    public float barrierLength;
    public BoxCollider2D barrierCollider;
    public float[] beamLineWidths;
    public float spaceBetweenBeamFx;
    public float beamStartDistance;
    public Vector2 beamStartOffset;
    public GameObject beamPartFx;

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
    private List<GameObject> beamFxO = new List<GameObject>();


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
        for (int i = beamFxO.Count - 1; i >= 0; i--)
        {
            Destroy(beamFxO[i]);
            beamFxO.RemoveAt(i);
        }
        beamFxO.Clear();


        animator.SetInteger("BeamState", 0);
        barrierCollider.gameObject.SetActive(false);
        canBeDamaged = true;

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
        animator.SetInteger("BeamState", 1);
        barrierCollider.gameObject.SetActive(false);
        canBeDamaged = true;
        Vector2 playerDirection = beamTarget - ((Vector2)parent.position + beamStartOffset);
        playerDirection.Normalize();

        if (BeatManager.Instance.onBeatSingleFrame && beamFxO.Count == 0)
        {
            beamFxO.Clear();
            int laserFxNumber = Mathf.CeilToInt(barrierLength / spaceBetweenBeamFx);
            GameObject laserFx = null;
            for (int i = 0; i < laserFxNumber; i++)
            {
                if (i == 0)
                {
                    laserFx = beamPartFx;
                }
                else
                {
                    laserFx = beamPartFx;
                }
                beamFxO.Add(Instantiate(laserFx, (Vector2)transform.position + beamStartOffset + playerDirection * beamStartDistance + playerDirection * (i * spaceBetweenBeamFx), Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, playerDirection))));
            }
        }

        moveFlag = true;
        triggeredFlag = true;
        if (PlayerIsInAggroRange())
        {
            GetTriggered();
        }
    }

    protected override void ActionBehaviour()
    {
        animator.SetInteger("BeamState", 2);
        canBeDamaged = true;
        Vector2 playerDirection = beamTarget - ((Vector2)parent.position + beamStartOffset);
        playerDirection.Normalize();
        if(BeatManager.Instance.onBeatSingleFrame)
        {
            int laserFxNumber = Mathf.CeilToInt(barrierLength / spaceBetweenBeamFx);
            for (int i = 0; i < laserFxNumber; i++)
            {
                beamFxO[i].GetComponent<Animator>().SetTrigger("Beam");
            }
        }

        if(BeatManager.Instance.currentBeatProgression < beatProgressionForbeam)
        {
            RaycastHit2D hit = Physics2D.Raycast(parent.position + (Vector3)beamStartOffset, playerDirection, barrierLength, LayerMask.GetMask("Player"));
            if(hit && !BeatManager.Instance.OnBeat(false, false, "__--__"))
            {
                GameManager.Instance.playerManager.TakeDamage(2);
            }
        }
        else
        {
            barrierCollider.gameObject.SetActive(true);
            barrierCollider.transform.localScale = new Vector2(barrierLength, 0.3f);
            barrierCollider.transform.position = (Vector2)parent.position + beamStartOffset + playerDirection * barrierLength * 0.5f;
            barrierCollider.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, playerDirection));
        }
    }

    protected override void VulnerableBehaviour()
    {
        animator.SetInteger("BeamState", 2);
        Vector2 playerDirection = beamTarget - ((Vector2)parent.position + beamStartOffset);
        playerDirection.Normalize();
        barrierCollider.gameObject.SetActive(true);
        barrierCollider.transform.localScale = new Vector2(barrierLength, 0.3f);
        barrierCollider.transform.position = (Vector2)parent.position + beamStartOffset + playerDirection * barrierLength * 0.5f;
        barrierCollider.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, playerDirection));

        canBeDamaged = true;

        if (BeatManager.Instance.onBeatSingleFrame)
        {
            int laserFxNumber = Mathf.CeilToInt(barrierLength / spaceBetweenBeamFx);
            for (int i = 0; i < laserFxNumber; i++)
            {
                beamFxO[i].GetComponent<Animator>().SetTrigger("Barrier");
            }
        }
    }

    protected override void TriggeredBehaviour()
    {
        animator.SetInteger("BeamState", 0);

        for (int i = beamFxO.Count - 1; i >= 0; i--)
        {
            Destroy(beamFxO[i]);
            beamFxO.RemoveAt(i);
        }
        beamFxO.Clear();

        if (triggeredFlag)
        {
            triggeredFlag = false;
            Instantiate(disparitionFx, parent.position, Quaternion.identity);
        }
        barrierCollider.gameObject.SetActive(false);
        canBeDamaged = false;
        spriteRenderer.enabled = false;
    }

    protected override void MovingBehaviour()
    {
        spriteRenderer.enabled = true;
        canBeDamaged = false;
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

    }

    protected override void ConvertedBehaviour()
    {

        spriteRenderer.enabled = true;
        barrierCollider.gameObject.SetActive(false);
        // juste un état passif où on s'assure que l'animator est au bon endroit toussa
        // (le fait de convertir se fait pas ici)
    }

    protected override void OnConverted()
    {
        for (int i = beamFxO.Count - 1; i >= 0; i--)
        {
            Destroy(beamFxO[i]);
            beamFxO.RemoveAt(i);
        }
        beamFxO.Clear();

        spriteRenderer.enabled = true;
        barrierCollider.gameObject.SetActive(false);
        animator.SetBool("Converted", true);
    }

}
