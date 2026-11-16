using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager instance;

    [Header("Clips de Audio por Defecto")]
    public AudioClip sonidoHover;
    public AudioClip sonidoClickConfirmar; // Para Iniciar, Ajustes, Aceptar, Continuar
    public AudioClip sonidoClickSalir;     // Para Salir, Cancelar, Cerrar

    [Header("Calibración de Volumen")]
    [Range(0f, 1f)]
    public float volumenMaximoHover = 0.5f;
    [Range(0f, 1f)]
    public float volumenMaximoClick = 0.8f;

    [Header("Automatización")]
    [Tooltip("Añade UIButtonAudio automáticamente a todos los botones de cada escena")]
    public bool asignarAutomaticamenteABotones = true;

    [HideInInspector]
    public float volumenEfectos = 1f;

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

        volumenEfectos = PlayerPrefs.GetFloat("VolumenEfectos", 1f);

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        if (asignarAutomaticamenteABotones)
        {
            AsignarAudioABotonesEnEscena();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (asignarAutomaticamenteABotones)
        {
            AsignarAudioABotonesEnEscena();
        }
    }

    public void AsignarAudioABotonesEnEscena()
    {
        // El parámetro 'true' incluye botones que estén desactivados (ej. menús de pausa o victoria)
        Button[] botones = FindObjectsOfType<Button>(true);

        foreach (Button btn in botones)
        {
            // Si el botón ya tiene UIButtonAudio (porque lo configuraste manual), lo respeta
            if (btn.GetComponent<UIButtonAudio>() != null) continue;

            UIButtonAudio audioBtn = btn.gameObject.AddComponent<UIButtonAudio>();

            // Detecta por el nombre del objeto si es un botón de salir/cancelar
            string nombre = btn.gameObject.name.ToLower();
            if (nombre.Contains("salir") || nombre.Contains("exit") || nombre.Contains("cerrar") || nombre.Contains("cancel"))
            {
                audioBtn.tipoBoton = UIButtonAudio.TipoBoton.Salir;
            }
            else
            {
                audioBtn.tipoBoton = UIButtonAudio.TipoBoton.Confirmar;
            }
        }
    }

    public void ReproducirHover(AudioClip clipPersonalizado = null)
    {
        AudioClip clip = clipPersonalizado != null ? clipPersonalizado : sonidoHover;
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volumenMaximoHover * volumenEfectos);
        }
    }

    public void ReproducirClickConfirmar(AudioClip clipPersonalizado = null)
    {
        AudioClip clip = clipPersonalizado != null ? clipPersonalizado : sonidoClickConfirmar;
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volumenMaximoClick * volumenEfectos);
        }
    }

    public void ReproducirClickSalir(AudioClip clipPersonalizado = null)
    {
        AudioClip clip = clipPersonalizado != null ? clipPersonalizado : sonidoClickSalir;
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volumenMaximoClick * volumenEfectos);
        }
    }

    public void SetVolumenEfectos(float porcentajeSlider)
    {
        volumenEfectos = Mathf.Clamp01(porcentajeSlider);
        PlayerPrefs.SetFloat("VolumenEfectos", volumenEfectos);
        PlayerPrefs.Save();
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