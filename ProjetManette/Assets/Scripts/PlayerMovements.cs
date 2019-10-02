using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovements : MonoBehaviour
{
    Physics _playerBody;

    [SerializeField] private float _jumpForce = 100;
    [SerializeField] private float _speed = 30;

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
        else
        {
            _jumpBuffer -= Time.deltaTime;
            if (_jumpBuffer > 0)
            {
                _isGrounded = true;
            }
        }
    }

    public void Move(float horizontalMove, bool jump)
    {
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
                targetVelocity.y = _jumpForce;
                _canDoubleJump = false;
            }
            _isGrounded = false;
        }

        Vector2 newVelocity = _playerBody.Velocity;
        _playerBody.Velocity = Vector2.SmoothDamp(_playerBody.Velocity, targetVelocity, ref newVelocity, _movementSmoothing);
        _playerBody.Velocity = newVelocity;
    }

}
