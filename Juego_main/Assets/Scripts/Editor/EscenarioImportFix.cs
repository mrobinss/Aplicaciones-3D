using UnityEditor;
using UnityEngine;

// El modelo ESCENARIO.fbx (exportado desde Maya/Arnold) trae sus coordenadas UV reales en el
// segundo canal y el primero vacio. Los shaders de URP leen el primer canal, asi que al importar
// se copia UV2 -> UV1 para que las texturas (paleta, piso, tierra) se vean bien.
public class EscenarioImportFix : AssetPostprocessor
{
    private const string ModelPath = "Assets/Models/Escenario/ESCENARIO.fbx";

    private void OnPostprocessModel(GameObject root)
    {
        if (assetPath != ModelPath) return;

        foreach (MeshFilter meshFilter in root.GetComponentsInChildren<MeshFilter>(true))
        {
            Mesh mesh = meshFilter.sharedMesh;
            if (mesh == null) continue;

            Vector2[] uv2 = mesh.uv2;
            if (uv2 == null || uv2.Length != mesh.vertexCount) continue;

            mesh.uv = uv2;
        }
    }
}
