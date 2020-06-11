using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkBubble : MonoBehaviour
{
    [SerializeField] private Transform bubbleDirection = null;
    public float bubbleSpeed;
    public float convertedSpeed;
    public float bubbleSize;
    public AnimationClip apparitionAnim;
    public GameObject blackExplosionPrefab;
    public GameObject colorExplosionPrefab;
    public AudioClip explosionSound;
    [HideInInspector] public Boss boss;

    private Rigidbody2D rb;
    private Vector2 direction;
    private bool convertable;
    private bool isConverted;
    private Animator animator;
    private AudioSource source;
    private Animator bossAnimator;

    void Start()
    {
        bossAnimator = boss.GetComponent<Animator>();
        source = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(Launch());
    }

    private IEnumerator Launch()
    {
        direction = bubbleDirection.position - transform.position;
        direction.Normalize();
        yield return new WaitForSeconds(apparitionAnim.length);
        convertable = true;
        animator.SetBool("Flying", true);
        rb.velocity = direction * bubbleSpeed;
        transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, direction));
    }

    void FixedUpdate()
    {
        CheckCollision();
    }

    private void CheckCollision()
    {
        if(Physics2D.OverlapCircle(transform.position, bubbleSize, LayerMask.GetMask("Obstacle")))
        {
            if(isConverted)
            {
                Vector2 bossDirection = boss.transform.position - transform.position;
                bossDirection.Normalize();
                transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, bossDirection));
                rb.velocity = bossDirection * convertedSpeed;
            }
            else
            {
                Explode();
            }
        }

        if (Physics2D.OverlapCircle(transform.position, bubbleSize, LayerMask.GetMask("Boss")))
        {
            Explode();
            if(isConverted)
            {
                bossAnimator.SetTrigger("Hurt");
            }
        }
    }

    public void Explode()
    {
        Instantiate(isConverted ? colorExplosionPrefab : blackExplosionPrefab, transform.position, Quaternion.identity);
        if (isConverted)
            boss.AddRecoloration();
        else
            boss.LoseRecoloration();
        source.PlayOneShot(explosionSound);
        Destroy(gameObject);
    }

    public void Convert(Vector2 direction)
    {
        if (convertable)
        {
            rb.velocity = direction.normalized * convertedSpeed;
            transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, direction));
            isConverted = true;
            animator.SetBool("Converted", true);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, bubbleDirection.position);
    }
}
