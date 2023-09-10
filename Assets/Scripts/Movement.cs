using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    public CharacterController2D controller;

    public float moveSpeed = 50f;

    float horizontalMove = 0f;

    bool jump = false;

    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * moveSpeed;

	if (Input.GetButton("Jump")) 
	{
		jump = true;
	}
     
    }

    void FixedUpdate ()
    {
        controller.Move(horizontalMove * Time.fixedDeltaTime, false, jump);
        jump = false;
    }

        void OnTriggerEnter2D(Collider2D other)
        {
              if (other.CompareTag("SpeedPad"))
              {
               //using variable to initialize and debug any issues
               moveSpeed = 80f;
               //yayyy!
               }
        }

        void OnTriggerExit2D(Collider2D other)
        {
               moveSpeed = 50f;
        }
}
