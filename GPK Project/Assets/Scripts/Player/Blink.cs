using System.Collections;
using UnityEngine;

public class Blink : MonoBehaviour
{
    #region Initialization
    [Header("Blink settings")]
    public float[] blinkRangeProgression;
    public int allowedConsecutiveOnbeatMiss;
    public int comboMalus;
    public Hook startHook;
    public bool unreachableHookNoMove;
    public bool holdToBlink;
    public float selectingTimeOffset;
    [Space]
    public LineRenderer blinkTrajectoryPreviewLine;
    public GameObject blinkTrajectoryStartPreviewO;
    public GameObject blinkTargetO;
    public GameObject blinkInvalidTargetO;
    [Space]
    public float rangeCenterLerpSpeed;

    [Header("Prefabs")]
    public GameObject timingEffectPrefab;
    public GameObject overActionEffectPrefab;
    public GameObject blinkDisparition;

    [Space]
    public GameObject blinkTrailStartPrefab;
    public GameObject blinkTrailEndPrefab;
    public float trailStartOffset;
    public float trailEndOffset;

    [Space]
    public bool rotatePoints;
    public bool ignoreObstacles;
    public int rangePointNumber;
    public float rangeBeatAmplitude;
    public GameObject rangePointPrefab;
    public Transform rangePointsParent;

    private Hook lastSecureHook;
    [HideInInspector] public Hook currentHook;

    [HideInInspector] public float currentRange;
    private int currentTimedCombo;
    private int consecutiveMiss;
    private Vector2 worldMousePos;
    private Hook selectedHook = null;

    private bool blinkReachDestination;
    private Vector2 blinkOrigin;
    private Vector2 blinkDestination;
    private PlayerManager playerManager;

    private Vector2 rangeCenter;
    private float currentRadius;
    private GameObject[] rangePoints;
    public LineRenderer rangeLine;

    private float lastSelectionTime;
    public Animator animator;
    #endregion
    void Start()
    {
        currentTimedCombo = 0;
        currentRange = blinkRangeProgression[0];
        transform.parent.position = startHook.transform.position;
        lastSecureHook = startHook;
        playerManager = GetComponent<PlayerManager>();
        rangeCenter = transform.parent.position;
        rangePoints = new GameObject[rangePointNumber];
        CreateHookRange();
    }


    void Update()
    {
        DrawHookRange(currentRange, transform.position);
        if (!GameManager.Instance.paused && GameManager.Instance.playerManager.isInControl)
        {
            HookSelection();
        }
        else
        {
            blinkTrajectoryPreviewLine.enabled = false;
            blinkTargetO.SetActive(false);
            blinkInvalidTargetO.SetActive(false);
            blinkTrajectoryStartPreviewO.SetActive(false);
        }


        if (holdToBlink ? Input.GetButton("Blink") : (Input.GetButtonDown("Blink")) && selectedHook != null && !GameManager.Instance.paused && GameManager.Instance.playerManager.isInControl)
        {
            if (GameManager.Instance.Beat.CanAct())
            {
                BlinkMove();
                GameManager.Instance.attack.HasBlinked();
            }
            else
            {
                Instantiate(overActionEffectPrefab, transform.parent.position, Quaternion.identity);
                FailCombo();
            }
        }

        UpdateSelecting();
    }

    private void CreateHookRange()
    {
        for (int i = 0; i < rangePointNumber; i++)
        {
            rangePoints[i] = Instantiate(rangePointPrefab, transform.position, Quaternion.identity, rangePointsParent);
        }
        rangeLine.enabled = false;
    }

    private void DrawHookRange(float radius, Vector2 center)
    {
        rangeCenter = Vector2.Lerp(rangeCenter, center, rangeCenterLerpSpeed * Time.deltaTime);
        currentRadius += (radius - currentRadius) * rangeCenterLerpSpeed * Time.deltaTime;
        if(BeatManager.Instance.onBeatSingleFrame)
        {
            currentRadius += rangeBeatAmplitude;
        }

        Vector3[] circlePointPos = new Vector3[rangePointNumber];
        for (int i = 0; i < rangePointNumber; i++)
        {
            circlePointPos[i] = new Vector2(Mathf.Cos(((2 * Mathf.PI) / (rangePointNumber)) * i), Mathf.Sin(((2 * Mathf.PI) / (rangePointNumber)) * i));
            circlePointPos[i] *= currentRadius;
            circlePointPos[i] += (Vector3)rangeCenter;
            Vector2 pointVector = (Vector2)circlePointPos[i] - rangeCenter;
            if(!ignoreObstacles)
            {
                RaycastHit2D hit = Physics2D.Raycast(rangeCenter, pointVector.normalized, pointVector.magnitude, LayerMask.GetMask("Obstacle"));
                if (hit)
                {
                    circlePointPos[i] = hit.point;
                }
            }

            rangePoints[i].transform.position = circlePointPos[i];
            if(rotatePoints)
            {
                rangePoints[i].transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, pointVector));
            }

