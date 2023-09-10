using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchPad : MonoBehaviour
{

    public CharacterController2D launchController;

    public Movement launchController2;

    bool launch = false;

    void Collision2D (Collider2D other) 
    {
        if (other.CompareTag("PlayerLayer"))
        {
             launch = true;
        }
    }

    void FixedUpdate () 
    {
         launch = false;
    }

}
