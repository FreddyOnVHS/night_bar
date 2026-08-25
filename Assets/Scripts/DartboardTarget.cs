using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple dartboard target: swaps between a clean sprite and a "hit" sprite
/// (darts stuck in) when the player interacts with it, then optionally
/// resets itself after a delay. Requires a Collider2D set to "Is Trigger"
/// on this object, and the player object to be tagged "Player".
/// </summary>
public class DartboardTarget : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite cleanSprite;
    [SerializeField] private Sprite hitSprite;

    [Header("Interaction")]
    [SerializeField] private Key interactKey = Key.E;

    [Header("Reset")]
    [SerializeField] private bool autoReset = true;
    [SerializeField] private float resetDelay = 2f;

    [Header("Debug")]
    [SerializeField] private bool debugLogging = true;

    private bool playerInRange;
    private bool isHit;
    private Coroutine resetRoutine;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        ResetBoard();
    }

    private void Update()
    {
        if (!playerInRange || isHit)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current[interactKey].wasPressedThisFrame)
        {
            if (debugLogging)
                Debug.Log($"[DartboardTarget] {interactKey} pressed while in range, hitting board.", this);

            Hit();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (debugLogging)
            Debug.Log($"[DartboardTarget] OnTriggerEnter2D from '{other.gameObject.name}' (tag: {other.tag})", this);

        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (debugLogging)
            Debug.Log($"[DartboardTarget] OnTriggerExit2D from '{other.gameObject.name}' (tag: {other.tag})", this);

        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    public void Hit()
    {
        if (isHit)
            return;

        isHit = true;

        if (spriteRenderer != null && hitSprite != null)
            spriteRenderer.sprite = hitSprite;

        if (autoReset)
        {
            if (resetRoutine != null)
                StopCoroutine(resetRoutine);

            resetRoutine = StartCoroutine(ResetAfterDelay());
        }
    }

    public void ResetBoard()
    {
        if (resetRoutine != null)
        {
            StopCoroutine(resetRoutine);
            resetRoutine = null;
        }

        isHit = false;

        if (spriteRenderer != null && cleanSprite != null)
            spriteRenderer.sprite = cleanSprite;
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);
        ResetBoard();
    }

    private void OnDrawGizmos()
    {
        var col = GetComponent<Collider2D>();
        if (col == null)
            return;

        Gizmos.color = playerInRange ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}
