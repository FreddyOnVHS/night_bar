using UnityEngine;
using System.Collections;

public class BartenderPatrol : MonoBehaviour
{
    [Header("Patrol")]
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 1.2f;

    [Header("Timing")]
    public float minPourTime = 1f;
    public float maxPourTime = 2f;

    private Animator animator;
    private Transform currentTarget;
    private bool isPouring;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        ChooseRandomTarget();
    }

    private void Update()
    {
        if (isPouring || currentTarget == null)
            return;

        Vector2 current = transform.position;
        Vector2 target = currentTarget.position;

        transform.position = Vector2.MoveTowards(
            current,
            target,
            moveSpeed * Time.deltaTime
        );

        if (target.x > current.x)
            animator.Play("Bartender_Walk_NE");
        else
            animator.Play("Bartender_Walk_SW");

        if (Vector2.Distance(transform.position, target) < 0.02f)
            StartCoroutine(PourAndChooseNextTarget());
    }

    private IEnumerator PourAndChooseNextTarget()
    {
        isPouring = true;

        animator.Play("Bartender_Pour");

        float waitTime = Random.Range(minPourTime, maxPourTime);
        yield return new WaitForSeconds(waitTime);

        ChooseRandomTarget();

        isPouring = false;
    }

    private void ChooseRandomTarget()
    {
        if (pointA == null || pointB == null)
            return;

        currentTarget = Random.value < 0.5f ? pointA : pointB;

        // Avoid choosing the point we're already standing on.
        if (Vector2.Distance(transform.position, currentTarget.position) < 0.05f)
        {
            currentTarget = currentTarget == pointA ? pointB : pointA;
        }
    }
}