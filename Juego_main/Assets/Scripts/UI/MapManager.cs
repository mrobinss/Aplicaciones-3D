using UnityEngine;
using UnityEngine.SceneManagement;

// Mapa de niveles. El primer nivel muestra los controles (ExpBotones) como pop-up; su boton
// Continuar lleva a las instrucciones y de ahi al nivel. Salir vuelve a la escena anterior.
public class MapManager : MonoBehaviour
{
    // Serialized
    [SerializeField] private string controlsPopupScene = "ExpBotones";
    [SerializeField] private string fallbackExitScene = "MainMenu";
    // Si se llego al mapa desde un nivel (por ejemplo, con "Volver al mapa" del resultado),
    // Salir lleva al menu principal en vez de volver a ese nivel
    [SerializeField] private string[] levelScenes = { "SampleScene" };

    // Public Methods
    public void PlayFirstLevel()
    {
        if (SceneManager.GetSceneByName(controlsPopupScene).isLoaded) return;
        SceneManager.LoadScene(controlsPopupScene, LoadSceneMode.Additive);
    }

    public void Exit()
    {
        string target = SceneHistory.LastScene;
        if (string.IsNullOrEmpty(target) || target == SceneManager.GetActiveScene().name || System.Array.IndexOf(levelScenes, target) >= 0)
        {
            target = fallbackExitScene;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(target);
    }
}
