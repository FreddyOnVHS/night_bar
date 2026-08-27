using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;

    private string currentAnimation = "WalkDownIso";

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

        // Diagonals first
        if (movement.x > 0 && movement.y > 0)
        {
            PlayAnimation("WalkUpRightIso");
        }
        else if (movement.x < 0 && movement.y > 0)
        {
            PlayAnimation("WalkUpLeftIso");
        }
        else if (movement.x > 0 && movement.y < 0)
        {
            PlayAnimation("WalkDownRightIso");
        }
        else if (movement.x < 0 && movement.y < 0)
        {
            PlayAnimation("WalkDownLeftIso");
        }

        // Straight directions
        else if (movement.y > 0)
        {
            PlayAnimation("WalkUpIso");
        }
        else if (movement.y < 0)
        {
            PlayAnimation("WalkDownIso");
        }
        else if (movement.x > 0)
        {
            PlayAnimation("WalkRightIso");
        }
        else if (movement.x < 0)
        {
            PlayAnimation("WalkLeftIso");
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