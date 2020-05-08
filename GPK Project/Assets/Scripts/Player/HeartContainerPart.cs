using System.Collections;
using UnityEngine;

public class HeartContainerPart : MonoBehaviour
{
    public Hook nearbyHook;
    public float cinematicZoom;
    public float heartDestinationVerticalOffset;
    public float heartLerpSpeed;
    public float timeBeforeAnimStart;
    public float timeBeforeHeartVanish;
    public float timeBeforeHeal;

    private Vector2 targetPos;
    private bool isPickedUp;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    [HideInInspector] public bool isObtained;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        UpdatePlayerTrigger();
        UpdateObtention();
    }

    private void UpdateObtention()
    {
        if(isObtained)
        {
            spriteRenderer.enabled = false;
        }
    }

    private void UpdatePlayerTrigger()
    {
        if(Input.GetButtonDown("Blink") && GameManager.Instance.blink.currentHook == nearbyHook && !GameManager.Instance.blink.IsSelecting() && !isObtained)
        {
            StartCoroutine(PickUpHeart());
        }
    }

    private IEnumerator PickUpHeart()
    {
        StartCoroutine(GameManager.Instance.cameraHandler.StartCinematicLook(GameManager.Instance.player.transform.position, cinematicZoom, true));
        GameManager.Instance.playerManager.isInControl = false;
        targetPos = (Vector2)GameManager.Instance.player.transform.position + new Vector2(0, heartDestinationVerticalOffset);
        yield return new WaitForSeconds(timeBeforeAnimStart);
        while(Vector2.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector2.Lerp(transform.position, targetPos, heartLerpSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        transform.position = targetPos;
        yield return new WaitForSeconds(timeBeforeHeartVanish);
        animator.SetTrigger("Pick");
        while (Vector2.Distance(transform.position, GameManager.Instance.player.transform.position) > 0.05f)
        {
            transform.position = Vector2.Lerp(transform.position, GameManager.Instance.player.transform.position, heartLerpSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        isObtained = true;
        GameManager.Instance.playerManager.ObtainHeartContainer();
        GameManager.Instance.playerManager.InitializeHealthBar();
        yield return new WaitForSeconds(timeBeforeHeal);
        GameManager.Instance.playerManager.Heal(500);
        GameManager.Instance.cameraHandler.StartStopCinematicLook();
        GameManager.Instance.playerManager.isInControl = true;
    }
}
