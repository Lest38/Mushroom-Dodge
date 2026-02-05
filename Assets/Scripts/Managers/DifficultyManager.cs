using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Difficulty Curves")]
    [SerializeField] private AnimationCurve speedCurve;
    [SerializeField] private AnimationCurve spawnRateCurve;
    [SerializeField] private AnimationCurve countCurve;

    [Header("Speed Settings")]
    [SerializeField] private float minSpeedMultiplier = 1f;
    [SerializeField] private float maxSpeedMultiplier = 2.5f;
    [SerializeField] private float timeToMaxSpeed = 300f;

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

    public float GetSpeedMultiplier()
    {
        return speedCurve.Evaluate(currentTime / maxTime);
    }

    public float GetProgressiveSpeedMultiplier()
    {
        if (timeToMaxSpeed <= 0) return minSpeedMultiplier;

        float progress = Mathf.Clamp01(currentTime / timeToMaxSpeed);

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
}