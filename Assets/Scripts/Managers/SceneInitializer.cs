using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneInitializer : MonoBehaviour
{
    void Start()
    {
        Invoke(nameof(InitializeManagers), 0.1f);
    }

    void InitializeManagers()
    {
        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.RefreshPowerUp();
        }

        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.Reinitialize();
        }

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.StopSpawning();
        }
    }
}