using System;
using UnityEngine;

// Cesta de composta: recibe las hojas sucias (y comida que ya no sirve) y las desecha.
public class CompostBin : Tile
{
    [Header("Composta")]
    // Serialized *****
    [SerializeField] private AudioClip compostAudio;

    // Public Methods *****
    public override void TakeAction(PlayerInteraction owner, Item playerItem, Action onActionComplete)
    {
        _onActionComplete = onActionComplete;

        if (!playerItem) return;
        if (!playerItem.TryGetComponent(out DirtyPlate _) && !playerItem.TryGetComponent(out Food _)) return;

        owner.RemoveItem();
        if (compostAudio) AudioManager.CreateSFX(compostAudio, 0.7f);
    }

    public override void ActionComplete()
    {
    }

    // Private Methods *****
    protected override void TakeAdvanceAction(PlayerInteraction owner)
    {
    }
}
