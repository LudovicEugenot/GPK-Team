using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FallTransitionManager : MonoBehaviour
{
    public float playerFallSpeed;
    public float timeBeforeNextZone;
    public float transitionLerpSpeed;
    public float maxMaskSize;
    public GameObject blackScreen;
    public GameObject blackScreenMask;
    public Transform maskOpenPos;
    public Transform maskClosePos;
    public Transform player;

    private int nextZone;

    void Start()
    {
        StartCoroutine(Transition());
        nextZone = TransitionManager.Instance.previousTransitionHook.connectedSceneBuildIndex;
    }

    void Update()
    {
        player.position += Vector3.down * playerFallSpeed * Time.deltaTime;
    }

    private void LoadNextZone()
    {
        SceneManager.LoadScene(nextZone);
    }

    private IEnumerator Transition()
    {
        TransitionManager.Instance.blackScreen.SetActive(false);
        blackScreen.SetActive(true);
        blackScreenMask.transform.position = maskOpenPos.position;
        float maskLerpProgression = 0;
        while (maskLerpProgression < 0.95f)
        {
            maskLerpProgression += (1 - maskLerpProgression) * transitionLerpSpeed * Time.deltaTime;
            blackScreenMask.transform.localScale = new Vector2(maskLerpProgression * maxMaskSize, maskLerpProgression * maxMaskSize);
            yield return new WaitForEndOfFrame();
        }
        blackScreen.SetActive(false);



        yield return new WaitForSeconds(timeBeforeNextZone);



        blackScreen.SetActive(true);
        blackScreenMask.transform.position = maskClosePos.position;
        blackScreenMask.transform.localScale = new Vector2(maxMaskSize, maxMaskSize);
        maskLerpProgression = 0;
        while (maskLerpProgression < 0.98f)
        {
            maskLerpProgression += (1 - maskLerpProgression) * transitionLerpSpeed * Time.deltaTime;
            blackScreenMask.transform.localScale = new Vector2((1 - maskLerpProgression) * maxMaskSize, (1 - maskLerpProgression) * maxMaskSize);
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(0.5f);
        TransitionManager.Instance.blackScreen.SetActive(true);
        LoadNextZone();
    }
}
