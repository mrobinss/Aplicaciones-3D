using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Menu: Grupo 3 > Ambiente atardecer / Quitar ambiente atardecer
// Cambia solo la luz y el color de la escena abierta (luz direccional, luz ambiente y el perfil
// del Global Volume). El perfil original (SampleSceneProfile) NO se modifica: se crea una copia
// "AtardecerProfile" y se asigna al volumen. "Quitar" vuelve a dejar todo como estaba.
public static class AmbienteAtardecer
{
    private const string ProfilePath = "Assets/Settings/AtardecerProfile.asset";
    private const string Key = "Grupo3_AtardecerBackup_";

    [MenuItem("Grupo 3/Ambiente atardecer")]
    public static void Aplicar()
    {
        Light sol = BuscarSol();
        Volume volumen = Object.FindObjectOfType<Volume>();

        // Guardar los valores originales una sola vez, para poder volver
        if (!EditorPrefs.HasKey(Key + "guardado"))
        {
            if (sol != null)
            {
                EditorPrefs.SetString(Key + "solColor", ColorUtility.ToHtmlStringRGBA(sol.color));
                EditorPrefs.SetFloat(Key + "solIntensidad", sol.intensity);
                EditorPrefs.SetString(Key + "solRot", JsonUtility.ToJson(sol.transform.eulerAngles));
            }
            EditorPrefs.SetInt(Key + "ambMode", (int)RenderSettings.ambientMode);
            EditorPrefs.SetString(Key + "ambSky", ColorUtility.ToHtmlStringRGBA(RenderSettings.ambientSkyColor));
            EditorPrefs.SetString(Key + "ambEq", ColorUtility.ToHtmlStringRGBA(RenderSettings.ambientEquatorColor));
            EditorPrefs.SetString(Key + "ambGround", ColorUtility.ToHtmlStringRGBA(RenderSettings.ambientGroundColor));
            if (volumen != null && volumen.sharedProfile != null)
                EditorPrefs.SetString(Key + "perfil", AssetDatabase.GetAssetPath(volumen.sharedProfile));
            EditorPrefs.SetBool(Key + "guardado", true);
        }

        // Sol bajo y anaranjado
        if (sol != null)
        {
            Undo.RecordObject(sol, "Atardecer");
            Undo.RecordObject(sol.transform, "Atardecer");
            sol.color = new Color(1f, 0.80f, 0.62f);
            sol.intensity = 1.45f;
            Vector3 e = sol.transform.eulerAngles;
            sol.transform.eulerAngles = new Vector3(48f, e.y, e.z);
        }

        // Luz ambiente en degradado: cielo calido, horizonte lila, suelo oscuro
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(1.0f, 0.84f, 0.74f);
        RenderSettings.ambientEquatorColor = new Color(0.80f, 0.70f, 0.76f);
        RenderSettings.ambientGroundColor = new Color(0.45f, 0.38f, 0.42f);

        // Perfil de post-procesado: copia del original + ajustes de atardecer
        if (volumen != null)
        {
            string original = EditorPrefs.GetString(Key + "perfil", "");
            if (AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath) == null)
            {
                if (!string.IsNullOrEmpty(original)) AssetDatabase.CopyAsset(original, ProfilePath);
                else AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<VolumeProfile>(), ProfilePath);
                AssetDatabase.ImportAsset(ProfilePath);
            }
            VolumeProfile p = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);

            var tone = Get<Tonemapping>(p);
            tone.mode.Override(TonemappingMode.ACES);

            var color = Get<ColorAdjustments>(p);
            color.postExposure.Override(0.25f);
            color.contrast.Override(6f);
            color.saturation.Override(6f);
            color.colorFilter.Override(new Color(1f, 0.97f, 0.93f));

            var wb = Get<WhiteBalance>(p);
            wb.temperature.Override(10f);
            wb.tint.Override(4f);

            var split = Get<SplitToning>(p);
            split.shadows.Override(new Color(0.48f, 0.42f, 0.60f));
            split.highlights.Override(new Color(0.85f, 0.65f, 0.45f));
            split.balance.Override(-10f);

            var bloom = Get<Bloom>(p);
            bloom.threshold.Override(0.9f);
            bloom.intensity.Override(0.55f);
            bloom.scatter.Override(0.7f);
            bloom.tint.Override(new Color(1f, 0.8f, 0.6f));

            var vig = Get<Vignette>(p);
            vig.color.Override(new Color(0.18f, 0.08f, 0.15f));
            vig.intensity.Override(0.22f);
            vig.smoothness.Override(0.45f);

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();

            Undo.RecordObject(volumen, "Atardecer");
            volumen.sharedProfile = p;
        }

        foreach (Camera cam in Object.FindObjectsOfType<Camera>())
        {
            var data = cam.GetComponent<UniversalAdditionalCameraData>();
            if (data != null && !data.renderPostProcessing)
            {
                Undo.RecordObject(data, "Atardecer");
                data.renderPostProcessing = true;
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Ambiente atardecer aplicado. Para volver: Grupo 3 > Quitar ambiente atardecer.");
    }

    [MenuItem("Grupo 3/Quitar ambiente atardecer")]
    public static void Quitar()
    {
        if (!EditorPrefs.HasKey(Key + "guardado"))
        {
            Debug.Log("No hay valores originales guardados en este PC.");
            return;
        }
        Light sol = BuscarSol();
        if (sol != null && EditorPrefs.HasKey(Key + "solColor"))
        {
            Undo.RecordObject(sol, "Quitar atardecer");
            Undo.RecordObject(sol.transform, "Quitar atardecer");
            ColorUtility.TryParseHtmlString("#" + EditorPrefs.GetString(Key + "solColor"), out Color c);
            sol.color = c;
            sol.intensity = EditorPrefs.GetFloat(Key + "solIntensidad");
            sol.transform.eulerAngles = JsonUtility.FromJson<Vector3>(EditorPrefs.GetString(Key + "solRot"));
        }
        RenderSettings.ambientMode = (AmbientMode)EditorPrefs.GetInt(Key + "ambMode");
        ColorUtility.TryParseHtmlString("#" + EditorPrefs.GetString(Key + "ambSky"), out Color s);
        ColorUtility.TryParseHtmlString("#" + EditorPrefs.GetString(Key + "ambEq"), out Color q);
        ColorUtility.TryParseHtmlString("#" + EditorPrefs.GetString(Key + "ambGround"), out Color g);
        RenderSettings.ambientSkyColor = s;
        RenderSettings.ambientEquatorColor = q;
        RenderSettings.ambientGroundColor = g;

        Volume volumen = Object.FindObjectOfType<Volume>();
        string original = EditorPrefs.GetString(Key + "perfil", "");
        if (volumen != null && !string.IsNullOrEmpty(original))
        {
            Undo.RecordObject(volumen, "Quitar atardecer");
            volumen.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(original);
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Ambiente original restaurado.");
    }

    private static Light BuscarSol()
    {
        if (RenderSettings.sun != null) return RenderSettings.sun;
        foreach (Light l in Object.FindObjectsOfType<Light>())
            if (l.type == LightType.Directional) return l;
        return null;
    }

    private static T Get<T>(VolumeProfile p) where T : VolumeComponent
    {
        if (!p.TryGet(out T comp))
        {
            comp = p.Add<T>(false);
            AssetDatabase.AddObjectToAsset(comp, p);
        }
        comp.active = true;
        return comp;
    }
}
