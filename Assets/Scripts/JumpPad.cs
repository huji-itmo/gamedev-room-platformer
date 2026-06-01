using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField]
    float jumpStrengh;

    void OnCollisionEnter(Collision col)
    {
        Debug.Log("jumped");
        col.rigidbody.AddForce(Vector3.up * jumpStrengh, ForceMode.Impulse);
    }
}
