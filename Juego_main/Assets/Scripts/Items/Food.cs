using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : Item
{
    // Serialized *****
    [SerializeField] private FoodData foodData;
    [SerializeField] private FoodData nextFoodData;
    
    // Private *****
    private MeshFilter _mesh;

    // Public Methods *****

    // Solo se procesa en la mesa si es crudo y tiene un siguiente estado definido
    public bool CanBeCut() => foodData.foodType == FoodType.Raw && nextFoodData;
    public FoodData GetFoodData() => foodData;
    public FoodData GetNextFoodData() => nextFoodData;
}
