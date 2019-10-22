using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : CustomMonoBehaviour
{
    [SerializeField] private float _speed = 3;
    [SerializeField] private string _axis = "X";
    bool _isX = true;

    private void Start()
    {
        if (_axis == "Y")
        {
            _isX = false;
        }
    }

    public override void OnCustomCollisionStay(CustomCollision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CustomPhysics _playerBody = collision.gameObject.GetComponent<CustomPhysics>();
            if (_isX)
            {
                _playerBody.Velocity = new Vector2(_speed, _playerBody.Velocity.y);
            }
            else
            {
                _playerBody.Velocity = new Vector2(_playerBody.Velocity.x, _speed);
            }
        }
    }
}
