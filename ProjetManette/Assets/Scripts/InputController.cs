using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    [SerializeField] PlayerMovements playerMovements;
    private float timer;

    private bool _holdingJump;

    void FixedUpdate()
    {
        timer -= Time.fixedTime;
        float horizontalInput = Input.GetAxis("Horizontal");
        float crossPosition = 10 * Input.GetAxis("Horizontal_Cross");
        bool jump = Input.GetButton("A") || Input.GetKey(KeyCode.Space);

        // The player has to stop pressing jump to jump again
        bool jumpAction = jump & !_holdingJump;
        _holdingJump = jump;

        if ((Input.GetButton("B") || Input.GetKey(KeyCode.LeftShift)) && timer <= 0)
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

        playerMovements.Move(horizontalInput, jumpAction);
    }
}
