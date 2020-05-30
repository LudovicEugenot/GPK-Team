using System.Collections;
using UnityEngine;

public class Blink : MonoBehaviour
{
    #region Initialization
    [Header("Blink settings")]
    public BlinkRange[] blinkRangeProgression;
    public int allowedConsecutiveOnbeatMiss;
    public int comboMalus;
    public Hook startHook;
    public bool unreachableHookNoMove;
    public bool holdToBlink;
    public float selectingTimeOffset;
    [Space]
    public LineRenderer blinkTrajectoryPreviewLine;
    public GameObject blinkTrajectoryStartPreviewO;
    public GameObject blinkTrajectoryEndPreviewO;
    public GameObject blinkTargetO;
    public GameObject blinkInvalidTargetO;
    [Space]
    public float rangeCenterLerpSpeed;

    [Header("Prefabs")]
    public GameObject timingEffectPrefab;
    public GameObject missEffectPrefab;
    public GameObject overActionEffectPrefab;
    public GameObject blinkDisparition;

    [Space]
    public GameObject blinkTrailStartPrefab;
    public GameObject blinkTrailEndPrefab;
    public GameObject blinkCrossHurtFx;
    public float trailStartOffset;
    public float trailEndOffset;

    [Space]
    public bool rotatePoints;
    public bool ignoreObstacles;
    public int rangePointNumber;
    public float rangeBeatAmplitude;
    public GameObject rangePointPrefab;
    public Transform rangePointsParent;
    public float originTimingRadiusMultiplier;
    [Space]
    public LineRenderer rangeLine;
    public LineRenderer timingLine;
    public Animator animator;
    [Header("Sounds")]
    public AudioClip transitionBlinkSound;
    public AudioClip onBeatSound;
    public AudioClip missBeatSound;

    [System.Serializable]
    public class BlinkRange
    {
        public float range;
        public Sprite pointDisplay;
    }

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
    private SpriteRenderer[] rangePoints;
    private Vector3[] circlePointPos;
    private float currentTimingRadius;
    private Vector3[] timingLinePointPos;

    private float lastSelectionTime;
    #endregion
    void Start()
    {
        currentTimedCombo = 0;
        currentRange = blinkRangeProgression[0].range;
        transform.parent.position = startHook.transform.position;
        lastSecureHook = startHook;
        playerManager = GetComponent<PlayerManager>();
        rangeCenter = transform.parent.position;
        rangePoints = new SpriteRenderer[rangePointNumber];
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
            blinkTrajectoryEndPreviewO.SetActive(false);
        }


        if (holdToBlink ? Input.GetButton("Blink") : (Input.GetButtonDown("Blink")) && selectedHook != null && !GameManager.Instance.paused && GameManager.Instance.playerManager.isInControl)
        {
            if (GameManager.Instance.Beat.CanAct())
            {
                BlinkMove(blinkDestination);
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
            rangePoints[i] = Instantiate(rangePointPrefab, transform.position, Quaternion.identity, rangePointsParent).GetComponent<SpriteRenderer>();
        }
        circlePointPos = new Vector3[rangePointNumber];
        rangeLine.enabled = false;

        timingLinePointPos = new Vector3[rangePointNumber + 1];
        timingLine.enabled = true;
    }

