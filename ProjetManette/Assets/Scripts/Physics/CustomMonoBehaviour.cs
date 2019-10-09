using UnityEngine;

/// <summary>
/// Adds functions to manage collisions with objects using CustomPhysics. Behaviour is similar
/// to Unity's collision functions.
/// </summary>
public abstract class CustomMonoBehaviour : MonoBehaviour
{
    public virtual void OnCustomCollisionEnter(CustomCollision collision)
    {}

    public virtual void OnCustomCollisionStay(CustomCollision collision)
    {}

    public virtual void OnCustomCollisionExit(CustomCollision collision)
    {}
}