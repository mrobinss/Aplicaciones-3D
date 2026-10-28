using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Efecto de iluminacion al pasar el mouse y de "presion" al hacer click.
// Usa tiempo sin escalar para que funcione aunque el juego este en pausa (timeScale = 0).
[RequireComponent(typeof(Button))]
public class ButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    // Serialized
    [Header("Hover (iluminacion)")]
    [SerializeField] private float hoverScale = 1.07f;
    [SerializeField] private Color haloColor = new Color(1f, 0.9f, 0.5f, 1f);
    [SerializeField, Range(0f, 1f)] private float haloAlpha = 0.2f;
    [Tooltip("Ancho del resplandor alrededor del boton. No depende de la escala del boton.")]
    [SerializeField] private float haloSize = 15f;
    [SerializeField, Range(0f, 0.5f)] private float brightenAlpha = 0.05f;

    [Header("Click")]
    [SerializeField] private float pressedScale = 0.92f;
    [SerializeField, Range(0f, 0.5f)] private float clickFlash = 0.3f;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    [Header("Animacion")]
    [SerializeField] private float speed = 14f;

    // private *****
    private const int HaloBorder = 60;
    private const float HaloSpreadRatio = 4f;
    // Radio de las esquinas del halo y margen transparente tipico de los sprites (en multiplos de haloSize)
    private const float HaloInsetRatio = 3f;
    private const float HaloMarginRatio = 1f;
    private const float HaloFalloff = 6f;
    private static Sprite _haloSprite;
    private Button _button;
    private RectTransform _rect;
    private Image _brighten;
    private Image _halo;
    private RectTransform _haloRect;
    private Vector3 _baseScale;
    private float _targetScale = 1f;
    private float _targetGlow;
    private float _glow;
    private float _flash;
    private bool _hovered;
    private bool _pressed;

    // Monobehavior Callbacks

    private void Awake()
    {
        _button = GetComponent<Button>();
        _rect = GetComponent<RectTransform>();
        _baseScale = _rect.localScale;
        CreateBrighten();
        CreateHalo();
    }

    private void OnDisable()
    {
        _hovered = false;
        _pressed = false;
        _flash = 0f;
        _glow = 0f;
        _targetScale = 1f;
        _targetGlow = 0f;
        _rect.localScale = _baseScale;
        ApplyGlow();
    }

    private void OnDestroy()
    {
        if (_halo != null) Destroy(_halo.gameObject);
    }

    private void Update()
    {
        float t = 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime);

        float scale = Mathf.Lerp(_rect.localScale.x / _baseScale.x, _targetScale, t);
        _rect.localScale = _baseScale * scale;

        _flash = Mathf.MoveTowards(_flash, 0f, Time.unscaledDeltaTime * 2f);
        _glow = Mathf.Lerp(_glow, _targetGlow, t);
        ApplyGlow();
    }

    private void LateUpdate()
    {
        if (_haloRect == null) return;

        // El halo es hermano del boton (para quedar detras), asi que sigue su posicion y tamano.
        // No copia la escala: el tamano visible del boton se pasa a sizeDelta para que el
        // difuminado mida lo mismo en todos los botones, sin importar como se escalaron.
        Vector3 scale = _rect.localScale;
        Vector2 visibleSize = Vector2.Scale(_rect.rect.size, new Vector2(Mathf.Abs(scale.x), Mathf.Abs(scale.y)));
        _haloRect.position = _rect.TransformPoint(_rect.rect.center);
        _haloRect.rotation = _rect.rotation;
        _haloRect.localScale = Vector3.one;
        // La cola tenue del resplandor se extiende mas alla de haloSize para que se desvanezca suave
        float spread = haloSize * HaloSpreadRatio;
        _haloRect.sizeDelta = visibleSize + Vector2.one * spread * 2f;

        // El nucleo del halo empieza por debajo del boton para que las esquinas queden redondeadas
        float inset = haloSize * HaloInsetRatio;
        Canvas canvas = _halo.canvas;
        float referencePpu = canvas != null ? canvas.referencePixelsPerUnit : 100f;
        float borderInUnits = HaloBorder * referencePpu / _halo.sprite.pixelsPerUnit;
        _halo.pixelsPerUnitMultiplier = borderInUnits / Mathf.Max(spread + inset, 1f);
    }

    // Pointer events

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_button.interactable) return;
        _hovered = true;
        PlaySound(hoverSound);
        Refresh();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hovered = false;
        _pressed = false;
        Refresh();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_button.interactable || eventData.button != PointerEventData.InputButton.Left) return;
        _pressed = true;
        _flash = clickFlash;
        PlaySound(clickSound);
        Refresh();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pressed = false;
        Refresh();
    }

    // private methods

    private void Refresh()
    {
        _targetScale = _pressed ? pressedScale : (_hovered ? hoverScale : 1f);
        _targetGlow = _hovered ? 1f : 0f;
    }

    private void ApplyGlow()
    {
        if (_brighten != null) _brighten.color = new Color(1f, 1f, 1f, _glow * brightenAlpha + _flash);
        if (_halo != null) _halo.color = new Color(haloColor.r, haloColor.g, haloColor.b, Mathf.Clamp01(_glow * haloAlpha + _flash));
    }

    // Copia exacta del sprite del boton encima de su imagen (sin desplazamiento) que lo aclara un poco
    private void CreateBrighten()
    {
        Image source = _button.targetGraphic as Image;
        if (source == null) source = GetComponent<Image>();
        if (source == null || source.sprite == null) return;

        GameObject brightenObject = new GameObject("HoverBrighten", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        brightenObject.transform.SetParent(source.transform, false);
        brightenObject.transform.SetAsFirstSibling();
        brightenObject.GetComponent<LayoutElement>().ignoreLayout = true;

        RectTransform brightenRect = brightenObject.GetComponent<RectTransform>();
        brightenRect.anchorMin = Vector2.zero;
        brightenRect.anchorMax = Vector2.one;
        brightenRect.offsetMin = Vector2.zero;
        brightenRect.offsetMax = Vector2.zero;

        _brighten = brightenObject.GetComponent<Image>();
        _brighten.sprite = source.sprite;
        _brighten.type = source.type;
        _brighten.preserveAspect = source.preserveAspect;
        _brighten.raycastTarget = false;
    }

    // Resplandor suave detras del boton
    private void CreateHalo()
    {
        if (_rect.parent == null) return;

        GameObject haloObject = new GameObject(name + "_HoverHalo", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        haloObject.transform.SetParent(_rect.parent, false);
        haloObject.transform.SetSiblingIndex(_rect.GetSiblingIndex());
        haloObject.GetComponent<LayoutElement>().ignoreLayout = true;

        _haloRect = haloObject.GetComponent<RectTransform>();
        _haloRect.anchorMin = _haloRect.anchorMax = _haloRect.pivot = new Vector2(0.5f, 0.5f);

        _halo = haloObject.GetComponent<Image>();
        _halo.sprite = GetHaloSprite();
        _halo.type = Image.Type.Sliced;
        _halo.raycastTarget = false;

        LateUpdate();
    }

    // Textura de resplandor generada por codigo: rectangulo redondeado con borde difuminado
    private static Sprite GetHaloSprite()
    {
        if (_haloSprite != null) return _haloSprite;

        const int size = 128;
        const int border = HaloBorder;
        float tail = Mathf.Exp(-HaloFalloff);
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(border - (x + 0.5f), (x + 0.5f) - (size - border), 0f);
                float dy = Mathf.Max(border - (y + 0.5f), (y + 0.5f) - (size - border), 0f);
                float d = Mathf.Sqrt(dx * dx + dy * dy) / border;

                // Distancia al borde del boton en multiplos de haloSize (0 = borde, negativo = adentro)
                float s = d * (HaloInsetRatio + HaloSpreadRatio) - HaloInsetRatio;

                // Caida exponencial desde el borde visible del dibujo (un poco dentro del rect, por su
                // margen transparente) hasta el final: intenso pegado al boton, casi imperceptible al final
                float u = Mathf.Clamp01((s + HaloMarginRatio) / (HaloSpreadRatio + HaloMarginRatio));
                float a = (Mathf.Exp(-HaloFalloff * u) - tail) / (1f - tail);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        texture.Apply();

        _haloSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        return _haloSprite;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        AudioManager.CreateSFX(clip, PlayerPrefs.GetFloat("VolEfectos", 1f));
    }
}
