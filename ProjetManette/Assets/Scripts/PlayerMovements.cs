using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovements : MonoBehaviour
{
    #region GeneralMovements
    CustomPhysics _playerBody;
    Vector2 _spawnPos;

    [SerializeField] private float _jumpForce = 25;
    [SerializeField] private float _wallJumpForce = 100;
    [SerializeField] private Vector2 _wallJumpVector = Vector2.right + Vector2.up * 0.2f;
    [SerializeField] private float _speed = 10;
    [Range(0, 1f)] [SerializeField] private float _airborneSpeedMultiplier = 0.7f;

    private float _jumpBuffer = .15f;
    [Range(0, .3f)] [SerializeField] float _movementSmoothing = .05f;

    bool _canDoubleJump = true;

    #endregion

    #region Dash
    [SerializeField] private float _dashSpeed = 20;
    public float DashSpeed
    {
        get { return _dashSpeed; }
        set { _dashSpeed = value; }
    }

    [SerializeField] private float _dashCD = 5;
    public float DashCooldown
    {
        get { return _dashCD; }
        set { _dashCD = value; }
    }
    #endregion

    private void Awake()
    {
        _playerBody = GetComponent<CustomPhysics>();
        _spawnPos = gameObject.transform.position;
        GameObject.Find("InputController").GetComponent<InputController>().Reinitiate();
    }


    private void FixedUpdate()
    {
        if (_playerBody.OnGround)
        {
            _canDoubleJump = true;
            _playerBody.IsWallGrabbing = false;
        }
    }

    public void Move(float horizontalMove, bool jump)
    {
        //Make sure to quit wall grabing state
        _playerBody.IsWallGrabbing = false;

        float currentSpeed = _speed;
        if (!_playerBody.OnGround) { currentSpeed = _speed * _airborneSpeedMultiplier; }

        // Horizontal movement
        Vector2 targetVelocity = new Vector2(horizontalMove * currentSpeed, _playerBody.Velocity.y);

        if (jump)
        {
            _playerBody.IsWallGrabbing = false;
            if (_playerBody.OnGround)
            {
                // Jump
                targetVelocity.y = _jumpForce;
            }
            else if (_playerBody.OnWall)
            {
                // WallJump
                Vector2 wallJumpVelocity = new Vector2(_wallJumpVector.x * _playerBody.WallNormal.x, _wallJumpVector.y);
                targetVelocity = wallJumpVelocity.normalized * _wallJumpForce;
                _canDoubleJump = true;
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

    public void Fall()
    {
        _playerBody.Falling = true;
    }

    public void WallGrab(float verticalInput)
    {
        _playerBody.IsWallGrabbing = _playerBody.OnWall;

        if (_playerBody.IsWallGrabbing)
        {
            float newYVelocity = verticalInput * _speed * .4f;
            _playerBody.Velocity = new Vector2(0,  newYVelocity );
        }
    }

    public void Die()
    {
        gameObject.transform.position = new Vector3(_spawnPos.x, _spawnPos.y, 0);
    }
}
