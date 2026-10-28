using System;
using System.Collections.Generic;
using UnityEngine;

// Estacion que entrega una copia nueva de un item cada vez que el jugador interactua
// con las manos vacias (por ejemplo, la pila de hojas que sirven de plato).
// Si maxStock > 0 solo tiene esa cantidad: se recupera con Restock (al compostar hojas sucias)
// o devolviendo un item limpio igual al dispensador.
public class ItemDispenser : Tile
{
    [Header("Dispenser")]
    // Serialized *****
    [SerializeField] private Item itemPrefab;
    [SerializeField] private int maxStock = 5;
    // Donde se apilan las copias visuales del item (si no hay, se usa el itemAnchor)
    [SerializeField] private Transform stackAnchor;
    [SerializeField] private float stackSpacing = 0.035f;

    // Private *****
    private readonly List<GameObject> _stackVisuals = new List<GameObject>();
    private int _stock;

    // MonoBehavior Callbacks *****
    protected override void Awake()
    {
        base.Awake();
        _stock = maxStock;
        RefreshStack();
    }

    // Public Methods *****
    public override void TakeAction(PlayerInteraction owner, Item playerItem, Action onActionComplete)
    {
        // Devolver una hoja limpia (sin comida) a la pila
        if (maxStock > 0 && playerItem && IsCleanCopy(playerItem) && _stock < maxStock)
        {
            _onActionComplete = onActionComplete;
            owner.RemoveItem();
            Restock(1);
            return;
        }
        base.TakeAction(owner, playerItem, onActionComplete);
    }

    public override void ActionComplete()
    {
    }

    public void Restock(int amount)
    {
        if (maxStock <= 0) return;

        _stock = Mathf.Min(maxStock, _stock + amount);
        RefreshStack();
    }

    // Private Methods *****
    protected override void TakeAdvanceAction(PlayerInteraction owner)
    {
        if (maxStock > 0 && _stock <= 0) return;

        Item newItem = Instantiate(itemPrefab);
        if (!owner.GrabItem(newItem))
        {
            Destroy(newItem.gameObject);
            return;
        }

        if (maxStock > 0)
        {
            _stock--;
            RefreshStack();
        }
    }

    private bool IsCleanCopy(Item item)
    {
        if (item.GetType() != itemPrefab.GetType()) return false;
        return !(item is Plate plate) || !plate.IsCookedFood();
    }

    // Muestra una copia del item por cada unidad disponible
    private void RefreshStack()
    {
        if (maxStock <= 0 || !itemPrefab) return;

        MeshFilter sourceFilter = itemPrefab.GetComponent<MeshFilter>();
        MeshRenderer sourceRenderer = itemPrefab.GetComponent<MeshRenderer>();
        if (!sourceFilter || !sourceRenderer) return;

        Transform anchor = stackAnchor ? stackAnchor : itemAnchor;
        while (_stackVisuals.Count < maxStock)
        {
            int index = _stackVisuals.Count;
            GameObject visual = new GameObject("PilaVisual", typeof(MeshFilter), typeof(MeshRenderer));
            visual.transform.SetParent(anchor, false);
            visual.transform.localPosition = Vector3.up * stackSpacing * index;
            visual.transform.localRotation = Quaternion.Euler(0, (index * 29f) % 40f - 20f, 0);
            visual.transform.localScale = itemPrefab.transform.localScale;
            visual.GetComponent<MeshFilter>().sharedMesh = sourceFilter.sharedMesh;
            visual.GetComponent<MeshRenderer>().sharedMaterials = sourceRenderer.sharedMaterials;
            _stackVisuals.Add(visual);
        }

        for (int i = 0; i < _stackVisuals.Count; i++)
        {
            _stackVisuals[i].SetActive(i < _stock);
        }
    }
}
