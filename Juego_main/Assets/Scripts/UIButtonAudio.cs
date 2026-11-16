using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonAudio : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    public enum TipoBoton
    {
        Confirmar, // Iniciar, Ajustes, Aceptar
        Salir      // Salir, Cerrar
    }

    [Header("Configuración del Botón")]
    public TipoBoton tipoBoton = TipoBoton.Confirmar;

    [Header("Opcional: Clips exclusivos para este botón")]
    public AudioClip hoverPersonalizado;
    public AudioClip clickPersonalizado;

    private Button boton;

    void Awake()
    {
        boton = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (boton != null && !boton.interactable) return;

        if (UIAudioManager.instance != null)
        {
            UIAudioManager.instance.ReproducirHover(hoverPersonalizado);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (boton != null && !boton.interactable) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (UIAudioManager.instance != null)
        {
            if (tipoBoton == TipoBoton.Confirmar)
            {
                UIAudioManager.instance.ReproducirClickConfirmar(clickPersonalizado);
            }
            else
            {
                UIAudioManager.instance.ReproducirClickSalir(clickPersonalizado);
            }
        }
    }
}