using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Escena que se muestra como pop-up encima de otra (cargada en modo aditivo), por ejemplo
// WinScene / LoseScene sobre el nivel. Opcionalmente pausa el juego que queda detras.
// Se ejecuta antes que los demas scripts para apagar el EventSystem duplicado antes de que se active.
[DefaultExecutionOrder(-1000)]
public class PopupScene : MonoBehaviour
{
    // Serialized
    [SerializeField] private Canvas popupCanvas;
    [SerializeField] private RectTransform popupPanel;
    [SerializeField] private int sortingOrder = 100;
    [SerializeField] private bool pauseGame = true;

    // MonoBehaviour Callbacks
    private void Awake()
    {
        bool isPopup = SceneManager.sceneCount > 1;
        if (isPopup)
        {
            RemoveDuplicates();
            if (pauseGame) Time.timeScale = 0f;
        }

        if (popupCanvas) popupCanvas.sortingOrder = sortingOrder;

        if (popupPanel)
        {
            Vector3 baseScale = popupPanel.localScale;
            popupPanel.localScale = baseScale * 0.6f;
            LeanTween.scale(popupPanel, baseScale, 0.35f).setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(true);
        }
    }

    // Private Methods
    // La escena de abajo ya tiene su EventSystem y su luz: apagar los de esta escena
    private void RemoveDuplicates()
    {
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            if (root.TryGetComponent(out EventSystem _) || root.TryGetComponent(out Light _)) root.SetActive(false);
        }
    }
}
