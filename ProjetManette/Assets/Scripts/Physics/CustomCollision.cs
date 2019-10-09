
using UnityEngine;

public struct CustomCollision
{
    public GameObject gameObject;
    public Collider2D collider;
    public Transform transform;

    public CustomCollision(GameObject collidingGameObject)
    {
        gameObject = collidingGameObject;
        collider = collidingGameObject.GetComponent<Collider2D>();
        transform = collidingGameObject.transform;
    }
}