using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class CustomPhysics : MonoBehaviour
{
    private Transform _transform;

    // Physics tuning
    [Range(0, 1)] [SerializeField] private float _dragStrength;
    [Min(0)] [SerializeField] private float _gravityStrength = 1.5f;

    // Speed under which the movement is completely stopped
    [Min(0)] [SerializeField] private float _minimumSpeed = 0.001f;


    // Entity related data
    [ReadOnly] [SerializeField] private Vector2 _velocity = Vector2.zero;
    public Vector2 Velocity
    {
        get { return _velocity; }
        set { _velocity = value; }
    }

    private bool _onGround = false;
    public bool OnGround => _onGround;
    private bool _onCeiling = false;
    public bool OnCeiling => _onCeiling;
    private bool _onWall = false;
    public bool OnWall => _onWall;

    private void Start()
    {
        _transform = GetComponentInParent<Transform>();
    }

    private void FixedUpdate()
    {
        if (OnGround)
        {
            GetComponentInParent<SpriteRenderer>().color = Color.green;
        }
        else
        {
            GetComponentInParent<SpriteRenderer>().color = Color.red;
        }
        _velocity *= (1 - _dragStrength);
        _velocity.y -= _gravityStrength;

        // Null the speed if it is too small
        if (Math.Abs(_velocity.x) < _minimumSpeed)
        {
            _velocity.x = 0;
        }

        if (Math.Abs(_velocity.y) < _minimumSpeed)
        {
            _velocity.y = 0;
        }

        ManageCollision();

        if (Math.Abs(Velocity.magnitude) >= _minimumSpeed)
        {
            Vector2 deltaPos = Time.fixedDeltaTime * Velocity;

            _transform.position += new Vector3(deltaPos.x, deltaPos.y);
        }
        Debug.DrawLine(_transform.position, _transform.position + new Vector3(Velocity.normalized.x,0), Color.green);
        Debug.DrawLine(_transform.position, _transform.position + new Vector3(0,Velocity.normalized.y), Color.green);

    }

    /// <summary>
    /// Checks for collisions during the movement; update position and velocity.
    /// </summary>
    /// Casts a ray along the x and y movement of the player. If there is no hit, cast the full cube.
    /// If there is any hit, the velocity is canceled and the position set accordingly.
    /// <returns>True if there was a collision, false otherwise.</returns>
    private void ManageCollision()
    {
        // Raycast on each axis independently
        for (int i = 0; i < 2; i++)
        {
            Vector2 oneAxisVelocity = new Vector2();
            oneAxisVelocity[i] = Velocity[i];
            
            // Cast a singular ray from the center
            RaycastHit2D raycastHit = Physics2D.Raycast(
                _transform.position,
                oneAxisVelocity.normalized,
                oneAxisVelocity.magnitude * Time.fixedDeltaTime + _transform.localScale[i] / 2);


            // If the ray did not hit anything, there might be something else under the corners
            if (raycastHit.collider == null)
            {
                // So cast a the full box of the player
                raycastHit = Physics2D.BoxCast(
                    _transform.position,
                    _transform.localScale,
                    0,
                    oneAxisVelocity.normalized,
                    oneAxisVelocity.magnitude * Time.fixedDeltaTime);
            }
            else
            {
                // Takes into account the width of the cube as the singular ray is cast from the center
                raycastHit.distance -= _transform.localScale[i] / 2;
            }

            if (raycastHit.collider != null)
            {
                _transform.position += (Vector3)oneAxisVelocity.normalized * raycastHit.distance;

                // Checks if the movement is in the same direction as the impact normal
                // If it is not, then null it.
                if (_velocity[i] * raycastHit.normal[i] < 0)
                {
                    // Checks the side of the collision
                    // FIXME : This is so dirty, my goodness
                    if (i == 0)
                    {
                        _onWall = true;
                    }
                    else
                    {
                        if (_velocity[i] > 0)
                        {
                            _onCeiling = true;
                            _onGround = false;
                        }
                        else if (_velocity[i] < 0)
                        {
                            _onCeiling = false;
                            _onGround = true;
                        }
                    }
                    _velocity[i] = 0;
                }
            }
            else
            {
                if (i == 0)
                {
                    _onWall = false;
                }
                else
                {
                    _onCeiling = false;
                    _onGround = false;
                }
            }
        }
    }
}
