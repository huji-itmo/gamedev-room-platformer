using UnityEngine;

public class AnimatorParameterController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;

    [Header("Animator Parameters")]
    public float inputSpeed;
    public bool floating;
    public bool holdAttack;

    private int inputSpeedHash;
    private int floatingHash;
    private int holdAttackHash;

    void Awake()
    {
        inputSpeedHash = Animator.StringToHash("inputSpeed");
        floatingHash = Animator.StringToHash("floating");
        holdAttackHash = Animator.StringToHash("holdAttack");
    }

    void Update()
    {
        if (animator == null) return;

        animator.SetFloat(inputSpeedHash, inputSpeed);
        animator.SetBool(floatingHash, floating);
        animator.SetBool(holdAttackHash, holdAttack);
    }
}