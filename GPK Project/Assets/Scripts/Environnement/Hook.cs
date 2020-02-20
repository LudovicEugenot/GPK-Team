using System.Collections;
using UnityEngine;
/*
public enum HookState
{
    Selected,
    Unselected
}
*/
public class Hook : MonoBehaviour
{
    #region Initialization
    public bool selected;
    private SpriteRenderer sprite;

    public Color blue;
    public Color red;
    #endregion


    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
    }


    void Update()
    {
        sprite.color = selected ? red : blue;
    }

}
