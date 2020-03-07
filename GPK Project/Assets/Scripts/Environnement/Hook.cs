using System.Collections;
using UnityEngine;

public abstract class Hook : MonoBehaviour
{
    #region Initialization
    [Header("Temporary references")]
    public BeatManager beatManager;
    public Blink blink;
    [Header("General Hook Options")]
    public bool isSecureHook;
    public Color blinkableColor;
    public Color selectedColor;
    public Color unselectableColor;

    [HideInInspector] public bool selected;
    [HideInInspector] public bool blinkable;
    [HideInInspector] public SpriteRenderer sprite;

    #endregion


    public void HandlerStart()
    {
        sprite = GetComponent<SpriteRenderer>();

        selected = false;
        blinkable = true;
    }


    public void HandlerUpdate()
    {
        StateUpdate();
    }

    public abstract void StateUpdate();

    public abstract IEnumerator BlinkReaction();
}
