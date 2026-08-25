using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;

    private string currentAnimation = "WalkDown";

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        movement = Vector2.zero;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.wKey.isPressed)
            movement.y += 1f;

        if (Keyboard.current.sKey.isPressed)
            movement.y -= 1f;

        if (Keyboard.current.aKey.isPressed)
            movement.x -= 1f;

        if (Keyboard.current.dKey.isPressed)
            movement.x += 1f;

        movement = movement.normalized;

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movement * moveSpeed;
    }

    private void UpdateAnimation()
    {
        if (movement == Vector2.zero)
        {
            animator.speed = 0f;
            return;
        }

        animator.speed = 1f;

        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
        {
            if (movement.x > 0)
                PlayAnimation("WalkRight");
            else
                PlayAnimation("WalkLeft");
        }
        else
        {
            if (movement.y > 0)
                PlayAnimation("WalkUp");
            else
                PlayAnimation("WalkDown");
        }
    }

    private void PlayAnimation(string animationName)
    {
        if (currentAnimation == animationName)
            return;

        currentAnimation = animationName;
        animator.Play(animationName);
    }
}