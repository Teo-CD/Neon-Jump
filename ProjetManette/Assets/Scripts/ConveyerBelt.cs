using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyerBelt : CustomMonoBehaviour
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
            if (_isX)
            {
                float newX = collision.collider.transform.position.x + _speed * Time.fixedDeltaTime;
                collision.collider.transform.position = new Vector3(newX, collision.collider.transform.position.y, 0);
            }
            else
            {
                float newY = collision.collider.transform.position.y + _speed * Time.fixedDeltaTime;
                collision.collider.transform.position = new Vector3(collision.collider.transform.position.x, newY, 0);
            }
        }
    }
}
