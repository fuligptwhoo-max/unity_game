using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartGame : MonoBehaviour
{
    public void RestartCurrentScene()
    {
        Debug.Log("Рестарт сцены");
        
        // Сбрасываем прогресс перед перезагрузкой сцены
        ResetProgress();
        
        // Перезагружаем текущую сцену
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    // Метод: Сброс прогресса (аналогично F5 в LevelManager)
    public void ResetProgress()
    {
        Debug.Log("Сброс прогресса через RestartGame");
        
        // Ищем LevelManager новым методом
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            // Вызываем метод ResetProgress из LevelManager
            levelManager.ResetProgress();
            Debug.Log("Прогресс сброшен через LevelManager");
        }
        else
        {
            // Fallback: прямой сброс если LevelManager не найден
            PlayerPrefs.DeleteKey("CurrentSegment");
            PlayerPrefs.DeleteKey("GameFlags");
            PlayerPrefs.Save();
            Debug.Log("Прогресс сброшен напрямую (LevelManager не найден)");
        }
    }
    
    // Метод: Только сброс прогресса без перезагрузки сцены
    public void ResetProgressOnly()
    {
        ResetProgress();
        Debug.Log("Прогресс сброшен без перезагрузки сцены");
    }
}