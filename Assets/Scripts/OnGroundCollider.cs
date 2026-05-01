using UnityEngine;

public class OnGroundCollider : MonoBehaviour
{
    private bool _isOnGround = false;
    public bool IsOnGround => _isOnGround;

    void OnTriggerEnter(Collider collision)
    {
        _isOnGround = true;
    }

    void OnTriggerExit(Collider collision)
    {
        _isOnGround = false;
    }
}
