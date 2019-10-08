using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovements : MonoBehaviour
{
    #region GeneralMovements
    CustomPhysics _playerBody;

    [SerializeField] private float _jumpForce = 25;
    [SerializeField] private float _speed = 10;

    private float _jumpBuffer = .15f;
    [Range(0, .3f)] [SerializeField] float _movementSmoothing = .05f;
    bool _isDoubleJumping = false;

    bool _canDoubleJump = true;

    #endregion

    #region Dash
    [SerializeField] private float _dashSpeed = 10;
    public float DashSpeed
    {
        get { return _dashSpeed; }
        set { _dashSpeed = value; }
    }

    [SerializeField] private float _dashCD = 2;
    public float DashCooldown
    {
        get { return _dashCD; }
        set { _dashCD = value; }
    }
    #endregion

    private void Awake()
    {
        _playerBody = GetComponent<CustomPhysics>();
    }


    private void FixedUpdate()
    {
        if (_playerBody.OnGround)
        {
            _canDoubleJump = true;
        }
    }

    public void Move(float horizontalMove, bool jump)
    {
        float currentSpeed = _speed;
        if (!_playerBody.OnGround) { currentSpeed = _speed * .7f; }

        // Horizontal movement
        Vector2 targetVelocity = new Vector2(horizontalMove * _speed, _playerBody.Velocity.y);

        if (jump)
        {
            if (_playerBody.OnGround)
            {
                // Jump
                targetVelocity.y = _jumpForce;
            }
            else if (!_playerBody.OnGround && _canDoubleJump)
            {
                // DoubleJump
                targetVelocity.y = _jumpForce;
                _canDoubleJump = false;
            }
        }

        Vector2 newVelocity = _playerBody.Velocity;
        newVelocity = Vector2.SmoothDamp(_playerBody.Velocity, targetVelocity, ref newVelocity, _movementSmoothing);
        targetVelocity.x = newVelocity.x;
        _playerBody.Velocity = targetVelocity;
    }
}
