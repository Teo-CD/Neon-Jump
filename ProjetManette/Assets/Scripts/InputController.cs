using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    InputController instance;

    PlayerMovements playerMovements;
    GameObject Player;
    bool _canDash = true;
    FXManager fXManager;
    [SerializeField] AudioSource _jumpSFX;
    [SerializeField] AudioSource _dashSFX;


    private float timer;

    private bool _holdingJump;


    private void Awake()
    {
        if (instance is null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        Reinitiate();
    }


    public void Reinitiate()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        fXManager = Player.GetComponent<FXManager>();
        playerMovements = Player.GetComponent<PlayerMovements>();
    }

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

        if (timer < .8f)
        {
            if (!_canDash)
            {
                fXManager.PlayDashCdFX();
            }
            _canDash = true;
        }
        else _canDash = false;

        if ((Input.GetButton("RB") || Input.GetKey(KeyCode.LeftShift)) && timer < 0)
        {
            // Dash
            horizontalInput *= playerMovements.DashSpeed;
            timer = playerMovements.DashCooldown;
            fXManager.PlayDashTrailFX();
            if (!_dashSFX.isPlaying)
            {
                _dashSFX.Play();
            }
        }


        if (jump && Input.GetAxis("Vertical") < 0)
        {
            playerMovements.Fall();
        }
        else if (Input.GetButton("LB"))
        {
            playerMovements.WallGrab(verticalInput);
        }
        else
        {
            if (jumpAction && !_jumpSFX.isPlaying)
            {
                _jumpSFX.Play();
                fXManager.PlayJumpFX();
            }
            playerMovements.Move(horizontalInput, jumpAction);
        }

    }

}
