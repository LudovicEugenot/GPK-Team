using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public int maxhealthPoint; //A Health point is 2 health
    public float beatInvulnerableTime;
    public bool playerOffBeated;
    public bool beatAndOffBeatAllowed;
    [Space]
    public float deathCinematicZoom;
    public float interactMaxDistance;
    public float timeBeforeDancing;
    [Space]
    public GameObject gameOverPanel;
    public GameObject interactionIndicatorO;
    public RectTransform firstHealthPointPos;
    public float distanceBetweenHp;
    public GameObject hpIconPrefab;
    public Sprite fullHp;
    public Sprite halfHp;
    public Sprite emptyHp;
    public Animator animator;
    public GameObject healParticle;
    [Header("Sounds")]
    public AudioClip[] damageSounds;

    [HideInInspector] public int currentHealth;
    [HideInInspector] public int currentPower;
    [HideInInspector] public bool ownSpeaker;
    [HideInInspector] public bool isInControl;
    private List<GameObject> hpIcons = new List<GameObject>();
    private HpState[] hpIconsState;
    private enum HpState { Full, Half, Empty };
    [HideInInspector] public int heartContainerOwned;
    private bool keyPressed;
    private float invulTimeRemaining;
    private float remainingTimeBeforeDancing;
    public AnimSynchronizer animSynchronizer;
    private int interactionPossible;

    void Start()
    {
        isInControl = true;
        remainingTimeBeforeDancing = timeBeforeDancing;
    }

    private void Update()
    {
        keyPressed = Input.anyKeyDown;
        if(invulTimeRemaining > 0)
        {
            invulTimeRemaining -= Time.deltaTime;
        }

        if(remainingTimeBeforeDancing > 0)
        {
            remainingTimeBeforeDancing -= Time.deltaTime;
        }
        else if(remainingTimeBeforeDancing != -1)
        {
            remainingTimeBeforeDancing = -1;
            animSynchronizer.Synchronize();
            animator.SetTrigger("Dance");
        }
    }

    public void TakeDamage(int damage)
    {
        if(invulTimeRemaining <=  0)
        {
            currentHealth -= damage;
            UpdateHealthBar();
            animator.SetTrigger("Damage");
            invulTimeRemaining = beatInvulnerableTime * GameManager.Instance.Beat.BeatTime;
            if (currentHealth <= 0)
            {
                StartCoroutine(Die());
            }

            GameManager.playerSource.PlayOneShot(damageSounds[Random.Range(0, damageSounds.Length)]);
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
        Instantiate(healParticle, transform.position, Quaternion.identity);

        UpdateHealthBar();
    }

    public void InitializeHealthBar()
    {
        foreach(GameObject hpIcon in hpIcons)
        {
            Destroy(hpIcon);
        }
        hpIcons.Clear();
        hpIconsState = new HpState[maxhealthPoint];
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
                    break;

                case HpState.Half:
                    icon.sprite = halfHp;
                    break;

                case HpState.Empty:
                    icon.sprite = emptyHp;
                    break;
            }
        }
    }

    public void ObtainHeartContainer()
    {
        heartContainerOwned++;
        if(heartContainerOwned >= 2)
        {
            maxhealthPoint++;
        }
    }

    public IEnumerator Die()
    {
        GameManager.Instance.PauseEnemyBehaviour();
        StartCoroutine(GameManager.Instance.cameraHandler.StartCinematicLook(transform.parent.position, deathCinematicZoom, true));
        GameManager.playerAnimator.SetTrigger("Die");
        gameOverPanel.SetActive(true);
        yield return new WaitForSeconds(2);
        while(!keyPressed)
        {
            yield return new WaitForEndOfFrame();
        }
        StartCoroutine(TransitionManager.Instance.Respawn());
    }

    public void ResetIdleTime()
    {
        remainingTimeBeforeDancing = timeBeforeDancing;
    }

    /// <summary>
    /// Ajouter IsMouseNear()
    /// </summary>
    /// <returns></returns>
    public static bool CanInteract()
    {
        if (!GameManager.Instance.blink.IsSelecting() && ZoneHandler.Instance.AllEnemiesConverted())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool IsMouseNearPlayer()
    {
        if (Vector2.Distance(GameManager.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition), GameManager.Instance.player.transform.position) < GameManager.Instance.playerManager.interactMaxDistance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void DisplayIndicator()
    {
        GameManager.Instance.playerManager.interactionPossible++;
    }

    private void LateUpdate()
    {
        if(interactionPossible > 0)
        {
            interactionIndicatorO.SetActive(true);
        }
        else
        {
            interactionIndicatorO.SetActive(false);
        }
        interactionPossible = 0;
    }

}
