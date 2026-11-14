using UnityEngine;

// Botones de WinScene / LoseScene que dependen del nivel que se acaba de jugar.
public class ResultActions : MonoBehaviour
{
    // Serialized
    [SerializeField] private string mapScene = "MapScene";

    // Public Methods
    // Volver a intentar: carga otra vez el nivel jugado
    public void RetryLevel()
    {
        Time.timeScale = 1f;
        SceneHistory.Load(LevelProgress.LevelScene(LevelProgress.CurrentLevel));
    }

    // Siguiente nivel: empieza por sus instrucciones; si no hay mas niveles, vuelve al mapa
    public void NextLevel()
    {
        Time.timeScale = 1f;
        int next = LevelProgress.CurrentLevel + 1;
        SceneHistory.Load(LevelProgress.HasLevel(next) ? LevelProgress.IntroScene(next) : mapScene);
    }
}
