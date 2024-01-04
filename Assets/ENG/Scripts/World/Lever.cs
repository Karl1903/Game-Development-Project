using UnityEngine;
using Player;

public class Lever : MonoBehaviour {

    private Animator animator;
    private readonly int animationId = Animator.StringToHash("Getting Pulled");

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    public void Pull() {
        PlayerController.Current.PullLever(this);
    }

    public void StartPullAnimation() {
        animator.Play(animationId);
}
}
