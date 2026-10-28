using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transporter : Tile
{
    // Serialized *****
    [SerializeField] private GameObject successVfxPrefab;
    [SerializeField] private AudioClip successAudio;
    [Header("Hojas sucias")]
    [SerializeField] private Item dirtyPlatePrefab;
    [SerializeField] private float dirtyPlateDelay = 10f;
    [SerializeField] private float dirtyPlateRetryTime = 2f;
    // Mesones donde regresan las hojas sucias, en orden de preferencia
    [SerializeField] private List<Tile> dirtyPlateTiles = new List<Tile>();

    // MonoBehavior Callbacks *****

    // Public Methods *****
    public override void ActionComplete()
    {
        _onActionComplete();
    }

    // Private Methods *****
    public override void TakeAction(PlayerInteraction owner, Item playerItem, Action onActionComplete)
    {
        _onActionComplete = onActionComplete;
        
        if (playerItem && playerItem.TryGetComponent(out Plate plate) && plate.IsCookedFood())
        {
            if (GrabItem(playerItem))
            {
                owner.DropItem();
                SuccessOrder();
            }
            
        }
    }

    protected override void TakeAdvanceAction(PlayerInteraction owner)
    {
        
    }
    private void SuccessOrder()
    {
        Debug.Log("Order success");
        GameManager.Instance.SuccessOrder();
        AudioManager.CreateSFX(successAudio, 0.7f);
        GameObject successVfx = Instantiate(successVfxPrefab, itemAnchor, false);
        successVfx.transform.localPosition = Vector3.zero;
        Destroy(_item.gameObject, 0.25f);
        DropItem();

        if (dirtyPlatePrefab) StartCoroutine(ReturnDirtyPlate());
    }

    // Despues de un tiempo la hoja usada regresa sucia a un meson libre
    private IEnumerator ReturnDirtyPlate()
    {
        yield return new WaitForSeconds(dirtyPlateDelay);

        while (true)
        {
            foreach (Tile tile in dirtyPlateTiles)
            {
                if (!tile) continue;

                Item dirtyPlate = Instantiate(dirtyPlatePrefab);
                if (tile.TryPlaceItem(dirtyPlate)) yield break;
                Destroy(dirtyPlate.gameObject);
            }
            // Todos los mesones estan ocupados: intentar de nuevo mas tarde
            yield return new WaitForSeconds(dirtyPlateRetryTime);
        }
    }
}
