using System.Collections;
using UnityEngine;

public class Blink : MonoBehaviour
{
    #region Initialization
    private Vector2 worldMousePos;
    private Hook selectedHook = null;

    #endregion


    void Start()
    {

    }


    void Update()
    {
        #region Hover mouse
        worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Collider2D[] hookHover = Physics2D.OverlapPointAll(worldMousePos, LayerMask.GetMask("Hook"));
        float minDistanceToHook = 10000f;
        for (int i = 0; i < hookHover.Length; i++)
        {
            float distanceToHook;
            distanceToHook = Vector2.Distance(hookHover[i].transform.position, worldMousePos);
            if (distanceToHook < minDistanceToHook)
            {
                minDistanceToHook = distanceToHook;
                if (selectedHook != null)
                {
                    selectedHook.selected = false;
                }
                selectedHook = hookHover[i].GetComponent<Hook>();
                selectedHook.selected = true;
            }
        }

        if (hookHover.Length == 0 && selectedHook != null)
        {
            selectedHook.selected = false;
            selectedHook = null;
        }
        #endregion

        if (Input.GetButtonDown("Blink") && selectedHook != null)
        {
            transform.position = selectedHook.transform.position;
        }
    }
}
