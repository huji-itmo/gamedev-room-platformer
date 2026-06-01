using UnityEngine;

public class OnGroundCollider : MonoBehaviour
{
    public bool IsOnGround {
        get => shapesInside > 0;
    }

    public int shapesInside = 0;

    void OnTriggerEnter(Collider collision)
    {
        shapesInside++;
    }

    void OnTriggerExit(Collider collision)
    {
        shapesInside--;
    }
}
