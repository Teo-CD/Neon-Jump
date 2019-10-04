using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    [SerializeField] PlayerMovements playerMovements;


    void FixedUpdate()
    {

        float horizontalInput = Input.GetAxis("Horizontal");
        float crossPosition = 10 * Input.GetAxis("Horizontal_Cross");
        bool jump = false;

        if (Input.GetButton("A") || Input.GetKeyDown(KeyCode.Space)) {
            jump = true;
        }


        //if (Input.GetButton("B"))
        //      // Do smth 
        //      ;
        //  else if (Input.GetButton("X"))
        //      // Do smth
        //      ;
        //  else if (Input.GetButton("Y"))
        //      // Do smth 
        //      ;
        //  else ;

        Debug.Log(horizontalInput);
        playerMovements.Move(horizontalInput, jump);
    }
}
