using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Difficulty Curves")]
    [SerializeField] private AnimationCurve speedCurve;
    [SerializeField] private AnimationCurve spawnRateCurve;
    [SerializeField] private AnimationCurve countCurve;

    [Header("Speed Settings")]
    [SerializeField] private float minSpeedMultiplier = 1f;    // минимальный множитель (начало)
    [SerializeField] private float maxSpeedMultiplier = 2.5f;  // максимальный множитель (через 5 минут)
    [SerializeField] private float timeToMaxSpeed = 300f;      // время до максимума (5 минут = 300 сек)

    [Header("Settings")]
    [SerializeField] private float maxTime = 300f;

    private float currentTime = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateDifficulty(float gameTime)
    {
        currentTime = Mathf.Min(gameTime, maxTime);
    }

    // Этот метод используется в Fireball.cs сейчас
    public float GetSpeedMultiplier()
    {
        return speedCurve.Evaluate(currentTime / maxTime);
    }

    // НОВЫЙ МЕТОД: Плавное увеличение скорости со временем
    public float GetProgressiveSpeedMultiplier()
    {
        if (timeToMaxSpeed <= 0) return minSpeedMultiplier;

        // Прогресс от 0 до 1
        float progress = Mathf.Clamp01(currentTime / timeToMaxSpeed);

        // С ускорением (рекомендуется)
        float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
        return Mathf.Lerp(minSpeedMultiplier, maxSpeedMultiplier, smoothProgress);
    }

    public float GetSpawnRateMultiplier()
    {
        return spawnRateCurve.Evaluate(currentTime / maxTime);
    }

    public int GetMaxFireballs()
    {
        return Mathf.FloorToInt(countCurve.Evaluate(currentTime / maxTime));
    }

    // Для UI: получить процент скорости (0% до 100%)
    public float GetCurrentSpeedPercent()
    {
        float multiplier = GetProgressiveSpeedMultiplier();
        float progress = (multiplier - minSpeedMultiplier) / (maxSpeedMultiplier - minSpeedMultiplier);
        return Mathf.Clamp01(progress) * 100f;
    }

    // Для UI: получить текущий множитель
    public float GetCurrentMultiplier()
    {
        return GetProgressiveSpeedMultiplier();
    }
}