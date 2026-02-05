using UnityEngine;

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

    private float currentSpeed = 0f;
    private bool isDead = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Автоматически находим Animator если не назначен
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
    }

    void Update()
    {
        if (isDead) return;

        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning)
        {
            // Вне игры - анимация покоя
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
            // Движение
            currentSpeed = Mathf.MoveTowards(currentSpeed, input * moveSpeed, acceleration * Time.deltaTime);

            // Анимация бега
            if (animator != null)
            {
                animator.SetBool("IsRunning", true);
            }

            // Поворот в сторону движения
            if (input > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
                // Для спрайтов можно также использовать:
                // spriteRenderer.flipX = false;
            }
            else if (input < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
                // spriteRenderer.flipX = true;
            }
        }
        else
        {
            // Остановка
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);

            // Анимация покоя
            if (animator != null)
            {
                animator.SetBool("IsRunning", false);
            }
        }

        // Применяем движение
        Vector3 newPos = transform.position + Vector3.right * currentSpeed * Time.deltaTime;
        newPos.x = Mathf.Clamp(newPos.x, -maxX, maxX);
        transform.position = newPos;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Fireball") && !isDead)
        {
            // Проверяем есть ли щит
            if (hasShield)
            {
                // Используем щит
                UseShield();
                Destroy(other.gameObject);
                return;
            }

            Die();
        }
    }

    public void EnableShield(float duration)
    {
        hasShield = true;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(true);
        }

        Debug.Log("Щит активирован!");

        // Автоматически отключаем щит через duration секунд
        Invoke("DisableShield", duration);
    }

    void UseShield()
    {
        hasShield = false;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }

        Debug.Log("Щит использован для защиты!");
    }

    void DisableShield()
    {
        hasShield = false;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }

        Debug.Log("Щит закончился");
    }

    void Die()
    {
        isDead = true;

        // Анимация смерти
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        else
        {
            // Визуальный эффект если нет аниматора
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.red;
            }
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDied();
        }
    }

    public void ResetPlayer()
    {
        isDead = false;

        // Сброс анимации
        if (animator != null)
        {
            animator.SetTrigger("Reset");
            animator.SetBool("IsRunning", false);
        }

        // Сброс визуала
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        transform.position = new Vector3(0, -3.5f, 0);
        transform.localScale = new Vector3(1, 1, 1);
    }
}
