using UnityEngine;
using UnityEngine.SceneManagement;

// Para conectar botones desde el Inspector: On Click () -> SceneLoader.LoadScene("NombreDeLaEscena")
public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneHistory.Load(sceneName);
    }
}
