using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Introduction
{
    public class IntroPlayerController : MonoBehaviour
    {
        public float walkingSpeed;
        public float minusXBoundary;

        private Vector2 mousePosition;
        private Camera mainCamera;
        private Rigidbody2D rb;
        void Start()
        {
            mainCamera = Camera.main;
            rb = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            UpdateWalk();
        }

        private void UpdateWalk()
        {
            mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mouseDirection = mousePosition - (Vector2)transform.position;
            mouseDirection.Normalize();
            if (Input.GetButton("Blink"))
            {
                if (mouseDirection.x > 0)
                {
                    rb.velocity = new Vector2(walkingSpeed * Time.deltaTime, 0.0f);
                }
                else if (transform.position.x > minusXBoundary + 0.1f)
                {
                    rb.velocity = new Vector2(-walkingSpeed * Time.deltaTime, 0.0f);
                }
                else
                {
                    rb.velocity = Vector2.zero;
                }
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }
    }
}
