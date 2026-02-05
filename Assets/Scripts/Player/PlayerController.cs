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
    private bool hasShield = false;

    [Header("Death Effect")]
    [SerializeField] private float fadeOutDuration = 0.5f;

    private float currentSpeed = 0f;
    private bool isDead = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("Animator не найден на Player!");
            }
        }

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }

        if (animator != null)
        {
            animator.SetBool("IsRunning", false);
        }
    }

    void Update()
    {
        if (isDead) return;

        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning)
        {
            if (animator != null)
            {
                animator.SetBool("IsRunning", false);
            }
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

            if (animator != null)
            {
                animator.SetBool("IsRunning", true);
            }

            if (input > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else if (input < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);

            if (animator != null)
            {
                animator.SetBool("IsRunning", false);
            }
        }

        Vector3 newPos = transform.position + Vector3.right * currentSpeed * Time.deltaTime;
        newPos.x = Mathf.Clamp(newPos.x, -maxX, maxX);
        transform.position = newPos;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Fireball") && !isDead)
        {
            if (hasShield)
            {
                UseShield();
                Destroy(other.gameObject);
                return;
            }

            Die();
        }
    }

    private IEnumerator FadeOutAndDisable()
    {
        if (spriteRenderer == null) yield break;

        Color originalColor = spriteRenderer.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        gameObject.SetActive(false);
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        currentSpeed = 0f;

        if (animator != null)
        {
            animator.SetBool("IsRunning", false);
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        StartCoroutine(FadeOutAndDisable());

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDied();
        }
    }

    public void EnableShield(float duration)
    {
        hasShield = true;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(true);
        }

        Debug.Log("ўит активирован!");

        Invoke("DisableShield", duration);
    }

    void UseShield()
    {
        hasShield = false;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }

        Debug.Log("ўит использован дл€ защиты!");
    }

    void DisableShield()
    {
        hasShield = false;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }

        Debug.Log("ўит закончилс€");
    }

    public void ResetPlayer()
    {
        isDead = false;

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (animator != null)
        {
            animator.Rebind();

            animator.SetBool("IsRunning", false);

            animator.Play("Idle", 0, 0f);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }

        transform.position = new Vector3(0, -3.5f, 0);
        transform.localScale = new Vector3(1, 1, 1);

        currentSpeed = 0f;

        CancelInvoke("DisableShield");

        hasShield = false;
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }

        Debug.Log("»грок сброшен!");
    }
}