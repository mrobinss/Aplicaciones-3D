using System.Collections.Generic;
using UnityEngine;

// Pila de hojas (platos) sucias que regresan despues de entregar pedidos.
// No sirve para servir comida: hay que botarla en la cesta de composta.
// Las hojas que regresan al mismo meson se apilan en este mismo item.
public class DirtyPlate : Item
{
    // Serialized *****
    [SerializeField] private float stackSpacing = 0.03f;

    // Private *****
    private readonly List<GameObject> _stackedLeaves = new List<GameObject>();
    private int _count = 1;

    // Public Methods *****
    public int Count => _count;

    public void AddLeaves(int amount)
    {
        _count += amount;
        RefreshStack();
    }

    // Private Methods *****
    private void RefreshStack()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        // La primera hoja es este mismo objeto; las demas son copias visuales encima
        while (_stackedLeaves.Count < _count - 1)
        {
            int index = _stackedLeaves.Count + 1;
            GameObject leaf = new GameObject("HojaSuciaApilada", typeof(MeshFilter), typeof(MeshRenderer));
            leaf.transform.SetParent(transform, false);
            leaf.transform.localPosition = Vector3.up * stackSpacing * index;
            leaf.transform.localRotation = Quaternion.Euler(0, (index * 37f) % 50f - 25f, 0);
            leaf.GetComponent<MeshFilter>().sharedMesh = meshFilter.sharedMesh;
            leaf.GetComponent<MeshRenderer>().sharedMaterials = meshRenderer.sharedMaterials;
            _stackedLeaves.Add(leaf);
        }
    }
}
