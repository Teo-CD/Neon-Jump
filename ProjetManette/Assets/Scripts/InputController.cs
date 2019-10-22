using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    [SerializeField] PlayerMovements playerMovements;
    [SerializeField] GameObject _dashTrailFX;
    [SerializeField] GameObject _dashCdFX;
    bool _canDash = true;
    [SerializeField] AudioSource _audioSource;

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

        if (timer < .8f)
        {
            if (!_canDash)
            {
                StartCoroutine(DashCdFX());
            }
            _canDash = true;
        }
        else _canDash = false;

        if ((Input.GetButton("RB") || Input.GetKey(KeyCode.LeftShift)) && timer < 0)
        {
            // Dash
            horizontalInput *= playerMovements.DashSpeed;
            timer = playerMovements.DashCooldown;
            StartCoroutine(DashTrailFX());
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
            if (jumpAction && !_audioSource.isPlaying)
            {
                _audioSource.Play();
            }
            playerMovements.Move(horizontalInput, jumpAction);
        }

    }

    IEnumerator DashTrailFX()
    {
        _dashTrailFX.SetActive(true);
        yield return new WaitForSeconds(1f);
        _dashTrailFX.SetActive(false);

    }

    IEnumerator DashCdFX()
    {
        _dashCdFX.SetActive(true);
        yield return new WaitForSeconds(.9f);
        _dashCdFX.SetActive(false);

    }
}
