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
                // FIXME : Find a better way to have the player move on conveyors
                // Currently inputs are not taken into account for some reason.
                _playerBody.Velocity = _playerBody.Velocity + Vector2.right * _speed;
            }
            else
            {
                _playerBody.Velocity = _playerBody.Velocity + Vector2.up * _speed;
            }
        }
    }
}
