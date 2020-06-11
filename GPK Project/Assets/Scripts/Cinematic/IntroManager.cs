using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Introduction
{
    public class IntroManager : MonoBehaviour
    {
        public IntroPlayerController introPlayer;
        public float walkingCameraSize;
        public int startZone;
        public VideoPlayer videoPlayer;
        public RawImage videoImage;

        private int introCurrentStep;
        private Camera mainCamera;
        private bool introPlaying;
        void Start()
        {
            videoImage.color = Color.clear;
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

            if(introPlaying)
            {
                if(videoPlayer.isPrepared)
                {
                    videoImage.texture = videoPlayer.texture;
                    videoPlayer.Play();
                    if(!videoPlayer.isPlaying)
                    {
                        SceneManager.LoadScene(startZone);
                    }
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if(collider.CompareTag("Player"))
            {
                videoPlayer.Prepare();
                videoImage.color = Color.white;
                introPlaying = true;
            }
        }
    }
}
