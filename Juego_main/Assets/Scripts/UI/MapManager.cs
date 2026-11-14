using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Mapa de niveles. El primer nivel muestra los controles (ExpBotones) como pop-up; su boton
// Continuar lleva a las instrucciones y de ahi al nivel. Los demas niveles empiezan por sus
// instrucciones y solo se pueden jugar despues de ganar el anterior. Salir vuelve a la escena anterior.
public class MapManager : MonoBehaviour
{
    // Serialized
    [SerializeField] private string controlsPopupScene = "ExpBotones";
    [SerializeField] private string fallbackExitScene = "MainMenu";
    // Si se llego al mapa desde un nivel (por ejemplo, con "Volver al mapa" del resultado),
    // Salir lleva al menu principal en vez de volver a ese nivel
    [SerializeField] private string[] levelScenes = { "SampleScene", "Nivel2" };

    [Header("Nodos del mapa (en orden: nivel 1, nivel 2...)")]
    [SerializeField] private Button[] levelButtons;
    [SerializeField] private Sprite activeLevelSprite;
    [SerializeField] private Sprite lockedLevelSprite;

    [Header("Camino entre niveles (tramo 0 = nivel 1 -> 2, tramo 1 = nivel 2 -> 3...)")]
    // Cada tramo agrupa sus rayas en orden; aparece cuando se desbloquea el nivel al que llega
    [SerializeField] private RectTransform[] pathSegments;
    [SerializeField] private float dashDelay = 0.06f;
    [SerializeField] private float dashPopTime = 0.25f;

    private const string ShownSegmentsKey = "MapaTramosVistos";

    // MonoBehaviour Callbacks
    private void Start()
    {
        RefreshLevelButtons();
        RefreshPath();
    }

    // Public Methods
    public void PlayFirstLevel()
    {
        if (SceneManager.GetSceneByName(controlsPopupScene).isLoaded) return;
        SceneManager.LoadScene(controlsPopupScene, LoadSceneMode.Additive);
    }

    public void PlayLevel(int level)
    {
        if (!LevelProgress.IsUnlocked(level)) return;

        if (level == 1)
        {
            PlayFirstLevel();
            return;
        }
        Time.timeScale = 1f;
        SceneHistory.Load(LevelProgress.IntroScene(level));
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

    // Private Methods
    // Muestra los tramos hacia niveles desbloqueados. Los que se desbloquearon desde la ultima vez
    // que se abrio el mapa se dibujan raya por raya; los demas aparecen completos.
    private void RefreshPath()
    {
        if (pathSegments == null) return;

        int alreadyShown = PlayerPrefs.GetInt(ShownSegmentsKey, 0);
        int visible = 0;
        float delay = 0.3f;
        for (int i = 0; i < pathSegments.Length; i++)
        {
            RectTransform segment = pathSegments[i];
            if (!segment) continue;

            bool unlocked = LevelProgress.IsUnlocked(i + 2);
            segment.gameObject.SetActive(unlocked);
            if (!unlocked) continue;

            visible = i + 1;
            if (i < alreadyShown) continue;

            foreach (RectTransform dash in segment)
            {
                Vector3 baseScale = dash.localScale;
                dash.localScale = Vector3.zero;
                LeanTween.scale(dash, baseScale, dashPopTime).setDelay(delay).setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(true);
                delay += dashDelay;
            }
        }

        if (visible != alreadyShown)
        {
            PlayerPrefs.SetInt(ShownSegmentsKey, visible);
            PlayerPrefs.Save();
        }
    }

    // Los niveles desbloqueados se ven activos; los demas, bloqueados y sin poder pulsarse
    private void RefreshLevelButtons()
    {
        if (levelButtons == null) return;

        for (int i = 0; i < levelButtons.Length; i++)
        {
            Button button = levelButtons[i];
            if (!button) continue;

            bool unlocked = LevelProgress.IsUnlocked(i + 1);
            button.interactable = unlocked;
            if (button.image && activeLevelSprite && lockedLevelSprite)
            {
                button.image.sprite = unlocked ? activeLevelSprite : lockedLevelSprite;
            }
        }
    }
}
