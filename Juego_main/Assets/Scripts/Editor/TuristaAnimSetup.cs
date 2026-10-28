using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menu: Grupo 3 > Configurar animaciones del Turista
// 1. Corta los clips (frames a 60 fps) dentro de "TURISTA RIG.fbx".
// 2. Crea Assets/Animation/Turista.controller con un estado por clip.
// 3. En la escena abierta: agrega Animator + TuristaClipAnimator al objeto Turista del jugador.
public static class TuristaAnimSetup
{
    private const string ModelPath = "Assets/Models/Characters/TURISTA RIG.fbx";
    private const string ControllerPath = "Assets/Animation/Turista.controller";

    private static readonly (string name, int first, int last, bool loop)[] Clips =
    {
        ("Idle", 1, 121, true),
        ("Walk", 131, 157, true),
        ("PickUp", 171, 195, false),
        ("CarryIdle", 201, 321, true),
        ("CarryWalk", 331, 357, true),
        ("Cut", 371, 419, true),
        ("Knead", 431, 491, true),
    };

    // Quita el componente viejo TuristaAnimation de la escena y borra su script del proyecto.
    [MenuItem("Grupo 3/Eliminar TuristaAnimation (script viejo)")]
    public static void EliminarViejo()
    {
        int quitados = 0;
        foreach (MonoBehaviour mb in Object.FindObjectsOfType<MonoBehaviour>(true))
        {
            if (mb != null && mb.GetType().Name == "TuristaAnimation")
            {
                Undo.DestroyObjectImmediate(mb);
                quitados++;
            }
        }
        foreach (GameObject go in Object.FindObjectsOfType<GameObject>(true))
            quitados += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        bool borrado = AssetDatabase.DeleteAsset("Assets/Scripts/TuristaAnimation.cs");
        Debug.Log("TuristaAnimation: " + quitados + " componente(s) quitado(s) de la escena; script borrado: " + borrado);
    }

    [MenuItem("Grupo 3/Configurar animaciones del Turista")]
    public static void Configure()
    {
        // 1. Clips del FBX
        var importer = (ModelImporter)AssetImporter.GetAtPath(ModelPath);
        if (importer == null)
        {
            Debug.LogError("No encontre " + ModelPath);
            return;
        }

        importer.importAnimation = true;
        importer.animationType = ModelImporterAnimationType.Generic;
        string take = importer.defaultClipAnimations.Length > 0
            ? importer.defaultClipAnimations[0].takeName
            : (importer.importedTakeInfos.Length > 0 ? importer.importedTakeInfos[0].name : "Take 001");

        importer.clipAnimations = Clips.Select(c => new ModelImporterClipAnimation
        {
            name = c.name,
            takeName = take,
            firstFrame = c.first,
            lastFrame = c.last,
            loopTime = c.loop,
            loopPose = false,
        }).ToArray();
        importer.SaveAndReimport();

        // 2. Controlador
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null)
            AssetDatabase.DeleteAsset(ControllerPath);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        AnimatorStateMachine sm = controller.layers[0].stateMachine;

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(ModelPath);
        foreach (var c in Clips)
        {
            AnimationClip clip = assets.OfType<AnimationClip>().FirstOrDefault(a => a.name == c.name);
            if (clip == null)
            {
                Debug.LogError("No se creo el clip " + c.name + " en " + ModelPath);
                continue;
            }
            AnimatorState state = sm.AddState(c.name);
            state.motion = clip;
            if (c.name == "Idle") sm.defaultState = state;
        }
        AssetDatabase.SaveAssets();

        Avatar avatar = assets.OfType<Avatar>().FirstOrDefault();

        // 3. Escena abierta
        int count = 0;
        foreach (PlayerInteraction interaction in Object.FindObjectsOfType<PlayerInteraction>(true))
        {
            Transform t = interaction.transform.Find("Turista");
            if (t == null) continue;
            GameObject go = t.gameObject;

            Animator anim = go.GetComponent<Animator>();
            if (anim == null) anim = Undo.AddComponent<Animator>(go);
            Undo.RecordObject(anim, "Configurar Animator Turista");
            anim.runtimeAnimatorController = controller;
            if (avatar != null) anim.avatar = avatar;
            anim.applyRootMotion = false;

            TuristaClipAnimator clipAnim = go.GetComponent<TuristaClipAnimator>();
            if (clipAnim == null) clipAnim = Undo.AddComponent<TuristaClipAnimator>(go);

            var so = new SerializedObject(clipAnim);
            so.FindProperty("sourceAnimator").objectReferenceValue = interaction.GetComponent<Animator>();
            so.FindProperty("itemAnchor").objectReferenceValue =
                new SerializedObject(interaction).FindProperty("itemAnchor").objectReferenceValue;
            so.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(go.scene);
            count++;
        }

        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Turista configurado: " + Clips.Length + " clips, " + count + " personaje(s) en escena.");
    }
}
