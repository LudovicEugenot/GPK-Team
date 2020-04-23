﻿using System.Collections;
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
    public GameObject[] musicianVisualO;

    [HideInInspector] public int currentHealth;
    [HideInInspector] public int currentPower;
    private List<GameObject> hpIcons = new List<GameObject>();
    private HpState[] hpIconsState;
    private enum HpState { Full, Half, Empty };

    void Start()
    {
        currentHealth = maxhealthPoint * 2;
        hpIconsState = new HpState[maxhealthPoint];
        InitializeHealthBar();
        UseMusicians();
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

    public void Heal(int life)
    {
        if(currentHealth + life <= maxhealthPoint * 2)
        {
            currentHealth += life;
        }
        else
        {
            currentHealth = maxhealthPoint * 2;
        }

        UpdateHealthBar();
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

    public void AddMusician()
    {
        if(currentPower < 3)
        {
            currentPower++;
        }

        musicianVisualO[currentPower - 1].SetActive(true);
    }

    public void UseMusicians()
    {
        currentPower = 0;
        foreach(GameObject musician in musicianVisualO)
        {
            musician.SetActive(false);
        }
    }
}
