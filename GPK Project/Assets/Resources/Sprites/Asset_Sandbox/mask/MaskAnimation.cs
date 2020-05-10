using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskAnimation : MonoBehaviour
{
    Animator animator;
    //bool IsExpend;
    private SpriteMask mask;
    private SpriteRenderer targetRenderer;


    void Start()
    {
        targetRenderer = GetComponent<SpriteRenderer>();
        mask = GetComponent<SpriteMask>();
        animator = gameObject.GetComponent<Animator>();
        //IsExpend = false;

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            //IsExpend = true;
            StartCoroutine("Blink");
        }
    }

    void LateUpdate()
    {
        if (mask.sprite != targetRenderer.sprite)
        {
            mask.sprite = targetRenderer.sprite;
        }
    }


    IEnumerator Blink()
    {
        animator.SetBool("IsExpend", true);
        yield return new WaitForSeconds(1.25f);
        animator.SetBool("IsExpend", false);
    }
}
