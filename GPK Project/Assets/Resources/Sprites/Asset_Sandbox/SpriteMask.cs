using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteMask : MonoBehaviour
{
    Animator animator;
    bool IsExpend;

    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        IsExpend = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
        IsExpend = true;
        StartCoroutine("Blink");
        }
    }

    IEnumerator Blink()
    {
        animator.SetBool("IsExpend", true);
        yield return new WaitForSeconds (1.25f);
        animator.SetBool("IsExpend", false);
    }
}
