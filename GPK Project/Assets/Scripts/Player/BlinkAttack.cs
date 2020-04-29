using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkAttack : MonoBehaviour
{
    public int attackDamage;
    public Vector2 attackInitialRange;
    public GameObject attackDirectionPreview;
    public GameObject chargeBeginParticle;
    public GameObject attackFx;

    private bool isCharging;
    private Vector2 attackDirection;
    private float attackDirectionAngle;
    private Vector2 worldMousePos;
    private ContactFilter2D enemyFilter;

    void Start()
    {
        enemyFilter = new ContactFilter2D();
        enemyFilter.SetLayerMask(LayerMask.GetMask("Enemy"));
        enemyFilter.useTriggers = true;
    }

    void Update()
    {
        UpdateCharge();
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

            if(Input.GetButtonUp("Attack"))
            {
                Attack();
            }
        }
        else
        {
            attackDirectionPreview.SetActive(false);
        }
    }

    public void StartCharge()
    {
        isCharging = true;
        Instantiate(chargeBeginParticle, transform.position, Quaternion.identity);
    }

    private void Attack()
    {
        isCharging = false;
        Instantiate(attackFx, (Vector2)transform.position + attackDirection * attackInitialRange.x * 0.5f, Quaternion.Euler(new Vector3(0.0f, 0.0f, attackDirectionAngle)));
        List<Collider2D> colliders = new List<Collider2D>();
        Physics2D.OverlapBox((Vector2)transform.position + attackDirection * attackInitialRange.x * 0.5f, attackInitialRange, attackDirectionAngle, enemyFilter, colliders);
        if(colliders.Count > 0)
        {
            foreach(Collider2D collider in colliders)
            {
                EnemyBase enemy = collider.transform.parent.GetComponentInChildren<EnemyBase>();
                enemy.TakeDamage(attackDamage);
            }
        }

        GameManager.Instance.playerManager.isInControl = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(new Vector2(transform.position.x  + 0.3f + attackInitialRange.x * 0.5f, transform.position.y), attackInitialRange);
    }
}
