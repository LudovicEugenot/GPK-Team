using System.Collections;
using UnityEngine;

public class Blink : MonoBehaviour
{
    #region Initialization
    [Header("Blink settings")]
    public float[] blinkRangeProgression;
    public Hook startHook;
    public bool unreachableHookNoMove;
    [Space]
    public LineRenderer blinkTrajectoryPreviewLine;
    public GameObject blinkTargetO;
    public GameObject blinkInvalidTargetO;

    [Header("Prefabs")]
    public GameObject timingEffectPrefab;
    public GameObject overActionEffectPrefab;
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
            if(GameManager.Instance.Beat.CanAct())
            {
                BlinkMove();
            }
            else
            {
                Instantiate(overActionEffectPrefab, transform.parent.position, Quaternion.identity);
                currentTimedCombo = 0;
            }
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
        Hook hoveredHook = null;
        Collider2D[] hookHover = Physics2D.OverlapPointAll(worldMousePos, LayerMask.GetMask("Hook"));
        float minDistanceToHook = 10000f;
        for (int i = 0; i < hookHover.Length; i++)
        {
            float distanceToHook;
            distanceToHook = Vector2.Distance(hookHover[i].transform.position, worldMousePos);
            if (distanceToHook < minDistanceToHook && hookHover[i].GetComponent<Hook>().blinkable && (Vector2)transform.position != (Vector2)hookHover[i].transform.position)
            {
                minDistanceToHook = distanceToHook;
                if (hoveredHook != null)
                {
                    hoveredHook.selected = false;
                }
                hoveredHook = hookHover[i].GetComponent<Hook>();
            }
        }

        if (hoveredHook != null && (hookHover.Length == 0 || !hoveredHook.blinkable))
        {
            hoveredHook.selected = false;
            hoveredHook = null;
        }

        currentRange = blinkRangeProgression[currentTimedCombo < blinkRangeProgression.Length ? currentTimedCombo : blinkRangeProgression.Length - 1];


        if(hoveredHook != null)
        {
            TrajectoryTest(hoveredHook);
        }
        else
        {
            blinkTrajectoryPreviewLine.enabled = false;
            blinkTargetO.SetActive(false);
            blinkInvalidTargetO.SetActive(false);
        }
    }

    private void TrajectoryTest(Hook hoveredHook)
    {
        blinkReachDestination = false;
        blinkOrigin = transform.position;
        RaycastHit2D blinkHitObject = Physics2D.Raycast
            (
                transform.position,
                hoveredHook.transform.position - transform.position,
                Vector2.Distance(hoveredHook.transform.position, transform.position),
                LayerMask.GetMask("Obstacle")
            );

        if (!blinkHitObject)
        {
            selectedHook = hoveredHook;
            blinkDestination = selectedHook.transform.position;
            blinkReachDestination = true;

            blinkTrajectoryPreviewLine.enabled = true;

            Vector3[] previewPositions = new Vector3[2];
            previewPositions[0] = transform.parent.position;
            previewPositions[1] = selectedHook.transform.position;
            blinkTrajectoryPreviewLine.SetPositions(previewPositions);

            blinkTargetO.SetActive(true);
            blinkTargetO.transform.position = selectedHook.transform.position;
        }
        else
        {
            Vector2 obstacleHitPos = Vector2.ClampMagnitude(blinkHitObject.point - blinkOrigin, blinkHitObject.distance - .4f) + blinkOrigin; // 0.4f = half of the player's Width, à changer une fois qu'on prend en compte le sprite renderer

            blinkTrajectoryPreviewLine.enabled = true;

            Vector3[] previewPositions = new Vector3[2];
            previewPositions[0] = transform.parent.position;
            previewPositions[1] = obstacleHitPos;
            blinkTrajectoryPreviewLine.SetPositions(previewPositions);



            if (!unreachableHookNoMove)
            {
                blinkDestination = obstacleHitPos;

                blinkTargetO.SetActive(true);
                blinkTargetO.transform.position = obstacleHitPos;
            }
            else
            {
                blinkDestination = transform.parent.position;

                blinkInvalidTargetO.SetActive(true);
                blinkInvalidTargetO.transform.position = obstacleHitPos;
            }
        }
    }

    private void BlinkMove()
    {
        Vector2 direction = blinkDestination - (Vector2)transform.parent.position;
        direction.Normalize();
        Instantiate(blinkTrailStartPrefab, (Vector2)transform.parent.position + direction * trailStartOffset, Quaternion.Euler(0,0,Vector2.SignedAngle(Vector2.right, direction)));
        Instantiate(blinkDisparition, transform.position, Quaternion.identity);
        transform.parent.position = blinkDestination;

        if (selectedHook.isSecureHook)
        {
            lastSecureHook = selectedHook;
        }

        StartCoroutine(selectedHook.BlinkReaction());

        if (GameManager.Instance.Beat.OnBeat(true) && blinkReachDestination)
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
