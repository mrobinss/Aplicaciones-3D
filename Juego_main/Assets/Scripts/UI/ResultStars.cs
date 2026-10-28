using UnityEngine;
using UnityEngine.UI;

// Rellena las estrellas de WinScene / LoseScene segun el resultado del ultimo nivel.
public class ResultStars : MonoBehaviour
{
    // Serialized
    [SerializeField] private Image[] stars;
    [SerializeField] private Sprite filledStar;
    [SerializeField] private Sprite emptyStar;
    [SerializeField] private float popDelay = 0.25f;

    // MonoBehaviour Callbacks
    private void Start()
    {
        // Si la escena se abre sola (sin jugar un nivel) se deja como esta en el editor
        if (!LevelResult.HasResult) return;

        for (int i = 0; i < stars.Length; i++)
        {
            bool filled = i < LevelResult.Stars;
            stars[i].sprite = filled ? filledStar : emptyStar;

            if (!filled) continue;
            RectTransform rect = stars[i].rectTransform;
            Vector3 baseScale = rect.localScale;
            rect.localScale = Vector3.zero;
            LeanTween.scale(rect, baseScale, 0.4f).setDelay(popDelay * (i + 1)).setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(true);
        }
    }
}
