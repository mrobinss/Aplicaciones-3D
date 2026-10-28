using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // Iniciar lleva al mapa de niveles
    public void PlayGame()
    {
        SceneHistory.Load("MapScene");
    }

    public void OpenSettings()
    {
        SceneHistory.LastScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("Ajustes");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}