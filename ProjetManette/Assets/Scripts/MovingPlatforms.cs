using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MovingPlatforms : CustomMonoBehaviour
{
    [SerializeField] Transform _firstPoint;
    [SerializeField] Transform _secondPoint;
    [SerializeField] float _speed;
    bool _direction;
    Vector2 _vectorDirection;

    private void Start()
    {
        _direction = true;
    }

    private void Update()
    {
        Debug.DrawLine(_firstPoint.position, _secondPoint.position);
    }

    private void FixedUpdate()
    {

        if (_direction)
        {
            _vectorDirection = _secondPoint.position - _firstPoint.position;
        }
        else
        {
            _vectorDirection = _firstPoint.position - _secondPoint.position;
        }

        _vectorDirection = _vectorDirection.normalized;

        float speedScale = _speed * Time.deltaTime;

        Vector3 newPosition = _vectorDirection * speedScale;

        if (_direction && IsAhead(transform.position + newPosition, _secondPoint.position, newPosition))
        {
            transform.position = _secondPoint.position;
            _direction = !_direction;
        }
        else if (!_direction && IsAhead(transform.position + newPosition, _firstPoint.position, newPosition))
        {
            transform.position = _firstPoint.position;
            _direction = !_direction;
        }
        else
        {
            transform.position += newPosition;
        }
    }

    private bool IsAhead(Vector2 first, Vector2 second, Vector2 direction)
    {
        if (Vector2.Distance(first, second) < Vector2.Distance(first + direction, second))
        {
            return true;
        }
        return false;
    }

    public override void OnCustomCollisionEnter(CustomCollision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.collider.transform.SetParent(transform);
        }
    }

    public override void OnCustomCollisionExit(CustomCollision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.collider.transform.SetParent(null);
            collision.collider.GetComponent<CustomPhysics>().Velocity += _vectorDirection * _speed;
        }
    }
}


