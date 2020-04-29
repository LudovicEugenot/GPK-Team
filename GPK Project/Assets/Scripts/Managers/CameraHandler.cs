using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    [Header("General settings")]
    [Range(0.0f, 10f)] public float moveLerpSpeed = 5;
    public float baseOrthographicSize = 5.625f;
    public float cinematicOrthographicSize;
    public float sizeLerpSpeed = 5;
    public RectTransform cinematicBar1;
    public RectTransform cinematicBar2;
    public float barLerpSpeed;
    public float cinematicBarOffset;

    private Camera mainCamera;
    private float currentOrthographicSize;
    private bool cameraCentered;
    private Vector2 cameraFinalPos;
    private Vector2 cameraCenterPos;
    private Vector2 cinematicBar1InitialPos;
    private Vector2 cinematicBar2InitialPos;

    private void Start()
    {
        mainCamera = Camera.main;
        cameraCentered = true;
        currentOrthographicSize = baseOrthographicSize;
        cinematicBar1InitialPos = cinematicBar1.anchoredPosition;
        cinematicBar2InitialPos = cinematicBar2.anchoredPosition;
        cameraCenterPos = Vector2.zero;
    }

    void Update()
    {
        if(Input.GetButtonDown("Interact"))
        {
            //StartCoroutine(CinematicLook(Camera.main.ScreenToWorldPoint(Input.mousePosition), 3, cinematicOrthographicSize));
        }

        if(cameraCentered)
        {
            SetCameraDefaultPos();
        }

        MoveCamera(cameraFinalPos);
    }

    private void SetCameraDefaultPos()
    {
        cameraFinalPos = cameraCenterPos;
    }

    private void MoveCamera(Vector2 targetCameraPos)
    {
        if(Vector2.Distance(mainCamera.transform.position, targetCameraPos) > 0.01f)
        {
            Vector2 lerpPos = Vector2.Lerp(mainCamera.transform.position, targetCameraPos, moveLerpSpeed * Time.deltaTime);
            mainCamera.transform.position = new Vector3(lerpPos.x, lerpPos.y, -10.0f);
        }
        else
        {
            mainCamera.transform.position = new Vector3(targetCameraPos.x, targetCameraPos.y, -10.0f);
        }

        if (Mathf.Abs(mainCamera.orthographicSize - currentOrthographicSize) > 0.01f)
        {
            mainCamera.orthographicSize -= (mainCamera.orthographicSize - currentOrthographicSize) * sizeLerpSpeed * Time.deltaTime;
        }
        else
        {
            mainCamera.orthographicSize = currentOrthographicSize;
            if(mainCamera.orthographicSize == baseOrthographicSize)
            {
                GameManager.Instance.Beat.useCameraBeatShake = true;
            }
        }
    }

    public IEnumerator CinematicLook(Vector2 lookPosition, float lookingTime, float orthographicSize, bool useBars)
    {
        cameraCentered = false;
        cameraFinalPos = lookPosition;
        currentOrthographicSize = orthographicSize;
        GameManager.Instance.Beat.useCameraBeatShake = false;
        if(useBars)
        {
            while (Vector2.Distance(cinematicBar1.anchoredPosition, new Vector2(cinematicBar1InitialPos.x, cinematicBar1InitialPos.y - cinematicBarOffset)) > 0.01f)
            {
                cinematicBar1.anchoredPosition = Vector2.Lerp(cinematicBar1.anchoredPosition, new Vector2(cinematicBar1InitialPos.x, cinematicBar1InitialPos.y - cinematicBarOffset), barLerpSpeed);
                cinematicBar2.anchoredPosition = Vector2.Lerp(cinematicBar2.anchoredPosition, new Vector2(cinematicBar2InitialPos.x, cinematicBar2InitialPos.y + cinematicBarOffset), barLerpSpeed);

                yield return new WaitForEndOfFrame();
            }
        }

        yield return new WaitForSeconds(lookingTime);

        cameraCentered = true;
        cameraFinalPos = cameraCenterPos;
        currentOrthographicSize = baseOrthographicSize;

        while (Vector2.Distance(cinematicBar1.anchoredPosition, cinematicBar1InitialPos) > 0.01f)
        {
            cinematicBar1.anchoredPosition = Vector2.Lerp(cinematicBar1.anchoredPosition, cinematicBar1InitialPos, barLerpSpeed);
            cinematicBar2.anchoredPosition = Vector2.Lerp(cinematicBar2.anchoredPosition, cinematicBar2InitialPos, barLerpSpeed);

            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator StartCinematicLook(Vector2 lookPosition, float orthographicSize, bool useBars)
    {
        cameraCentered = false;
        cameraFinalPos = lookPosition;
        currentOrthographicSize = orthographicSize;
        GameManager.Instance.Beat.useCameraBeatShake = false;
        if (useBars)
        {
            while (Vector2.Distance(cinematicBar1.anchoredPosition, new Vector2(cinematicBar1InitialPos.x, cinematicBar1InitialPos.y - cinematicBarOffset)) > 0.01f)
            {
                cinematicBar1.anchoredPosition = Vector2.Lerp(cinematicBar1.anchoredPosition, new Vector2(cinematicBar1InitialPos.x, cinematicBar1InitialPos.y - cinematicBarOffset), barLerpSpeed);
                cinematicBar2.anchoredPosition = Vector2.Lerp(cinematicBar2.anchoredPosition, new Vector2(cinematicBar2InitialPos.x, cinematicBar2InitialPos.y + cinematicBarOffset), barLerpSpeed);

                yield return new WaitForEndOfFrame();
            }
        }
    }

    public IEnumerator StopCinematicLook()
    {
        cameraCentered = true;
        cameraFinalPos = cameraCenterPos;
        currentOrthographicSize = baseOrthographicSize;

        while (Vector2.Distance(cinematicBar1.anchoredPosition, cinematicBar1InitialPos) > 0.01f)
        {
            cinematicBar1.anchoredPosition = Vector2.Lerp(cinematicBar1.anchoredPosition, cinematicBar1InitialPos, barLerpSpeed);
            cinematicBar2.anchoredPosition = Vector2.Lerp(cinematicBar2.anchoredPosition, cinematicBar2InitialPos, barLerpSpeed);

            yield return new WaitForEndOfFrame();
        }
    }
}
