﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    [SerializeField] PlayerMovements playerMovements;
    private float timer;

    void FixedUpdate()
    {
        timer -= Time.fixedTime;
        float horizontalInput = Input.GetAxis("Horizontal");
        float crossPosition = 10 * Input.GetAxis("Horizontal_Cross");
        bool jump = Input.GetButtonDown("A") || Input.GetKeyDown(KeyCode.Space);


        if ((Input.GetButtonDown("B") || Input.GetKeyDown(KeyCode.LeftShift)) && timer <= 0)
        {
            // Dash
            horizontalInput *= playerMovements.DashSpeed;
            timer = playerMovements.DashCooldown;

        }
        //  else if (Input.GetButton("X"))
        //      // Do smth
        //      ;
        //  else if (Input.GetButton("Y"))
        //      // Do smth 
        //      ;
        //  else ;

        playerMovements.Move(horizontalInput, jump);
    }
}
