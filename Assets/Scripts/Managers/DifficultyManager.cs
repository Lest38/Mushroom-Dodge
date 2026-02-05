using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Difficulty Curves")]
    [SerializeField] private AnimationCurve speedCurve;
    [SerializeField] private AnimationCurve spawnRateCurve;
    [SerializeField] private AnimationCurve countCurve;

    [Header("Speed Settings")]
    [SerializeField] private float minSpeedMultiplier = 1.2f;
    [SerializeField] private float maxSpeedMultiplier = 3f;
    [SerializeField] private float timeToMaxSpeed = 200f;

    [Header("Settings")]
    [SerializeField] private float maxTime = 260f;

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
        float t = currentTime / maxTime;
        float curveValue = speedCurve.Evaluate(t);

        if (t > 0.7f)
        {
            curveValue *= Mathf.Lerp(1f, 1.3f, (t - 0.7f) / 0.3f);
        }

        return curveValue;
    }

    public float GetProgressiveSpeedMultiplier()
    {
        if (timeToMaxSpeed <= 0) return minSpeedMultiplier;

        float progress = Mathf.Clamp01(currentTime / timeToMaxSpeed);

        float aggressiveProgress = Mathf.Pow(progress, 0.7f);
        return Mathf.Lerp(minSpeedMultiplier, maxSpeedMultiplier, aggressiveProgress);
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