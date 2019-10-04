using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovements : MonoBehaviour
{
    #region GeneralMovements
    Physics _playerBody;

    [SerializeField] private float _jumpForce = 25;
    [SerializeField] private float _speed = 10;

    private float _jumpBuffer = .15f;
    [Range(0, .3f)] [SerializeField] float _movementSmoothing = .05f;
    bool _isDoubleJumping = false;

    [SerializeField] LayerMask _groundLayer;
    [SerializeField] Transform _groundCheck1;
    [SerializeField] Transform _groundCheck2;
    [SerializeField] float _groundedRadius;
    bool _isGrounded = false;
    bool _canDoubleJump = true;

    [SerializeField] Transform _ceilingCheck;

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
        _playerBody = GetComponent<Physics>();
    }


    private void FixedUpdate()
    {
        bool wasGrounded = _isGrounded;
        _isGrounded = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(_groundCheck1.position, _groundedRadius, _groundLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                _isGrounded = true;
            }
        }

        colliders = Physics2D.OverlapCircleAll(_groundCheck2.position, _groundedRadius, _groundLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                _isGrounded = true;
            }
        }

        if (_isGrounded)
        {
            _canDoubleJump = true;
        }
    }

    public void Move(float horizontalMove, bool jump)
    {
        float currentSpeed = _speed;
        if (!_isGrounded) { currentSpeed = _speed * .7f; }

        // Horizontal movement
        Vector2 targetVelocity = new Vector2(horizontalMove * _speed, _playerBody.Velocity.y);

        if (jump)
        {
            if (_isGrounded)
            {
                // Jump
                targetVelocity.y = _jumpForce;
            }
            else if (!_isGrounded && _canDoubleJump)
            {
                // Jump
                Debug.Log("doubleJumped");
                targetVelocity.y = _jumpForce;
                _canDoubleJump = false;
            }
            _isGrounded = false;
        }

        Vector2 newVelocity = _playerBody.Velocity;
        Debug.Log(targetVelocity);
        newVelocity = Vector2.SmoothDamp(_playerBody.Velocity, targetVelocity, ref newVelocity, _movementSmoothing);
        targetVelocity.x = newVelocity.x;
        _playerBody.Velocity = targetVelocity;
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(_groundCheck1.position, _groundedRadius);
        Gizmos.DrawSphere(_groundCheck2.position, _groundedRadius);
    }
}
