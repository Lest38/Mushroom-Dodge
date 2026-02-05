using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneInitializer : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"SceneInitializer: сцена {SceneManager.GetActiveScene().name} загружена");

        // Ждем один кадр чтобы все Awake() выполнились
        Invoke(nameof(InitializeManagers), 0.1f);
    }

    void InitializeManagers()
    {
        Debug.Log("Инициализация менеджеров...");

        // Реинициализируем все менеджеры
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
            // Останавливаем спавн на всякий случай
            SpawnManager.Instance.StopSpawning();
        }

        Debug.Log("Менеджеры инициализированы");
    }
}