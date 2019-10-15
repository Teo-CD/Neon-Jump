﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    [SerializeField] PlayerMovements playerMovements;
    private float timer;

    private bool _holdingJump;

    void FixedUpdate()
    {
        timer -= Time.fixedDeltaTime;
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        //float crossPosition = 10 * Input.GetAxis("Horizontal_Cross");
        bool jump = Input.GetButton("A") || Input.GetKey(KeyCode.Space);

        // The player has to stop pressing jump to jump again
        bool jumpAction = jump & !_holdingJump;
        _holdingJump = jump;

        if ((Input.GetButton("RB") || Input.GetKey(KeyCode.LeftShift)) && timer <= 0)
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

        // FIXME : Find the best way to register the combination to go down
        if (jump && Input.GetAxis("Vertical") < 0)
        {
            playerMovements.Fall();
        }
        else if (Input.GetButton("LB"))
        {
            Debug.Log("VertInput " + verticalInput);   
            playerMovements.WallGrab(verticalInput);
        }

        else
        {
            playerMovements.Move(horizontalInput, jumpAction);
        }

    }
}
