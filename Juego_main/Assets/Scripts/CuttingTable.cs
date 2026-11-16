using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingTable : Tile
{
    // Serialized Field
    [SerializeField] private float timeToCut = 2;
    [SerializeField] private bool esMesaDeAmasar = false;

    // Private *****
    private float _timeToCut;
    private bool _isCutting;

    // MonoBehavior Methods
    private void Start()
    {
        _timeToCut = timeToCut;
    }

    private void Update()
    {
        if (!_isCutting) return;

        _timeToCut -= Time.deltaTime;
        ShowProgress(1f - Mathf.Clamp01(_timeToCut / timeToCut));

        if (_timeToCut <= 0 && _item.TryGetComponent(out Food food) && food.CanBeCut())
        {
            if (CraftFood(food.GetNextFoodData()))
            {
                ActionComplete();
                _timeToCut = timeToCut;
            }
        }
    }

    // Public Methods *****
    public override void TakeAction(PlayerInteraction owner, Item playerItem, Action onActionComplete)
    {
        _onActionComplete = onActionComplete;

        if (playerItem & !_item)
        {
            if (GrabItem(playerItem)) owner.DropItem();
        }
        else if (!playerItem && _item)
        {
            if (_item.TryGetComponent(out Food food) && food.CanBeCut())
            {
                TakeAdvanceAction(owner);
            }
            else
            {
                if (owner.GrabItem(_item)) DropItem();
            }
        }
        else if (playerItem && _item && playerItem.TryGetComponent(out Plate plate) &&
                 _item.TryGetComponent(out Food done) && !done.CanBeCut() && plate.TryAddFood(done.GetFoodData()))
        {
            Destroy(_item.gameObject);
            DropItem();
        }
    }

    public override void ActionComplete()
    {
        if (!_item && !_isCutting) return;

        _isCutting = false;
        HideProgress();
        _onActionComplete();
    }

    // Private Methods *****
    protected override void TakeAdvanceAction(PlayerInteraction owner)
    {
        _timeToCut = timeToCut;
        _isCutting = true;

        // 1. Si el ingrediente (FoodData) tiene un audio asignado (ej. masa), reproduce ese
        if (_item != null && _item.TryGetComponent(out Food food) && food.GetFoodData().processingAudio != null)
        {
            owner.StartCutAnimation(food.GetFoodData().processingAudio);
        }
        // 2. Si esta mesa está marcada como mesa de amasar, usa el sonido de amasar del jugador
        else if (esMesaDeAmasar)
        {
            owner.StartKneadAnimation();
        }
        // 3. Para cualquier otro alimento/mesa, mantiene el sonido normal de picar
        else
        {
            owner.StartCutAnimation();
        }
    }

    private bool CraftFood(FoodData newFoodData)
    {
        if (!_item) return false;

        GameObject newFoodClone = Instantiate(newFoodData.prefab, _item.transform.parent, false);
        Destroy(_item.gameObject);
        _item = newFoodClone.GetComponent<Item>();
        return true;
    }
}