            rangeLine.positionCount = rangePointNumber;
            rangeLine.SetPositions(circlePointPos);
        }
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
            selectedHook = null;
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
            blinkTrajectoryStartPreviewO.SetActive(false);
            blinkDestination = transform.position;
            selectedHook = null;
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

            Vector2 blinkDirection = blinkDestination - (Vector2)transform.parent.position;
            blinkDirection.Normalize();

            //blinkTrajectoryPreviewLine.enabled = true;

            Vector3[] previewPositions = new Vector3[2];
            previewPositions[0] = transform.parent.position;
            previewPositions[1] = selectedHook.transform.position;
            blinkTrajectoryPreviewLine.SetPositions(previewPositions);

            blinkTargetO.SetActive(true);
            blinkTargetO.transform.position = selectedHook.transform.position;
            blinkTargetO.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, blinkDirection));
            blinkTrajectoryStartPreviewO.SetActive(true);
            blinkTrajectoryStartPreviewO.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, blinkDirection));

            blinkInvalidTargetO.SetActive(false);
        }
        else
        {
            Vector2 obstacleHitPos = Vector2.ClampMagnitude(blinkHitObject.point - blinkOrigin, blinkHitObject.distance - .4f) + blinkOrigin; // 0.4f = half of the player's Width, à changer une fois qu'on prend en compte le sprite renderer

            //blinkTrajectoryPreviewLine.enabled = true;

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
        if(blinkDestination != (Vector2)transform.position)
        {
            Vector2 direction = blinkDestination - (Vector2)transform.parent.position;
            direction.Normalize();
            Instantiate(blinkTrailStartPrefab, (Vector2)transform.parent.position + direction * trailStartOffset, Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, direction)));
            Instantiate(blinkDisparition, transform.position, Quaternion.identity);
            transform.parent.position = blinkDestination;

            if (selectedHook.isSecureHook)
            {
                lastSecureHook = selectedHook;
            }


            if (GameManager.Instance.Beat.OnBeat(GameManager.Instance.playerManager.playerOffBeated ,true) && blinkReachDestination)
            {
                StartCoroutine(selectedHook.BlinkReaction(true));

                currentTimedCombo++;
                consecutiveMiss = 0;
                Instantiate(timingEffectPrefab, transform.parent.position, Quaternion.identity);
            }
            else
            {
                StartCoroutine(selectedHook.BlinkReaction(false));

                FailCombo();
            }
            Instantiate(blinkTrailEndPrefab, (Vector2)transform.parent.position - direction * trailEndOffset, Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, direction)));
            animator.SetTrigger("Blink");
            currentHook = selectedHook;
            selectedHook.selected = false;
            selectedHook = null;
        }
        else
        {
            //No move effect
        }
    }

    public IEnumerator RespawnPlayer()
    {
        FailCombo();
        playerManager.TakeDamage(1);
        yield return new WaitForSeconds(0.2f);
        transform.parent.position = lastSecureHook.transform.position;
        currentHook = lastSecureHook;
    }

    private void UpdateSelecting()
    {
        if(selectedHook != null)
        {
            lastSelectionTime = Time.time;
        }
    }

    public bool IsSelecting()
    {
        if(Time.time - lastSelectionTime < selectingTimeOffset || selectedHook != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void FailCombo()
    {
        if(consecutiveMiss < allowedConsecutiveOnbeatMiss)
        {
            consecutiveMiss++;
            if(currentTimedCombo > blinkRangeProgression.Length)
            {
                currentTimedCombo = blinkRangeProgression.Length;
            }
        }
        else
        {
            currentTimedCombo -= comboMalus;
            if(currentTimedCombo < 0)
            {
                currentTimedCombo = 0;
            }
        }
    }
}
