using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Hook : MonoBehaviour
{
    #region Initialization
    [Header("General Hook Options")]
    public bool isSecureHook;
    public Color blinkableColor;
    public Color selectedColor;
    public Color unselectableColor;

    public bool agressiveHook;
    public float agressionRange;
    public GameObject rangeVisualO; // à remplacer par le mask de recoloration
    public float agressionTime;

    [HideInInspector] public bool selected;
    [HideInInspector] public bool blinkable;
    [HideInInspector] public SpriteRenderer sprite;

    private ContactFilter2D enemiFilter = new ContactFilter2D();

    #endregion


    public void HandlerStart()
    {
        sprite = GetComponent<SpriteRenderer>();

        selected = false;
        blinkable = true;

        enemiFilter.useTriggers = true;
        enemiFilter.SetLayerMask(LayerMask.GetMask("Enemi"));
    }


    public void HandlerUpdate()
    {
        StateUpdate();
    }

    public abstract void StateUpdate();

    public IEnumerator BlinkReaction()
    {
        StartCoroutine(BlinkSpecificReaction());

        rangeVisualO.SetActive(true);
        List<Collider2D> colliders = new List<Collider2D>();
        Physics2D.OverlapCircle(transform.position, agressionRange, enemiFilter, colliders);
        if(colliders.Count > 0)
        {
            foreach(Collider2D collider in colliders)
            {
                EnemyBase enemy = collider.transform.parent.GetChild(0).GetComponent<EnemyBase>();
                enemy.TakeDamage();
                Debug.Log("Heloooooo my name is : " + enemy.gameObject.name);
            }
        }

        yield return new WaitForSeconds(agressionTime);
        rangeVisualO.SetActive(false);
    }

    public abstract IEnumerator BlinkSpecificReaction();
}
