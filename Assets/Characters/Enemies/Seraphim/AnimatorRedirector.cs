using UnityEngine;

[DefaultExecutionOrder(-100)]
public class AnimatorRedirector : MonoBehaviour
{
    public Animator targetAnimator;

    void Awake()
    {
        if (targetAnimator == null) return;

        Animator anim = gameObject.GetComponent<Animator>();
        if (anim == null)
            anim = gameObject.AddComponent<Animator>();

        anim.runtimeAnimatorController = targetAnimator.runtimeAnimatorController;
        anim.avatar = targetAnimator.avatar;
    }
}
