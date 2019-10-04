using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class Physics : MonoBehaviour
{
    private Transform _transform;
    
    // Physics tuning
    [Range(0,1)][SerializeField] private float _dragStrength;
    [Min(0)][SerializeField] private float _gravityStrength = 1.5f;

    // Speed under which the movement is completely stopped
    [Min(0)][SerializeField] private float _minimumSpeed;
    
    
    // Entity related data
    [ReadOnly][SerializeField] private Vector2 _velocity = Vector2.zero;
    public Vector2 Velocity
    {
        get { return _velocity; }
        set { _velocity = value; }
    }

    private void Start()
    {
        _transform = GetComponentInParent<Transform>();
    }

    private void FixedUpdate()
    {
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

        if (!manageCollision())
        {
            // TODO : Reset onGround and onWall
        }
        
        if (Math.Abs(Velocity.magnitude) >= _minimumSpeed)
        {
            Vector2 deltaPos = Time.fixedDeltaTime * Velocity;

            _transform.position += new Vector3(deltaPos.x, deltaPos.y);
        }
    }

    /// <summary>
    /// Checks for collisions during the movement; update position and velocity.
    /// </summary>
    /// Casts a ray along the x and y movement of the player. If there is no hit, cast the full cube.
    /// If there is any hit, the velocity is canceled and the position set accordingly.
    /// <returns>True if there was a collision, false otherwise.</returns>
    private bool manageCollision()
    {
        bool collided = false;
        // Raycast on each axis independently
        for (int i = 0; i < 2; i++)
        {
            Vector2 oneAxisVelocity = new Vector2();
            oneAxisVelocity[i] = Velocity[i];
            
            Debug.DrawLine(_transform.position,_transform.position+(Vector3)oneAxisVelocity.normalized,Color.green);

            // Cast a singular ray from the center
            RaycastHit2D raycastHit = Physics2D.Raycast(
                _transform.position,
                oneAxisVelocity.normalized,
                oneAxisVelocity.magnitude * Time.fixedDeltaTime + _transform.localScale[i]/2);

            
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
                    _velocity[i] = 0;
                }
            }
            collided = true;
        }
        return collided;
    }
}
