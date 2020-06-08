using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkAttack : MonoBehaviour
{
    public float minHoldTimeToStartCharge;
    public int[] attackDamage;
    public Color[] attackColor;
    public Vector2 attackInitialRange;
    public float attackMissRange;
    public float attackKnockbackDistance;
    public bool bossCombat;
    public GameObject attackDirectionPreview;
    public GameObject attackFx, missAttackFx;
    [Header("Sounds")]
    public AudioClip attackOnBeatSound;
    public AudioClip attackMissBeatSound;

    private bool isCharging;
    private Vector2 attackDirection;
    private float attackDirectionAngle;
    private Vector2 worldMousePos;
    private ContactFilter2D enemyFilter;
    private ContactFilter2D inkBubbleFilter;
    private float remainingHoldingTime;
    private RemoteSpeaker remoteSpeaker;

    void Start()
    {
        remoteSpeaker = GetComponent<RemoteSpeaker>();
        enemyFilter = new ContactFilter2D();
        enemyFilter.SetLayerMask(LayerMask.GetMask("Enemy"));
        enemyFilter.useTriggers = true;
        if(bossCombat)
        {
            inkBubbleFilter = new ContactFilter2D();
            inkBubbleFilter.SetLayerMask(LayerMask.GetMask("InkBubble"));
            inkBubbleFilter.useTriggers = true;
        }
        remainingHoldingTime = -10;
    }

    void Update()
    {
        UpdateHold();
        UpdateCharge();
    }


    private void UpdateHold()
    {
        if (Input.GetButton("Blink"))
        {
            if (remainingHoldingTime > 0)
            {
                remainingHoldingTime -= Time.deltaTime;
            }
            else if (remainingHoldingTime != -10)
            {
                isCharging = true;
                remainingHoldingTime = -10;
            }
        }
        else
        {
            remainingHoldingTime = -10;
        }
    }

    private void UpdateCharge()
    {
        if (isCharging)
        {
            GameManager.Instance.playerManager.isInControl = false;
            attackDirectionPreview.SetActive(true);
            worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            attackDirection = worldMousePos - (Vector2)transform.parent.position;
            attackDirection.Normalize();
            attackDirectionAngle = Vector2.SignedAngle(Vector2.right, attackDirection);
            attackDirectionPreview.transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, attackDirectionAngle - 90));

            if(Input.GetButtonUp("Blink"))
            {
                if(BeatManager.Instance.CanAct())
                {
                    Attack(BeatManager.Instance.OnBeat(GameManager.Instance.playerManager.playerOffBeated, true, "AttackRelease"));
                }
                else
                {
                    Instantiate(GameManager.Instance.blink.overActionEffectPrefab, transform.parent.position, Quaternion.identity);
                    GameManager.Instance.blink.FailCombo();
                    StopCharge();
                }
            }
        }
        else
        {
            attackDirectionPreview.SetActive(false);
        }
    }

    public void HasBlinked()
    {
        remainingHoldingTime = minHoldTimeToStartCharge;
    }

    private void Attack(bool onBeat)
    {
        GameManager.Instance.playerManager.ResetIdleTime();
        GameManager.playerSource.PlayOneShot(onBeat ? attackOnBeatSound : attackMissBeatSound);
        isCharging = false;
        float currentAttackLength = onBeat ? attackInitialRange.x : attackMissRange;
        SpriteRenderer fxSprite = Instantiate(onBeat ? attackFx : missAttackFx, (Vector2)transform.position + attackDirection * currentAttackLength * 0.5f, Quaternion.Euler(new Vector3(0.0f, 0.0f, attackDirectionAngle))).GetComponent<SpriteRenderer>();
        fxSprite.color = attackColor[GameManager.Instance.playerManager.currentPower];

        if (remoteSpeaker.speakerPlaced)
        {
            remoteSpeaker.Attack(onBeat,
                new Vector2(currentAttackLength, attackInitialRange.y),
                attackDamage[GameManager.Instance.playerManager.currentPower],
                onBeat ? attackFx : missAttackFx,
                attackColor[GameManager.Instance.playerManager.currentPower],
                enemyFilter);
        }

        List<Collider2D> colliders = new List<Collider2D>();
        Physics2D.OverlapBox((Vector2)transform.position + attackDirection * currentAttackLength * 0.5f, new Vector2(currentAttackLength, attackInitialRange.y), attackDirectionAngle, enemyFilter, colliders);
        if(colliders.Count > 0)
        {
            foreach(Collider2D collider in colliders)
            {
                EnemyBase enemy = collider.transform.parent.GetComponentInChildren<EnemyBase>();
                enemy.TakeDamage(attackDamage[GameManager.Instance.playerManager.currentPower], attackDirection * attackKnockbackDistance);
            }
        }

        if(bossCombat)
        {
            Physics2D.OverlapBox((Vector2)transform.position + attackDirection * currentAttackLength * 0.5f, new Vector2(currentAttackLength, attackInitialRange.y), attackDirectionAngle, inkBubbleFilter, colliders);
            if (colliders.Count > 0)
            {
                foreach (Collider2D collider in colliders)
                {
                    InkBubble inkBubble = collider.GetComponent<InkBubble>();
                    inkBubble.Convert();
                }
            }

            Collider2D bossCollider = Physics2D.OverlapBox((Vector2)transform.position + attackDirection * currentAttackLength * 0.5f, new Vector2(currentAttackLength, attackInitialRange.y), attackDirectionAngle, LayerMask.GetMask("Boss"));
            if (bossCollider != null)
            {
                Boss boss = bossCollider.GetComponent<Boss>();
                boss.TakeDamage();
            }
        }
        GameManager.Instance.playerManager.isInControl = true;
    }

    private void StopCharge()
    {
        isCharging = false;
        GameManager.Instance.playerManager.isInControl = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(new Vector2(transform.position.x  + 0.3f + attackInitialRange.x * 0.5f, transform.position.y), attackInitialRange);
    }
}
