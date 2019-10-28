using System;
using System.Collections.Generic;
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

    // Colliders currently affecting the object
    private List<Collider2D> _colliders = new List<Collider2D>();

    // Physics tuning
    [Range(0, 1)] [SerializeField] private float _dragStrength;
    [Min(0)] [SerializeField] private float _gravityStrength = 1.5f;
    public float GravityStrength => _gravityStrength;

    // Speed under which the movement is completely stopped
    [Min(0)] [SerializeField] private float _minimumSpeed = 0.001f;
    // Deactivated if zero
    [Min(0)] [SerializeField] private float _maximumSpeed = 30;


    // Entity related data
    [ReadOnly] [SerializeField] private Vector2 _velocity = Vector2.zero;

    public Vector2 Velocity
    {
        get { return _velocity; }
        set { _velocity = value; }
    }

    // Scale of the collider used to control inputs and movement.
    [SerializeField] private float _inputColliderScale = 1.4f;
    
    [SerializeField] private bool[] _collidingSide = new bool[4];

    public bool OnGround => _collidingSide[3];
    public bool OnCeiling => _collidingSide[2];
    public bool OnWall => _collidingSide[0] || _collidingSide[1];
    public Vector2 WallNormal => _collidingSide[0] ? Vector2.left : _collidingSide[1] ? Vector2.right : Vector2.zero;

    // Controls whether or not the player falls through platforms
    [Unity.Collections.ReadOnlyAttribute][SerializeField] private bool _falling;
    public bool Falling
    {
        get { return _falling; }
        set { _falling = value; }
    }

    [SerializeField] private bool _isWallGrabbing = false;
    public bool IsWallGrabbing
    {
        get { return _isWallGrabbing;}
        set { _isWallGrabbing = value; }
    }

    private void Start()
    {
        _transform = GetComponentInParent<Transform>();
    }

    private void FixedUpdate()
    {
#if DEBUG
        if (OnGround)
        {
            GetComponentInParent<SpriteRenderer>().color = Color.green;
        }
        else if (OnWall)
        {
            GetComponentInParent<SpriteRenderer>().color = Color.white;
        }
        else
        {
            GetComponentInParent<SpriteRenderer>().color = Color.red;
        }
#endif
        
        _velocity *= (1 - _dragStrength);
        
        var downVelocity = _gravityStrength;

        // Slow down the fall if leaning on a wall
        if (OnWall && _velocity.y < 0)
        {
            downVelocity *= 0.2f;
        }
        
        if (_isWallGrabbing)
        {
            downVelocity = 0;
        }
        
        _velocity.y -= downVelocity;

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

        if (_maximumSpeed > 0 && Velocity.magnitude >= _maximumSpeed)
        {
            Velocity = Velocity.normalized * _maximumSpeed;
        }
        
        if (Math.Abs(Velocity.magnitude) >= _minimumSpeed)
        {
            Vector2 deltaPos = Time.fixedDeltaTime * Velocity;

            _transform.position += new Vector3(deltaPos.x, deltaPos.y);
        }

#if DEBUG
        Debug.DrawLine(_transform.position, _transform.position + new Vector3(Velocity.normalized.x, 0), Color.green);
        Debug.DrawLine(_transform.position, _transform.position + new Vector3(0, Velocity.normalized.y), Color.green);
#endif
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
            Vector2 oneAxisVelocity = new Vector2 { [axis] = Velocity[axis] };

            // Cast a singular ray from the center
            RaycastHit2D raycastHit = Physics2D.Raycast(
                _transform.position,
                oneAxisVelocity.normalized,
                oneAxisVelocity.magnitude * Time.fixedDeltaTime + _transform.lossyScale[axis] / 2);


            // If the ray did not hit anything, there might be something else under the corners
            if (raycastHit.collider == null)
            {
                // So cast a the resized box of the player
                // in order to prevent detecting collisions on the same axis twice.
                raycastHit = Physics2D.BoxCast(
                    _transform.position,
                    _transform.lossyScale * 0.95f,
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
                CollisionUpdate(raycastHit, axis);
            }
            else
            {
                if (axis == (int) Axis.Y)
                {
                    _falling = false;
                }
            }
        }

        // Reset collisions
        for (var i = 0; i < _collidingSide.Length; i++)
        {
            _collidingSide[i] = false;
        }

        // Cast a slightly bigger box around the player
        RaycastHit2D[] surroundingHits = Physics2D.BoxCastAll(
            _transform.position,
            _transform.lossyScale * _inputColliderScale,
            0,
            Vector2.zero,
            0);

        // For each collider hit, handle collision and check what is the biggest axis of its norm and adjust the colliding sides.
        foreach (RaycastHit2D hit in surroundingHits)
        {
            collisionCount++;
            HandleCollisionEnterStay(hit.collider);
            
            Axis biggestAxis =  Math.Abs(hit.normal[(int) Axis.X]) > Math.Abs(hit.normal[(int) Axis.Y]) ? Axis.X : Axis.Y;

            if (hit.normal[(int) biggestAxis] > 0)
            {
                _collidingSide[(int) biggestAxis * 2 + 1] = true;
            }
            else
            {
                _collidingSide[(int) biggestAxis * 2] = true;
            }
        }

        HandleCollisionExit(collisionCount);
    }

    /// <summary>
    /// Update the player state after the collision : position, speed and collision status.
    /// </summary>
    /// <param name="raycastHit">Hit from the raycast</param>
    /// <param name="axis">Axis the collision occured on</param>
    private void CollisionUpdate(RaycastHit2D raycastHit, int axis)
    {
        // Go through platforms if coming from the bottom or sides or if falling down from it
        if (raycastHit.collider.gameObject.CompareTag("Platform") &&
            (_falling ||
             _velocity[(int)Axis.Y] > 0 ||
             axis == (int)Axis.X))
        {
            return;
        }

        if (raycastHit.collider.gameObject.CompareTag("DeathZone"))
        {
            gameObject.GetComponent<PlayerMovements>().Die();
        }

        _falling = false;

        // Checks if the movement is in the same direction as the impact normal
        // If it is not, then null it.
        if (_velocity[axis] * raycastHit.normal[axis] < 0)
        {
            _velocity[axis] = 0;
        }

        // Compute the position that prevents the collision
        var relativeDirection = raycastHit.point[axis] - _transform.position[axis] < 0 ? 1 : -1;
        var newPosition = _transform.position;
        newPosition[axis] = raycastHit.point[axis] + relativeDirection * _transform.lossyScale[axis] / 2;
        _transform.position = newPosition;
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

        // Find the collider in the list.
        // As there will never be more than a few items, it is more efficient to search manually.
        int colliderIndex = -1;
        for (int i = 0; i < _colliders.Count; i++)
        {
            if (_colliders[i].Equals(collidingObject))
            {
                colliderIndex = i;
                break;
            }
        }

        if (colliderIndex >= 0)
        {
            foreach (CustomMonoBehaviour customPhysicsHandler in customPhysicsHandlers)
            {
                customPhysicsHandler.OnCustomCollisionStay(new CustomCollision(gameObject));
            }

            _colliders.RemoveAt(colliderIndex);
        }
        else
        {
            foreach (CustomMonoBehaviour customPhysicsHandler in customPhysicsHandlers)
            {
                customPhysicsHandler.OnCustomCollisionEnter(new CustomCollision(gameObject));
            }
        }
        _colliders.Insert(0, collidingObject);
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
