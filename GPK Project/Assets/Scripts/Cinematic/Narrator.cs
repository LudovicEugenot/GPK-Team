using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Introduction
{
    public class Narrator : MonoBehaviour
    {
        public float fadeTime;
        public float apparitionOffset;
        public Text narratorText;

        private Narration currentNarration;
        private int currentSentence;
        private bool isNarrating;
        private bool canShowNextSentence;
        private Vector2 initialTextPos;
        private Color textColor;

        private void Start()
        {
            textColor = narratorText.color;
            initialTextPos = narratorText.rectTransform.anchoredPosition;
            narratorText.gameObject.SetActive(false);
        }

        void Update()
        {
            UpdateNarration();
        }

        public void StartNarration(Narration narration)
        {
            if(!isNarrating)
            {
                narratorText.gameObject.SetActive(true);
                isNarrating = true;
                canShowNextSentence = true;
                currentNarration = narration;
                currentSentence = 0;
            }
        }

        public void UpdateNarration()
        {
            if(isNarrating)
            {
                if(currentSentence < currentNarration.sentences.Length)
                {
                    if(canShowNextSentence)
                    {
                        StartCoroutine(ShowSentence());
                    }
                }
                else
                {
                    narratorText.gameObject.SetActive(false);
                    isNarrating = false;
                }
            }
        }

        private IEnumerator ShowSentence()
        {
            canShowNextSentence = false;
            narratorText.text = currentNarration.sentences[currentSentence];
            narratorText.rectTransform.anchoredPosition = new Vector2(narratorText.rectTransform.anchoredPosition.x, narratorText.rectTransform.anchoredPosition.y + apparitionOffset);
            StartCoroutine(TextFade(true));
            while (Vector2.Distance(narratorText.rectTransform.anchoredPosition, initialTextPos) > 1)
            {
                narratorText.rectTransform.anchoredPosition += initialTextPos - narratorText.rectTransform.anchoredPosition;
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(currentNarration.timeBetweenSentences);
            StartCoroutine(TextFade(false));
            yield return new WaitForSeconds(fadeTime);
            currentSentence++;
            canShowNextSentence = true;
        }

        private IEnumerator TextFade(bool fadeIn)
        {
            float timer = fadeTime;
            while(timer > 0)
            {
                narratorText.color = new Color(textColor.r, textColor.g, textColor.b, fadeIn ? (1 - timer / fadeTime) : timer / fadeTime );
                timer -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        [System.Serializable]
        public class Narration
        {
            [TextArea] public string[] sentences;
            public float timeBetweenSentences;
        }
    }
}

