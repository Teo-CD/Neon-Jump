﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

enum Axis
{
    X = 0,
    Y = 1,
}

public class CustomPhysics : MonoBehaviour
{
    private Transform _transform;

    // Colliders currently currently affecting the object
    private List<Collider2D> _colliders = new List<Collider2D>();

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

    [SerializeField] private bool[] _collidingSide = new bool[4];

    public bool OnGround => _collidingSide[3];
    public bool OnCeiling => _collidingSide[2];
    public bool OnWall => _collidingSide[0] || _collidingSide[1];
    public Vector2 WallNormal => _collidingSide[0] ? Vector2.left : _collidingSide[1] ? Vector2.right : Vector2.zero;

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

        DetectCollision();

        if (Math.Abs(Velocity.magnitude) >= _minimumSpeed)
        {
            Vector2 deltaPos = Time.fixedDeltaTime * Velocity;

            _transform.position += new Vector3(deltaPos.x, deltaPos.y);
        }

        Debug.DrawLine(_transform.position, _transform.position + new Vector3(Velocity.normalized.x, 0), Color.green);
        Debug.DrawLine(_transform.position, _transform.position + new Vector3(0, Velocity.normalized.y), Color.green);

    }

    /// <summary>
    /// Checks for collisions during the movement; update position and velocity.
    /// </summary>
    /// Casts a ray along the x and y movement of the player. If there is no hit, cast the full cube.
    /// If there is any hit, the velocity is canceled and the position set accordingly.
    /// <returns>True if there was a collision, false otherwise.</returns>
    private void DetectCollision()
    {
        int collisionCount = 0;
        // Raycast on each axis independently
        foreach (int axis in Enum.GetValues(typeof(Axis)))
        {
            Vector2 oneAxisVelocity = new Vector2 {[axis] = Velocity[axis]};

            // Cast a singular ray from the center
            RaycastHit2D raycastHit = Physics2D.Raycast(
                _transform.position,
                oneAxisVelocity.normalized,
                oneAxisVelocity.magnitude * Time.fixedDeltaTime + _transform.lossyScale[axis] / 2);


            // If the ray did not hit anything, there might be something else under the corners
            if (raycastHit.collider == null)
            {
                // So cast a the full box of the player
                raycastHit = Physics2D.BoxCast(
                    _transform.position,
                    _transform.lossyScale,
                    0,
                    oneAxisVelocity.normalized,
                    oneAxisVelocity.magnitude * Time.fixedDeltaTime);
            }
            else
            {
                // Takes into account the width of the cube as the singular ray is cast from the center
                raycastHit.distance -= _transform.lossyScale[axis] / 2;
            }

            if (raycastHit.collider != null)
            {
                collisionCount++;
                HandleCollisionEnterStay(raycastHit.collider);

                CollisionUpdate(raycastHit,axis,oneAxisVelocity);
            }
            else
            {
                _collidingSide[2 * axis] = false;
                _collidingSide[2 * axis + 1] = false;
            }
        }

        HandleCollisionExit(collisionCount);
    }

    /// <summary>
    /// Update the player state after the collision : position, speed and collision status.
    /// </summary>
    /// <param name="raycastHit">Hit from the raycast</param>
    /// <param name="axis">Axis the collision occured on</param>
    /// <param name="velocity">Velocity on this axis</param>
    private void CollisionUpdate(RaycastHit2D raycastHit, int axis, Vector2 velocity)
    {
        // Go through platforms if coming from the bottom or sides
        if (raycastHit.collider.gameObject.CompareTag("Platform") &&
            (_velocity[(int)Axis.Y] > 0 ||
             axis == (int)Axis.X))
        {
            return;
        }
        
        // Checks if the movement is in the same direction as the impact normal
        // If it is not, then null it.
        if (_velocity[axis] * raycastHit.normal[axis] < 0)
        {
            // Checks the side of the collision
            if (_velocity[axis] > 0)
            {
                _collidingSide[2 * axis] = true;
                _collidingSide[2 * axis + 1] = false;
            }
            else if (_velocity[axis] < 0)
            {
                _collidingSide[2 * axis] = false;
                _collidingSide[2 * axis + 1] = true;
            }
            else
            {
                _collidingSide[2 * axis] = false;
                _collidingSide[2 * axis + 1] = false;
            }

            _velocity[axis] = 0;
            
        }
        _transform.position += (Vector3) velocity.normalized * raycastHit.distance;
    }

    /// <summary>
    /// Handles the calls to OnCustomCollisionEnter and OnCustomCollisionStay. Colliders still colliding
    /// are moved to the beginning of the _colliders in order to always keep active colliders at indices
    /// less than collisionCount in ManageCollision().
    /// </summary>
    /// <param name="collidingObject">Incoming collider to check</param>
    private void HandleCollisionEnterStay(Collider2D collidingObject)
    {
        var customPhysicsHandlers = collidingObject.gameObject.GetComponents<CustomMonoBehaviour>();
        // If the object does not handle custom physics, do not take time searching for it
        if (customPhysicsHandlers.Length == 0)
        {
            return;
        }
        
        if (_colliders.Contains(collidingObject))
        {
            foreach (CustomMonoBehaviour customPhysicsHandler in customPhysicsHandlers)
            {
                customPhysicsHandler.OnCustomCollisionStay(new CustomCollision(gameObject));
            }
            
            // TODO : Find a better way than searching for the object twice ?
            _colliders.Remove(collidingObject);
        }
        else
        {
            foreach (CustomMonoBehaviour customPhysicsHandler in customPhysicsHandlers)
            {
                customPhysicsHandler.OnCustomCollisionEnter(new CustomCollision(gameObject));
            }
        }
        _colliders.Insert(0,collidingObject);
    }

    /// <summary>
    /// Calls OnCustomCollisionExit() for the objects which are no longer colliding with this object.
    /// Removes their colliders from _colliders.
    /// </summary>
    /// <param name="collisionCount">Number of collisions to keep</param>
    private void HandleCollisionExit(int collisionCount)
    {
        for (int i = collisionCount; i < _colliders.Count; i++)
        {
            var customPhysicsHandlers = _colliders[i].gameObject.GetComponents<CustomMonoBehaviour>();
            _colliders.RemoveAt(i);
            // If the object does not handle custom physics, skip it
            if (customPhysicsHandlers.Length != 0)
            {
                foreach (CustomMonoBehaviour customPhysicsHandler in customPhysicsHandlers)
                {
                    customPhysicsHandler.OnCustomCollisionExit(new CustomCollision(gameObject));
                }
            }
        }
    }
}
