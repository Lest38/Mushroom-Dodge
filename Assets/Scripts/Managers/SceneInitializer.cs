using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneInitializer : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"SceneInitializer: сцена {SceneManager.GetActiveScene().name} загружена");

        Invoke(nameof(InitializeManagers), 0.1f);
    }

    void InitializeManagers()
    {
        Debug.Log("Инициализация менеджеров...");

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.Reinitialize();
        }

        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.Reinitialize();
        }

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.StopSpawning();
        }

        Debug.Log("Менеджеры инициализированы");
    }
}