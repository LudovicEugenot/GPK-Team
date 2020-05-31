using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Introduction
{
    public class NarrationTrigger : MonoBehaviour
    {
        public Narrator narrator;
        public Narrator.Narration narration;
        private bool triggered;

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if(!triggered)
            {
                triggered = true;
                narrator.StartNarration(narration);
            }
        }
    }
}
