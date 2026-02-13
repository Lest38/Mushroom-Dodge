using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float maxX = 8f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Shield Settings")]
    [SerializeField] private GameObject shieldVisual;

    [Header("Death Effect")]
    [SerializeField] private float fadeOutDuration = 0.5f;

    private float currentSpeed;
    private bool isDead;
    private bool hasShield;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();

        if (animator == null)
            animator = GetComponent<Animator>();

        ResetAnimationState();
        shieldVisual?.SetActive(false);
    }

    void OnEnable() => ResetAnimationState();

    void Update()
    {
        if (isDead) return;

        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning)
        {
            animator?.SetBool("IsRunning", false);
            return;
        }

        HandleMovement();
    }

    void HandleMovement()
    {
        float input = Input.GetAxisRaw("Horizontal");

        if (Mathf.Abs(input) > 0.1f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, input * moveSpeed, acceleration * Time.deltaTime);
            animator?.SetBool("IsRunning", true);
            transform.localScale = new Vector3(input > 0 ? 1 : -1, 1, 1);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
            animator?.SetBool("IsRunning", false);
        }

        Vector3 newPos = transform.position + Vector3.right * currentSpeed * Time.deltaTime;
        newPos.x = Mathf.Clamp(newPos.x, -maxX, maxX);
        transform.position = newPos;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Fireball") || isDead) return;

        if (hasShield)
        {
            UseShield();
            Destroy(other.gameObject);
            return;
        }

        Die();
    }

    IEnumerator FadeOutAndDisable()
    {
        if (spriteRenderer == null) yield break;

        Color startColor = spriteRenderer.color;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        gameObject.SetActive(false);
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        currentSpeed = 0f;
        animator?.SetBool("IsRunning", false);

        if (playerCollider != null)
            playerCollider.enabled = false;

        StartCoroutine(FadeOutAndDisable());
        GameManager.Instance?.PlayerDied();
    }

    public void EnableShield(float duration)
    {
        hasShield = true;
        shieldVisual?.SetActive(true);
        Invoke(nameof(DisableShield), duration);
    }

    void UseShield()
    {
        hasShield = false;
        shieldVisual?.SetActive(false);
    }

    void DisableShield()
    {
        hasShield = false;
        shieldVisual?.SetActive(false);
    }

    void ResetAnimationState()
    {
        if (animator == null) return;

        animator.Rebind();
        animator.Update(0f);
        animator.SetBool("IsRunning", false);
        animator.Play("Idle", 0, 0f);
    }

    public void ResetPlayer()
    {
        isDead = false;
        hasShield = false;
        currentSpeed = 0f;

        gameObject.SetActive(true);

        ResetAnimationState();

        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;

        if (playerCollider != null)
            playerCollider.enabled = true;

        transform.position = new Vector3(0, -3.5f, 0);
        transform.localScale = Vector3.one;

        shieldVisual?.SetActive(false);
        CancelInvoke(nameof(DisableShield));
    }
}