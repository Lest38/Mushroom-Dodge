using UnityEngine;

public class Fireball : MonoBehaviour
{
    [Header("Basic Settings")]
    public float minSpeed = 3f;
    public float maxSpeed = 8f;
    public float rotationSpeed = 100f;
    public ParticleSystem trailParticles;
    public ParticleSystem explosionParticles;

    [Header("Speed Settings")]
    public bool useProgressiveSpeed = true;

    [Header("Color Settings")]
    public bool changeColorBySpeed = true;
    public Color slowColor = Color.yellow;
    public Color mediumColor = new Color(1f, 0.5f, 0f); // Orange
    public Color fastColor = Color.red;

    [Header("Size Settings")]
    public bool changeSizeBySpeed = true;
    public float minSizeMultiplier = 0.7f;

    [Header("Thresholds")]
    public float slowThreshold = 6f;
    public float mediumThreshold = 10f;
    public float fastThreshold = 14f;

    // Приватные поля
    private float currentSpeed;
    private bool hasCollided = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Получаем множитель скорости
        float speedMultiplier = 1f;

        if (useProgressiveSpeed && DifficultyManager.Instance != null)
        {
            speedMultiplier = DifficultyManager.Instance.GetProgressiveSpeedMultiplier();
        }
        else if (DifficultyManager.Instance != null)
        {
            speedMultiplier = DifficultyManager.Instance.GetSpeedMultiplier();
        }

        // Устанавливаем скорость
        float baseSpeed = Random.Range(minSpeed, maxSpeed);
        currentSpeed = baseSpeed * speedMultiplier;

        Debug.Log($"Fireball создан: база={baseSpeed:F1}, множитель=x{speedMultiplier:F2}, итог={currentSpeed:F1}");

        // Настраиваем визуал
        UpdateVisuals();

        // Случайный размер
        float randomScale = Random.Range(0.9f, 1.1f);
        transform.localScale = Vector3.one * randomScale;
    }

    void Update()
    {
        if (hasCollided) return;

        // Движение
        transform.Translate(Vector3.down * currentSpeed * Time.deltaTime, Space.World);
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // Уничтожение если упал
        if (transform.position.y < -7f)
        {
            Destroy(gameObject);
        }
    }

    void UpdateVisuals()
    {
        if (spriteRenderer == null) return;

        // Цвет
        if (changeColorBySpeed)
        {
            if (currentSpeed < slowThreshold)
                spriteRenderer.color = slowColor;
            else if (currentSpeed < mediumThreshold)
                spriteRenderer.color = mediumColor;
            else
                spriteRenderer.color = fastColor;
        }

        // Размер
        if (changeSizeBySpeed && currentSpeed > mediumThreshold)
        {
            float t = Mathf.Clamp01((currentSpeed - mediumThreshold) / (fastThreshold - mediumThreshold));
            float sizeMultiplier = Mathf.Lerp(1f, minSizeMultiplier, t);
            transform.localScale *= sizeMultiplier;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasCollided || !other.CompareTag("Player")) return;

        hasCollided = true;

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (explosionParticles != null)
            explosionParticles.Play();

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;

        Destroy(gameObject, 1f);
    }
}