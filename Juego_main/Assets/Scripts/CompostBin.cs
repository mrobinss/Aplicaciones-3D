using System;
using UnityEngine;

// Cesta de composta: recibe las hojas sucias (y comida que ya no sirve) y las desecha.
// Cada cosa que se bota sube poco a poco el nivel de composta dentro de la cesta.
public class CompostBin : Tile
{
    [Header("Composta")]
    // Serialized *****
    [SerializeField] private AudioClip compostAudio;
    // Dispensador de hojas limpias que se recupera al compostar hojas sucias
    [SerializeField] private ItemDispenser plateDispenser;

    [Header("Llenado")]
    // Caja que representa la composta; su base queda en emptyHeight y su tope sube hasta fullHeight
    [SerializeField] private Transform fillVisual;
    [SerializeField] private float emptyHeight = 0.05f;
    [SerializeField] private float fullHeight = 0.95f;
    [SerializeField] private int itemsToFill = 40;
    [SerializeField] private float fillSpeed = 0.15f;

    // Private *****
    private Renderer _fillRenderer;
    private int _itemsComposted;
    private float _currentLevel;

    // MonoBehavior Callbacks *****
    protected override void Awake()
    {
        base.Awake();
        if (fillVisual) _fillRenderer = fillVisual.GetComponent<Renderer>();
        ApplyLevel();
    }

    private void Update()
    {
        float target = Mathf.Clamp01((float)_itemsComposted / itemsToFill);
        if (Mathf.Approximately(_currentLevel, target)) return;

        _currentLevel = Mathf.MoveTowards(_currentLevel, target, fillSpeed * Time.deltaTime);
        ApplyLevel();
    }

    // Public Methods *****
    public override void TakeAction(PlayerInteraction owner, Item playerItem, Action onActionComplete)
    {
        _onActionComplete = onActionComplete;

        if (!playerItem) return;

        int composted = 1;

        // Olla (por ejemplo con comida quemada): se vacia y el jugador se queda con la olla
        if (playerItem.TryGetComponent(out Cookware cookware))
        {
            if (!cookware.TryEmpty()) return;
        }
        // Hojas sucias: cada una compostada devuelve una hoja limpia al dispensador
        else if (playerItem.TryGetComponent(out DirtyPlate dirtyPlate))
        {
            composted = dirtyPlate.Count;
            owner.RemoveItem();
            if (plateDispenser) plateDispenser.Restock(composted);
        }
        else if (playerItem.TryGetComponent(out Food _))
        {
            owner.RemoveItem();
        }
        else
        {
            return;
        }

        _itemsComposted += composted;
        if (compostAudio) AudioManager.CreateSFX(compostAudio, 0.7f);
    }

    public override void ActionComplete()
    {
    }

    // Private Methods *****
    protected override void TakeAdvanceAction(PlayerInteraction owner)
    {
    }

    private void ApplyLevel()
    {
        if (!fillVisual) return;

        float height = Mathf.Lerp(0f, fullHeight - emptyHeight, _currentLevel);
        if (_fillRenderer) _fillRenderer.enabled = height > 0.001f;

        Vector3 scale = fillVisual.localScale;
        fillVisual.localScale = new Vector3(scale.x, Mathf.Max(height, 0.001f), scale.z);
        Vector3 position = fillVisual.localPosition;
        fillVisual.localPosition = new Vector3(position.x, emptyHeight + height / 2f, position.z);
    }
}
