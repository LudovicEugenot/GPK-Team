using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public int maxhealthPoint;
    public RectTransform firstHealthPointPos;
    public float distanceBetweenHp;
    public GameObject hpIconPrefab;
    public Sprite fullHp;
    public Sprite halfHp;
    public Color emptyHpColor;
    public Animator animator;

    [HideInInspector] public int currentHealth;
    private List<GameObject> hpIcons = new List<GameObject>();
    private HpState[] hpIconsState;
    private enum HpState { Full, Half, Empty };

    void Start()
    {
        currentHealth = maxhealthPoint * 2;
        hpIconsState = new HpState[maxhealthPoint];
        InitializeHealthBar();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        UpdateHealthBar();
        animator.SetTrigger("Damage");
        if(currentHealth <= 0)
        {
            Die();
        }
    }

    private void InitializeHealthBar()
    {
        for (int i = 0; i < maxhealthPoint; i++)
        {
            GameObject newIcon;
            hpIcons.Add(newIcon = Instantiate(hpIconPrefab, firstHealthPointPos));
            newIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(distanceBetweenHp * i, 0.0f);
        }
        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
        int hpRemaining = currentHealth;
        for (int i = 0; i < maxhealthPoint; i++)
        {
            if (hpRemaining >= 2)
            {
                hpIconsState[i] = HpState.Full;
                hpRemaining -= 2;
            }
            else if (hpRemaining == 1)
            {
                hpIconsState[i] = HpState.Half;
                hpRemaining--;
            }
            else if (hpRemaining <= 0)
            {
                hpIconsState[i] = HpState.Empty;
            }
        }

        for (int i = 0; i < hpIconsState.Length; i++)
        {
            Image icon = hpIcons[i].GetComponent<Image>();
            switch (hpIconsState[i])
            {
                case HpState.Full:
                    icon.sprite = fullHp;
                    icon.color = Color.white;
                    break;

                case HpState.Half:
                    icon.sprite = halfHp;
                    icon.color = Color.white;
                    break;

                case HpState.Empty:
                    icon.sprite = fullHp;
                    icon.color = emptyHpColor;
                    break;
            }
        }
    }

    public void Die()
    {
        Debug.Log("This is not victory");
    }
}
