using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Introduction
{
    public class IntroManager : MonoBehaviour
    {
        public IntroPlayerController introPlayer;
        public float walkingCameraSize;

        private int introCurrentStep;
        private Camera mainCamera;
        void Start()
        {
            mainCamera = Camera.main;
            introCurrentStep = 0;
        }


        void Update()
        {
            switch (introCurrentStep)
            {
                case 0:
                    mainCamera.transform.position = new Vector3(introPlayer.transform.position.x, 0.0f, -10.0f);
                    mainCamera.orthographicSize = walkingCameraSize;
                    break;
            }
        }
    }
}
