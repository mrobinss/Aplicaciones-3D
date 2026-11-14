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
        
        // Solo se recibe un plato que este pidiendo alguna tarjeta en pantalla (una hoja con el plato a
        // medio armar, o un plato que nadie pidio, no sirve)
        if (playerItem && playerItem.TryGetComponent(out Plate plate) && plate.IsCookedFood() && GameManager.Instance.IsOrderDish(plate.GetDish()) &&
            (!OrdersUI.Instance || OrdersUI.Instance.WantsDish(plate.GetDish())))
        {
            if (GrabItem(playerItem))
            {
                owner.DropItem();
                SuccessOrder(plate.GetDish());
            }
            
        }
    }

    protected override void TakeAdvanceAction(PlayerInteraction owner)
    {
        
    }
    private void SuccessOrder(FoodData dish)
    {
        Debug.Log("Order success");
        // Liberar la ventana de entrega primero: si algo falla mas adelante, la estacion no queda trabada
        Item deliveredPlate = _item;
        DropItem();
        Destroy(deliveredPlate.gameObject, 0.25f);

        AudioManager.CreateSFX(successAudio, 0.7f);
        GameObject successVfx = Instantiate(successVfxPrefab, itemAnchor, false);
        successVfx.transform.localPosition = Vector3.zero;
        if (dirtyPlatePrefab) StartCoroutine(ReturnDirtyPlate());

        GameManager.Instance.SuccessOrder(dish);
    }

    // Despues de un tiempo la hoja usada regresa sucia al meson de regreso, apilandose con las demas
    private IEnumerator ReturnDirtyPlate()
    {
        yield return new WaitForSeconds(dirtyPlateDelay);

        while (true)
        {
            foreach (Tile tile in dirtyPlateTiles)
            {
                if (!tile) continue;

                if (tile.CurrentItem && tile.CurrentItem.TryGetComponent(out DirtyPlate stack))
                {
                    stack.AddLeaves(1);
                    yield break;
                }

                Item dirtyPlate = Instantiate(dirtyPlatePrefab);
                if (tile.TryPlaceItem(dirtyPlate)) yield break;
                Destroy(dirtyPlate.gameObject);
            }
            // El meson esta ocupado con otra cosa: intentar de nuevo mas tarde
            yield return new WaitForSeconds(dirtyPlateRetryTime);
        }
    }
}
