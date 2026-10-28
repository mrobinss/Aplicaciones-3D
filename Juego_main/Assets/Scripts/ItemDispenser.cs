using UnityEngine;

// Estacion que entrega una copia nueva de un item cada vez que el jugador interactua
// con las manos vacias (por ejemplo, la pila infinita de hojas que sirven de plato).
public class ItemDispenser : Tile
{
    [Header("Dispenser")]
    // Serialized *****
    [SerializeField] private Item itemPrefab;

    // Public Methods *****
    public override void ActionComplete()
    {
    }

    // Private Methods *****
    protected override void TakeAdvanceAction(PlayerInteraction owner)
    {
        Item newItem = Instantiate(itemPrefab);
        if (!owner.GrabItem(newItem)) Destroy(newItem.gameObject);
    }
}
