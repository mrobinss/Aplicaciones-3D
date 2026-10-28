using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Tile : MonoBehaviour
{
    // Serialized *****
    [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
    [SerializeField] protected Transform itemAnchor;
    // Opcional: indicador que se muestra al resaltar (para estaciones cuyo modelo esta oculto)
    [SerializeField] private GameObject highlightIndicator;
    // Opcional: barra de progreso para acciones que toman tiempo (moler, amasar)
    [SerializeField] private ProgressUI progressUIPrefab;
    [Header("Reference")]
    [SerializeField] protected Item initialItem; 
    // Private *****
    private MeshRenderer _meshRenderer;
    private ProgressUI _progressUI;
    protected Item _item;
    protected Action _onActionComplete;
 
    // MonoBehavior Callbacks *****
    protected virtual void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        
        if (initialItem)
        {
            GrabItem(initialItem);
        }
    }

    // Public Methods
    public virtual void TakeAction(PlayerInteraction owner, Item playerItem, Action onActionComplete)
    {
        _onActionComplete = onActionComplete;
        
        if (!_item && playerItem)
        {
            if(GrabItem(playerItem)) owner.DropItem();
        }
        else if(_item && !playerItem)
        {
           if( owner.GrabItem(_item)) DropItem();
        // For pot   
        }else if (_item && _item.TryGetComponent(out Cookware cookware) && playerItem && playerItem.TryGetComponent(out Food food))
        {
            if (cookware.AddFood(food.GetFoodData()))
            {
                owner.RemoveItem();
            }
        }
        // For Plate and food
        else if (_item && _item.TryGetComponent(out Cookware cookware2) && playerItem && playerItem.TryGetComponent(out Plate plate))
        {
            // Una hoja que ya tiene comida no debe vaciar la olla (se perderia el plato)
            if (!plate.IsCookedFood() && cookware2.TryGetDish(out FoodData foodCooked))
            {
                plate.PlateUpSoup(foodCooked);
            }
            else
            {
                Debug.Log("Food not ready");
            }
        }
        else if (_item && _item.TryGetComponent(out Plate plateF) && playerItem && playerItem.TryGetComponent(out Cookware cookwareF))
        {
            if (!plateF.IsCookedFood() && cookwareF.TryGetDish(out FoodData foodCooked))
            {
                plateF.PlateUpSoup(foodCooked);
            }
            else
            {
                Debug.Log("Food not ready");
            }
        }
        else
        {
            TakeAdvanceAction(owner);
        }
    }
    public abstract void ActionComplete();
    
    public void StopHighlight()
    {
        if (highlightIndicator) highlightIndicator.SetActive(false);

        if (_meshRenderer)
        {
            for (int i=0; i<_meshRenderer.materials.Length; i++)
            {
                //_meshRenderer.materials[i].color = _baseColors[i];
                Material[] materials;
                (materials = _meshRenderer.materials)[i].DisableKeyword("_EMISSION");
                materials[i].globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                _meshRenderer.materials[i].SetColor("_EmissionColor", Color.black);
            }
        }else if (_skinnedMeshRenderer)
        {
            for (int i=0; i<_skinnedMeshRenderer.materials.Length; i++)
            {
                //_meshRenderer.materials[i].color = _baseColors[i];
                Material[] materials;
                (materials = _skinnedMeshRenderer.materials)[i].DisableKeyword("_EMISSION");
                materials[i].globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                _skinnedMeshRenderer.materials[i].SetColor("_EmissionColor", Color.black);
            }
        }
    }
    
    // Coloca un item sobre la estacion desde otro sistema (por ejemplo, hojas sucias que regresan)
    public bool TryPlaceItem(Item item) => GrabItem(item);
    public Item CurrentItem => _item;

    // Private Methods *****
    protected abstract void TakeAdvanceAction(PlayerInteraction owner);

    protected void ShowProgress(float progressNormalized)
    {
        if (!progressUIPrefab) return;

        if (!_progressUI)
        {
            GameObject canvas = GameObject.FindGameObjectWithTag("CanvasInteraction");
            if (!canvas) return;

            _progressUI = Instantiate(progressUIPrefab, canvas.transform);
            _progressUI.SetTarget(itemAnchor);
        }
        _progressUI.SetProgress(progressNormalized);
    }

    protected void HideProgress()
    {
        if (!_progressUI) return;

        _progressUI.Close();
        _progressUI = null;
    }

    public void StartHighlight()
    {
        if (highlightIndicator) highlightIndicator.SetActive(true);

        if (_meshRenderer)
        {
            for (int i=0; i<_meshRenderer.materials.Length; i++)
            {
                Material[] materials;
                (materials = _meshRenderer.materials)[i].EnableKeyword("_EMISSION");
                materials[i].globalIlluminationFlags = MaterialGlobalIlluminationFlags.AnyEmissive;
                _meshRenderer.materials[i].SetColor("_EmissionColor", new Color(0.2f,0.2f,0.2f));
            }
        }else if (_skinnedMeshRenderer)
        {
            for (int i=0; i<_skinnedMeshRenderer.materials.Length; i++)
            {
                Material[] materials;
                (materials = _skinnedMeshRenderer.materials)[i].EnableKeyword("_EMISSION");
                materials[i].globalIlluminationFlags = MaterialGlobalIlluminationFlags.AnyEmissive;
                _skinnedMeshRenderer.materials[i].SetColor("_EmissionColor", new Color(0.2f,0.2f,0.2f));
            }
        }
    }
    protected virtual bool GrabItem(Item item)
    {
        if (_item) return false; //This table already have item
        
        _item = item;
        _item.transform.SetParent(itemAnchor,false);
        _item.transform.localPosition = Vector3.zero;
        
        return true;
    }

    protected virtual void DropItem()
    {
        _item = null;
    }
}
