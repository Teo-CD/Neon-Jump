using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Physics : MonoBehaviour
{
    private Transform _transform;
    
    // Physics tuning
    [Range(0,1)][SerializeField] private float _dragStrength;
    [Min(0)][SerializeField] private float _gravityStrength;

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

        if (Math.Abs(Velocity.magnitude) >= _minimumSpeed)
        {
            Vector2 deltaPos = Time.fixedDeltaTime * Velocity;

            _transform.position += new Vector3(deltaPos.x, deltaPos.y);
        }
    }
}
