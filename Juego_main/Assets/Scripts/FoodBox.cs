using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodBox : Tile
{
    [Header("Foodbox")]
    // Serialized *****
    [SerializeField] private FoodData foodItem;
    
    // Private
    private Animator _animator;
    
    // MonoBehavior Methods *****
    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponent<Animator>();
    }

    // Public Methods *****
    public override void ActionComplete()
    {
        
    }

    protected override void TakeAdvanceAction(PlayerInteraction owner)
    {
        Item newFood = Instantiate(foodItem.prefab).GetComponent<Item>();
        if (!owner.GrabItem(newFood))
        {
            // El jugador ya tenia algo en la mano: no dejar comida suelta en la escena
            Destroy(newFood.gameObject);
            return;
        }
        _animator.SetTrigger("Open");
    }
}