    private void DrawHookRange(float radius, Vector2 center)
    {
        rangeCenter = Vector2.Lerp(rangeCenter, center, rangeCenterLerpSpeed * Time.deltaTime);
        currentRadius += (radius - currentRadius) * rangeCenterLerpSpeed * Time.deltaTime;
        currentTimingRadius = Mathf.Lerp(currentRadius * originTimingRadiusMultiplier, currentRadius, BeatManager.Instance.currentBeatProgression);
        if(BeatManager.Instance.onBeatSingleFrame)
        {
            currentRadius += rangeBeatAmplitude;
        }

        for (int i = 0; i < rangePointNumber; i++)
        {
            Vector2 pointDirection = new Vector2(Mathf.Cos(((2 * Mathf.PI) / (rangePointNumber)) * i), Mathf.Sin(((2 * Mathf.PI) / (rangePointNumber)) * i));

            circlePointPos[i] = (pointDirection * currentRadius) + rangeCenter;
            timingLinePointPos[i] = (pointDirection * currentTimingRadius) + rangeCenter;

            if (!ignoreObstacles)
            {
                RaycastHit2D hit = Physics2D.Raycast(rangeCenter, pointDirection, currentRadius, LayerMask.GetMask("Obstacle"));

                if (hit)
                {
                    circlePointPos[i] = hit.point;
                }
            }

            rangePoints[i].transform.position = circlePointPos[i];
            rangePoints[i].sprite = blinkRangeProgression[currentTimedCombo < blinkRangeProgression.Length ? currentTimedCombo : blinkRangeProgression.Length - 1].pointDisplay;
            if(rotatePoints)
            {
                rangePoints[i].transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, pointDirection));
            }
        }
        rangeLine.positionCount = rangePointNumber;
        rangeLine.SetPositions(circlePointPos);

        timingLine.positionCount = rangePointNumber + 1;
        timingLinePointPos[rangePointNumber] = timingLinePointPos[0];
        timingLine.SetPositions(timingLinePointPos);
    }

    private void HookSelection()
    {
        worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        float mouseDirectionAngle = Vector2.SignedAngle(Vector2.right, worldMousePos - (Vector2)transform.position);
        animator.SetFloat("BlinkDirection",  mouseDirectionAngle >= 0 ? mouseDirectionAngle : 360 + mouseDirectionAngle);

        Hook hoveredHook = null;
        Collider2D[] hookHover = Physics2D.OverlapPointAll(worldMousePos, LayerMask.GetMask("Hook","Speaker"));
        float minDistanceToHook = 10000f;
        for (int i = 0; i < hookHover.Length; i++)
        {
            float distanceToHook;
            distanceToHook = Vector2.Distance(hookHover[i].transform.position, worldMousePos);
            if (distanceToHook < minDistanceToHook && hookHover[i].GetComponent<Hook>().selectable && (Vector2)transform.position != (Vector2)hookHover[i].transform.position)
            {
                minDistanceToHook = distanceToHook;
                if (hoveredHook != null)
                {
                    hoveredHook.selected = false;
                }
                hoveredHook = hookHover[i].GetComponent<Hook>();
            }
        }

        if (hoveredHook != null && (hookHover.Length == 0 || !hoveredHook.selectable))
        {
            hoveredHook.selected = false;
            hoveredHook = null;
            selectedHook = null;
        }

        currentRange = blinkRangeProgression[currentTimedCombo < blinkRangeProgression.Length ? currentTimedCombo : blinkRangeProgression.Length - 1].range;


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
            blinkTrajectoryEndPreviewO.SetActive(false);
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
            blinkTrajectoryStartPreviewO.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, - blinkDirection));
            blinkTrajectoryEndPreviewO.SetActive(true);
            blinkTrajectoryEndPreviewO.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, blinkDirection));

            blinkInvalidTargetO.SetActive(false);
        }
        else
        {
            Vector2 obstacleHitPos = Vector2.ClampMagnitude(blinkHitObject.point - blinkOrigin, blinkHitObject.distance - .4f) + blinkOrigin;

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

    public void BlinkMove(Vector2 destination)
    {
        if(destination != (Vector2)transform.position)
        {
            Vector2 blinkDirection = destination - (Vector2)transform.parent.position;

            Instantiate(blinkTrailStartPrefab, (Vector2)transform.parent.position + blinkDirection.normalized * trailStartOffset, Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, blinkDirection)));
            Instantiate(blinkDisparition, transform.position, Quaternion.identity);
            GameManager.Instance.playerManager.ResetIdleTime();

            RaycastHit2D blinkCrossHurtHit = Physics2D.Raycast(transform.parent.position, blinkDirection, blinkDirection.magnitude, LayerMask.GetMask("CrossHurt"));
            if(blinkCrossHurtHit)
            {
                Range_Enemy rangeEnemy = blinkCrossHurtHit.collider.transform.parent.GetComponentInChildren<Range_Enemy>();
                playerManager.TakeDamage(rangeEnemy.barrierDamage);
                Instantiate(blinkCrossHurtFx, blinkCrossHurtHit.point, Quaternion.identity);
            }

            transform.parent.position = destination;

            if (selectedHook != null)
            {
                if(selectedHook.isSecureHook)
                {
                    lastSecureHook = selectedHook;
                }
            }
            else
            {
                selectedHook = startHook;
            }


            if (GameManager.Instance.Beat.OnBeat(playerManager.playerOffBeated ,true, "Blink") && blinkReachDestination)
            {
                StartCoroutine(selectedHook.BlinkReaction(true));

                currentTimedCombo++;
                consecutiveMiss = 0;
                Instantiate(timingEffectPrefab, transform.parent.position, Quaternion.identity);

                GameManager.playerSource.PlayOneShot(onBeatSound);
            }
            else
            {
                StartCoroutine(selectedHook.BlinkReaction(false));
                Instantiate(missEffectPrefab, transform.parent.position + (Vector3)(Vector2.up * 0.5f), Quaternion.Euler(90, 0, 0));

                FailCombo();

                GameManager.playerSource.PlayOneShot(missBeatSound);
            }
            Instantiate(blinkTrailEndPrefab, (Vector2)transform.parent.position - blinkDirection.normalized * trailEndOffset, Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, blinkDirection)));
            animator.SetTrigger("Blink");
            currentHook = selectedHook;
            selectedHook.selected = false;
            selectedHook = null;
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
