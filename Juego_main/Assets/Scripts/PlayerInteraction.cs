using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    // Serialized *****
    [SerializeField] private Transform itemAnchor;
    [SerializeField] private AudioClip grabAudio;
    [SerializeField] private AudioClip cuttingAudio;
    [SerializeField] private AudioClip kneadingAudio;

    [Header("Calibración de Volumen")]
    [Range(0f, 1f)] [SerializeField] private float cuttingVolume = 0.4f;
    [Range(0f, 1f)] [SerializeField] private float kneadingVolume = 1.0f;

    // Private *****
    private Animator _anim;
    private List<Tile> _tileCloseList = new List<Tile>();
    private Tile _closestTile;
    private Item _item;

    private AudioSource _actualAudioSurce;
    private SphereCollider _reach;
    private readonly Collider[] _nearbyColliders = new Collider[32];

    // MonoBehaviour Callbacks *****
    private void Awake()
    {
        _anim = GetComponent<Animator>();
        foreach (SphereCollider sphere in GetComponents<SphereCollider>())
        {
            if (sphere.isTrigger) _reach = sphere;
        }
    }

    private void Update()
    {
        RefreshClosestTile();

        if (!_closestTile) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _closestTile.TakeAction(this, _item, StopCutAnimation);
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            _closestTile.ActionComplete();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Tile tile) && !_tileCloseList.Contains(tile))
        {
            _tileCloseList.Add(tile);
            RefreshClosestTile();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Tile tile))
        {
            _tileCloseList.Remove(tile);
            RefreshClosestTile();
        }
    }

    // Private Methods *****
    private void RefreshClosestTile()
    {
        if (_reach) FindNearbyTiles();
        _tileCloseList.RemoveAll(tile => !tile);

        Tile closest = GetCloserTile();
        if (closest == _closestTile) return;

        if (_closestTile) _closestTile.StopHighlight();
        _closestTile = closest;
        if (_closestTile) _closestTile.StartHighlight();
    }

    private void FindNearbyTiles()
    {
        _tileCloseList.Clear();

        Vector3 lossyScale = _reach.transform.lossyScale;
        float radius = _reach.radius * Mathf.Max(lossyScale.x, Mathf.Max(lossyScale.y, lossyScale.z));
        Vector3 center = _reach.transform.TransformPoint(_reach.center);
        int count = Physics.OverlapSphereNonAlloc(center, radius, _nearbyColliders, ~0, QueryTriggerInteraction.Collide);
        for (int i = 0; i < count; i++)
        {
            if (_nearbyColliders[i].isTrigger && _nearbyColliders[i].TryGetComponent(out Tile tile) && !_tileCloseList.Contains(tile))
            {
                _tileCloseList.Add(tile);
            }
        }
    }

    private Tile GetCloserTile()
    {
        if (_tileCloseList.Count <= 0) return null;

        Tile winnerTile = null;
        float minDistance = Mathf.Infinity;
        foreach (Tile tile in _tileCloseList)
        {
            Vector3 point = tile.TryGetComponent(out Collider tileCollider) ? tileCollider.ClosestPoint(transform.position) : tile.transform.position;
            point.y = transform.position.y;
            float newDistance = Vector3.Distance(transform.position, point);
            if (newDistance <= minDistance)
            {
                winnerTile = tile;
                minDistance = newDistance;
            }
        }

        return winnerTile;
    }

    // Public Methods *****
    public bool GrabItem(Item item)
    {
        if (_item) return false;

        _item = item;
        _item.transform.SetParent(itemAnchor, false);
        _item.transform.localPosition = Vector3.zero;
        float volEfectos = PlayerPrefs.GetFloat("VolumenEfectos", 1f);
        AudioManager.CreateSFX(grabAudio, 1f * volEfectos, 1);

        return true;
    }

    public void DropItem()
    {
        _item = null;
        float volEfectos = PlayerPrefs.GetFloat("VolumenEfectos", 1f);
        AudioManager.CreateSFX(grabAudio, 1f * volEfectos, 0.65f);
    }

    public void RemoveItem()
    {
        Destroy(_item.gameObject);
        DropItem();
    }

    public void StartCutAnimation(AudioClip customAudio = null)
    {
        _anim.SetBool("Cutting", true);
        bool esAmasado = customAudio != null;
        AudioClip clipAUsar = esAmasado ? customAudio : cuttingAudio;
        float volumenBase = esAmasado ? kneadingVolume : cuttingVolume;
        float volEfectos = PlayerPrefs.GetFloat("VolumenEfectos", 1f);

        if (clipAUsar != null)
        {
            _actualAudioSurce = AudioManager.CreateSFX(clipAUsar, volumenBase * volEfectos, 1f, true);
        }
    }

    public void StartKneadAnimation()
    {
        StartCutAnimation(kneadingAudio);
    }

    public void StopCutAnimation()
    {
        _anim.SetBool("Cutting", false);
        if (_actualAudioSurce)
        {
            _actualAudioSurce.Stop();
            Destroy(_actualAudioSurce.gameObject);
        }
    }
}