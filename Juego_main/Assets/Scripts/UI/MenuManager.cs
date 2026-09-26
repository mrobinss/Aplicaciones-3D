using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene");
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