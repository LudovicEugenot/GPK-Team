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
        public VideoClip firstVideo;
        public VideoClip secondVideo;
        public VideoPlayer videoPlayer;
        public RawImage videoImage;
        public Material recolorMaterial;
        public float recolorLerpSpeed;
        public GameObject clarinetto;
        public float clarinettoMovingAmplitude;
        public float clarinettoMovingSpeed;
        public AudioClip recolorSound;

        private int introCurrentStep;
        private Camera mainCamera;
        [HideInInspector] public bool videoPlaying;
        private bool videoStartFlag;
        private bool endVideoFlag;
        private Vector2 initialClarinettoPos;
        private AudioSource source;
        void Start()
        {
            recolorMaterial.SetFloat("Vector1_Progression", 0);
            source = GetComponent<AudioSource>();
            initialClarinettoPos = clarinetto.transform.position;
            videoImage.color = Color.clear;
            mainCamera = Camera.main;
            introCurrentStep = 0;
            StartVideo(firstVideo);
        }


        void Update()
        {
            MoveClarinetto();

            if(Input.GetKeyDown(KeyCode.S))
            {
                SceneManager.LoadScene(startZone);
            }

            switch (introCurrentStep)
            {
                case 0:
                    mainCamera.transform.position = new Vector3(introPlayer.transform.position.x, 0.0f, -10.0f);
                    mainCamera.orthographicSize = walkingCameraSize;

                    if(endVideoFlag)
                    {
                        endVideoFlag = false;
                        introCurrentStep = 1;
                    }
                    break;

                case 1:
                    mainCamera.transform.position = new Vector3(introPlayer.transform.position.x, 0.0f, -10.0f);
                    mainCamera.orthographicSize = walkingCameraSize;

                    if (endVideoFlag)
                    {
                        endVideoFlag = false;
                        introCurrentStep = 2;
                        clarinetto.SetActive(false);

                        SceneManager.LoadScene(startZone);
                        source.PlayOneShot(recolorSound);
                        introPlayer.animator.SetTrigger("Relive");
                        StartCoroutine(Recolor());
                    }
                    break;

                case 2:
                    mainCamera.transform.position = new Vector3(0.0f, 0.0f, -10.0f);
                    mainCamera.orthographicSize = 5.625f;
                    break;
            }

            if(videoStartFlag)
            {
                if(videoPlayer.isPrepared)
                {
                    videoStartFlag = false;
                    videoPlaying = true;
                    videoImage.color = Color.white;
                    videoImage.texture = videoPlayer.texture;
                    videoPlayer.Play();
                }
            }

            if(videoPlaying)
            {
                if (!videoPlayer.isPlaying)
                {
                    videoPlaying = false;
                    videoPlayer.Stop();
                    endVideoFlag = true;
                    videoImage.color = Color.clear;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if(collider.CompareTag("Player"))
            {
                StartVideo(secondVideo);
            }
        }

        private void StartVideo(VideoClip clip)
        {
            videoPlayer.clip = clip;
            videoPlayer.Prepare();
            videoImage.color = Color.black;
            videoStartFlag = true;
        }

        private IEnumerator Recolor()
        {
            float lerp = 0;
            while (lerp < 0.98f)
            {
                lerp += (1 - lerp) * Time.deltaTime * recolorLerpSpeed;
                recolorMaterial.SetFloat("Vector1_Progression", lerp);
                yield return new WaitForEndOfFrame();
            }
        }

        private void MoveClarinetto()
        {
            clarinetto.transform.position = new Vector2(initialClarinettoPos.x, initialClarinettoPos.y + Mathf.Sin(Time.time * clarinettoMovingSpeed) * clarinettoMovingAmplitude);
        }
    }
}
