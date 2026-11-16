using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MenuMusicManager : MonoBehaviour
{
    public static MenuMusicManager instance;

    [Header("Calibración de Volumen")]
    [Tooltip("Volumen real del AudioSource cuando el slider de Ajustes está al 100%")]
    [Range(0.05f, 1f)]
    public float volumenMaximo = 0.35f;

    [Range(0f, 1f)]
    public float volumenUsuario = 1f; // Porcentaje del slider (1 = 100%)

    [Header("Escenas donde la música se mantiene")]
    public string[] escenasPermitidas = { "MainMenu", "MapScene", "Ajustes", "ExpBotones", "Instructions" };

    private AudioSource audioSource;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Por defecto arranca al 100% del slider (que equivale a 'volumenMaximo')
        volumenUsuario = PlayerPrefs.GetFloat("VolumenMusica", 1f);

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = true;
        audioSource.spatialBlend = 0f;
        AplicarVolumen();

        if (!audioSource.isPlaying && audioSource.clip != null)
        {
            audioSource.Play();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void SetVolumen(float porcentajeSlider)
    {
        volumenUsuario = Mathf.Clamp01(porcentajeSlider);
        AplicarVolumen();

        PlayerPrefs.SetFloat("VolumenMusica", volumenUsuario);
        PlayerPrefs.Save();
    }

    private void AplicarVolumen()
    {
        if (audioSource != null)
        {
            audioSource.volume = volumenUsuario * volumenMaximo;
        }
    }

    // Permite calibrar 'Volumen Maximo' en vivo desde el Inspector mientras juegas
    void OnValidate()
    {
        AplicarVolumen();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool mantenerMusica = false;

        foreach (string nombre in escenasPermitidas)
        {
            if (scene.name == nombre)
            {
                mantenerMusica = true;
                break;
            }
        }

        if (!mantenerMusica)
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }
}