using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Escena ExpBotones mostrada como pop-up encima del menu principal (se carga en modo aditivo).
// Al darle Continuar se pasa a la escena de instrucciones.
// Se ejecuta antes que los demas scripts para apagar el EventSystem duplicado antes de que se active.
[DefaultExecutionOrder(-1000)]
public class ControlsPopup : MonoBehaviour
{
    // Serialized
    [SerializeField] private Canvas popupCanvas;
    [SerializeField] private RectTransform popupPanel;
    [SerializeField] private string nextScene = "InstructionsScene";
    [SerializeField] private int sortingOrder = 100;

    // MonoBehaviour Callbacks
    private void Awake()
    {
        bool isPopup = SceneManager.sceneCount > 1;
        if (isPopup) RemoveDuplicates();

        if (popupCanvas)
        {
            popupCanvas.overrideSorting = true;
            popupCanvas.sortingOrder = sortingOrder;
        }

        if (popupPanel)
        {
            popupPanel.localScale = Vector3.one * 0.6f;
            LeanTween.scale(popupPanel, Vector3.one, 0.35f).setEase(LeanTweenType.easeOutBack);
        }
    }

    // Public Methods
    public void Continue()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextScene);
    }

    // Private Methods
    // Como pop-up, la escena del menu ya tiene su EventSystem y su luz: quitar los de esta escena
    private void RemoveDuplicates()
    {
        Scene ownScene = gameObject.scene;
        foreach (GameObject root in ownScene.GetRootGameObjects())
        {
            if (root.TryGetComponent(out EventSystem _) || root.TryGetComponent(out Light _)) root.SetActive(false);
        }
    }
}
