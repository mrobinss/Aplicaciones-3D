using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressUI : MonoBehaviour
{
    // Serialized
    [SerializeField] private Image progressBar;
    
    // Private *****
    private Camera _camera;
    private Item _followingItem;
    private Transform _followingTarget;
    private bool _destroying;
    
    // MonoBehavior Methods
    private void Awake()
    {
        _camera = Camera.main;
        transform.localScale = Vector3.zero;
    }

    private void OnEnable()
    {
        LeanTween.scale(gameObject, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBack);
    }

    void Update()
    {
        if (!_followingTarget) return;

        Vector3 progressUiPosition = _camera.WorldToScreenPoint(_followingTarget.position);
        progressUiPosition = new Vector3(progressUiPosition.x, progressUiPosition.y - 45, progressUiPosition.z);
        transform.position = progressUiPosition;
    }

    private void OnDestroy()
    {
        if(_followingItem) _followingItem.OnItemProgressChange -= Item_OnItemProgressChange;
    }

    // Public Methods
    public void Set(Item item)
    {
        _followingItem = item;
        _followingTarget = item.transform;
        _followingItem.OnItemProgressChange += Item_OnItemProgressChange;
    }

    // Barra para acciones de estaciones (moler, amasar): sigue un punto y se llena manualmente
    public void SetTarget(Transform target)
    {
        _followingTarget = target;
    }

    public void SetProgress(float progressNormalized)
    {
        progressBar.fillAmount = progressNormalized;
    }

    public void Close()
    {
        if (_destroying) return;

        _destroying = true;
        LeanTween.scale(gameObject, Vector3.zero, 0.3f).setEase(LeanTweenType.easeInBack).setOnComplete(() => Destroy(gameObject));
    }
    
    // Private Methods
    private void Item_OnItemProgressChange(object sender, float progressNormalized)
    {
        progressBar.fillAmount = progressNormalized;

        if (_destroying || !(progressNormalized <= 0.1)) return;
        
        LeanTween.scale(gameObject, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInBack);
        _destroying = true;

    }

}
