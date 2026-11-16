using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FoodType
{
    Raw,
    Processed,
    Cooked
}

[CreateAssetMenu (menuName = "New Food")]
public class FoodData : ScriptableObject
{
    public string itemName;
    public Sprite itemSprite;
    public FoodType foodType;
    public GameObject prefab;

    [Header("Audio al procesar en mesa (opcional)")]
    [Tooltip("Si se asigna, este clip sonará en lugar del de picar (ideal para amasar)")]
    public AudioClip processingAudio;
}