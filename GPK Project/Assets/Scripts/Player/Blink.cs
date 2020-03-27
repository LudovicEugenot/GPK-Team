using System.Collections;
using UnityEngine;

public class Blink : MonoBehaviour
{
    #region Initialization
    public float[] blinkRangeProgression;
    public Hook startHook;

    public GameObject timingEffectPrefab;
    public GameObject blinkDisparition;

    [Space]
    public GameObject blinkTrailStartPrefab;
    public GameObject blinkTrailEndPrefab;
    public float trailStartOffset;
    public float trailEndOffset;

    private Hook lastSecureHook;
    [HideInInspector] public Hook currentHook;

    [HideInInspector] public float currentRange;
    private int currentTimedCombo;
    private Vector2 worldMousePos;
    private Hook selectedHook = null;

    private bool blinkReachDestination;
    private Vector2 blinkOrigin;
    private Vector2 blinkDestination;
    private PlayerManager playerManager;

    private LineRenderer lineCircle;
    public Animator animator;
    #endregion
    void Start()
    {
        currentTimedCombo = 0;
        lineCircle = GetComponent<LineRenderer>();
        currentRange = blinkRangeProgression[0];
        transform.parent.position = startHook.transform.position;
        lastSecureHook = startHook;
        playerManager = GetComponent<PlayerManager>();
    }


    void Update()
    {
        DrawHookRange(currentRange, transform.position);

        HookSelection();

        if (Input.GetButtonDown("Blink") && selectedHook != null)
        {
            BlinkTest();
            BlinkMove();
        }
    }

    private void DrawHookRange(float radius, Vector2 center)
    {
        Vector3[] circleLinePos = new Vector3[50];
        for (int i = 0; i < circleLinePos.Length; i++)
        {
            circleLinePos[i] = new Vector2(Mathf.Cos(((2 * Mathf.PI) / 50) * i), Mathf.Sin(((2 * Mathf.PI) / 50) * i));
            circleLinePos[i] *= radius;
            circleLinePos[i] += (Vector3)center;
        }
        lineCircle.SetPositions(circleLinePos);
    }

    private void HookSelection()
    {
        worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Collider2D[] hookHover = Physics2D.OverlapPointAll(worldMousePos, LayerMask.GetMask("Hook"));
        float minDistanceToHook = 10000f;
        for (int i = 0; i < hookHover.Length; i++)
        {
            float distanceToHook;
            distanceToHook = Vector2.Distance(hookHover[i].transform.position, worldMousePos);
            if (distanceToHook < minDistanceToHook && hookHover[i].GetComponent<Hook>().blinkable && (Vector2)transform.position != (Vector2)hookHover[i].transform.position)
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

        if (selectedHook != null && (hookHover.Length == 0 || !selectedHook.blinkable))
        {
            selectedHook.selected = false;
            selectedHook = null;
        }

        currentRange = blinkRangeProgression[currentTimedCombo < blinkRangeProgression.Length ? currentTimedCombo : blinkRangeProgression.Length - 1];
    }

    private void BlinkTest()
    {
        blinkReachDestination = false;
        blinkOrigin = transform.position;
        RaycastHit2D blinkHitObject = Physics2D.Raycast
            (
                transform.position,
                selectedHook.transform.position - transform.position,
                Vector2.Distance(selectedHook.transform.position, transform.position),
                LayerMask.GetMask("Obstacle")
            );
        if (!blinkHitObject)
        {
            blinkDestination = selectedHook.transform.position;
            blinkReachDestination = true;
            if(selectedHook.isSecureHook)
            {
                lastSecureHook = selectedHook;
            }
            StartCoroutine(selectedHook.BlinkReaction());
        }
        else
        {
            blinkDestination = Vector2.ClampMagnitude(blinkHitObject.point - blinkOrigin, blinkHitObject.distance - .4f) + blinkOrigin; // 0.4f = half of the player's Width, à changer une fois qu'on prend en compte le sprite renderer
        }
    }

    private void BlinkMove()
    {
        Vector2 direction = blinkDestination - (Vector2)transform.parent.position;
        direction.Normalize();
        Instantiate(blinkTrailStartPrefab, (Vector2)transform.parent.position + direction * trailStartOffset, Quaternion.Euler(0,0,Vector2.SignedAngle(Vector2.right, direction)));
        Instantiate(blinkDisparition, transform.position, Quaternion.identity);
        transform.parent.position = blinkDestination;
        if (GameManager.Instance.Beat.OnBeat() && blinkReachDestination)
        {
            currentTimedCombo++;
            Instantiate(timingEffectPrefab, transform.parent.position, Quaternion.identity);
        }
        else
        {
            currentTimedCombo = 0;
        }
        Instantiate(blinkTrailEndPrefab, (Vector2)transform.parent.position - direction * trailEndOffset, Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, direction)));
        animator.SetTrigger("Blink");
        currentHook = selectedHook;
        selectedHook.selected = false;
        selectedHook = null;
    }

    public IEnumerator RespawnPlayer()
    {
        currentTimedCombo = 0;
        playerManager.TakeDamage(1);
        yield return new WaitForSeconds(0.2f);
        transform.parent.position = lastSecureHook.transform.position;
        currentHook = lastSecureHook;
    }
}
