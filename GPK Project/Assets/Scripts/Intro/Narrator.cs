using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Introduction
{
    public class Narrator : MonoBehaviour
    {
        public Text narratorText;

        private NarratorTalk currentTalk;
        private int currentSentence;
        private bool isNarrating;
        private bool canShowNextSentence;

        void Update()
        {
            UpdateTalk();
        }

        public void StartNarratorTalk(NarratorTalk talk)
        {
            isNarrating = true;
        }

        public void UpdateTalk()
        {
            if(isNarrating)
            {
                if(currentSentence < currentTalk.sentences.Length)
                {

                }
            }
        }

        public class NarratorTalk
        {
            public string[] sentences;
            public float timeBetweenSentences;
        }
    }
}

