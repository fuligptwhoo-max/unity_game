using UnityEngine;

public class RestartGame : MonoBehaviour
{
    public void RestartCurrentScene()
    {
        Debug.Log("Рестарт сцены");
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}