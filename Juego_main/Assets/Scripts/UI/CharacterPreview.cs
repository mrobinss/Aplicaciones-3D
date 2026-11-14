using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

// Muestra un personaje 3D animado (su idle) dentro de la UI en lugar de una imagen fija.
// Crea lejos de la escena un "estudio" con el personaje y lo dibuja en este RawImage con el mismo
// aspecto de los niveles: la luz del atardecer y su post-procesado. Como en URP el post-procesado
// pierde la transparencia, se renderiza dos veces (color con post-procesado y silueta sin el) y el
// material "UI/CharacterPreview" las junta.
[RequireComponent(typeof(RawImage))]
public class CharacterPreview : MonoBehaviour
{
    // Serialized
    [SerializeField] private GameObject characterPrefab;
    // Giro del personaje: 0 = mirando de frente; positivo lo voltea hacia la izquierda de la pantalla
    [SerializeField] private float yaw;
    [SerializeField] private float padding = 1.08f;
    [SerializeField] private int textureHeight = 1024;

    [Header("Aspecto de los niveles")]
    [SerializeField] private Material previewMaterial;
    [SerializeField] private VolumeProfile volumeProfile;
    [SerializeField] private Color lightColor = new Color(1f, 0.8f, 0.62f);
    [SerializeField] private float lightIntensity = 1.45f;
    [SerializeField] private Vector3 lightAngles = new Vector3(48f, 326f, 0f);
    [SerializeField] private Color ambientSky = new Color(1f, 0.84f, 0.74f);
    [SerializeField] private Color ambientEquator = new Color(0.8f, 0.7f, 0.76f);
    [SerializeField] private Color ambientGround = new Color(0.45f, 0.38f, 0.42f);

    // Private
    private const string LightName = "CharacterPreviewLight";
    private const string VolumeName = "CharacterPreviewVolume";
    private static int _layer = -1;
    private static int _stages;
    private RenderTexture _colorTexture;
    private RenderTexture _alphaTexture;
    private Material _material;
    private GameObject _stage;

    // MonoBehaviour Callbacks
    private void Start()
    {
        if (!characterPrefab) return;

        // Proporcion con la que se ve en pantalla (las imagenes pueden estar escaladas distinto en X y en Y)
        Vector3[] corners = new Vector3[4];
        ((RectTransform)transform).GetWorldCorners(corners);
        float width = Vector3.Distance(corners[0], corners[3]);
        float height = Vector3.Distance(corners[0], corners[1]);
        float aspect = height > 0.001f ? width / height : 0.7f;
        int textureWidth = Mathf.Max(64, Mathf.RoundToInt(textureHeight * aspect));

        _colorTexture = CreateTexture(textureWidth, "Color");
        _alphaTexture = CreateTexture(textureWidth, "Silueta");

        int layer = PreviewLayer();

        // Cada estudio en su propio lugar, lejos de todo lo demas
        _stage = new GameObject("CharacterPreviewStage_" + characterPrefab.name);
        _stage.transform.position = new Vector3(0f, -1000f - 100f * _stages++, 0f);

        GameObject character = Instantiate(characterPrefab, _stage.transform);
        character.transform.localPosition = Vector3.zero;
        character.transform.localRotation = Quaternion.Euler(0f, yaw, 0f) * characterPrefab.transform.localRotation;
        foreach (Transform part in character.GetComponentsInChildren<Transform>(true)) part.gameObject.layer = layer;

        Bounds bounds = GetBounds(character);
        float size = Mathf.Max(bounds.extents.y, bounds.extents.x / aspect) * padding;
        Vector3 cameraPosition = bounds.center + Vector3.forward * 20f;

        bool composite = previewMaterial && volumeProfile;
        CreateCamera("Color", cameraPosition, size, layer, _colorTexture, composite);
        CreateCamera("Silueta", cameraPosition, size, layer, _alphaTexture, false);

        RawImage image = GetComponent<RawImage>();
        image.color = Color.white;
        if (composite)
        {
            _material = new Material(previewMaterial);
            _material.SetTexture("_AlphaTex", _alphaTexture);
            image.material = _material;
            image.texture = _colorTexture;
        }
        else
        {
            image.texture = _alphaTexture;
        }

        SetupLighting(layer);
    }

    private void OnDestroy()
    {
        if (_stage) Destroy(_stage);
        if (_material) Destroy(_material);
        if (_colorTexture) _colorTexture.Release();
        if (_alphaTexture) _alphaTexture.Release();
    }

    // Private Methods
    private RenderTexture CreateTexture(int width, string label)
    {
        return new RenderTexture(width, textureHeight, 24, RenderTextureFormat.ARGB32)
        {
            antiAliasing = 4,
            name = "CharacterPreview_" + characterPrefab.name + "_" + label
        };
    }

    // postProcess: la camara de color usa el post-procesado de los niveles (fondo negro);
    // la de silueta no, y deja el fondo transparente
    private void CreateCamera(string cameraName, Vector3 position, float size, int layer, RenderTexture target, bool postProcess)
    {
        Camera previewCamera = new GameObject(cameraName).AddComponent<Camera>();
        previewCamera.transform.SetParent(_stage.transform, false);
        previewCamera.transform.position = position;
        previewCamera.transform.rotation = Quaternion.LookRotation(Vector3.back);
        previewCamera.orthographic = true;
        previewCamera.orthographicSize = size;
        previewCamera.nearClipPlane = 0.1f;
        previewCamera.farClipPlane = 40f;
        previewCamera.cullingMask = 1 << layer;
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        previewCamera.targetTexture = target;

        UniversalAdditionalCameraData cameraData = previewCamera.GetUniversalAdditionalCameraData();
        cameraData.renderPostProcessing = postProcess;
        cameraData.renderShadows = false;
        cameraData.volumeLayerMask = 1 << layer;
    }

    // Luz, ambiente y post-procesado del atardecer, solo para los personajes (su capa)
    private void SetupLighting(int layer)
    {
        foreach (Light sceneLight in FindObjectsOfType<Light>())
        {
            if (sceneLight.name != LightName) sceneLight.cullingMask &= ~(1 << layer);
        }

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = ambientSky;
        RenderSettings.ambientEquatorColor = ambientEquator;
        RenderSettings.ambientGroundColor = ambientGround;

        if (!GameObject.Find(LightName))
        {
            Light previewLight = new GameObject(LightName).AddComponent<Light>();
            previewLight.transform.rotation = Quaternion.Euler(lightAngles);
            previewLight.type = LightType.Directional;
            previewLight.color = lightColor;
            previewLight.intensity = lightIntensity;
            previewLight.shadows = LightShadows.None;
            previewLight.cullingMask = 1 << layer;
        }

        if (volumeProfile && !GameObject.Find(VolumeName))
        {
            GameObject volumeObject = new GameObject(VolumeName) { layer = layer };
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = volumeProfile;
        }
    }

    // Primera capa sin nombre (no la usa nada del proyecto)
    private static int PreviewLayer()
    {
        if (_layer >= 0) return _layer;

        for (int i = 31; i >= 8; i--)
        {
            if (!string.IsNullOrEmpty(LayerMask.LayerToName(i))) continue;
            _layer = i;
            return _layer;
        }
        _layer = 0;
        return _layer;
    }

    private static Bounds GetBounds(GameObject character)
    {
        Renderer[] renderers = character.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(character.transform.position, Vector3.one);

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer characterRenderer in renderers) bounds.Encapsulate(characterRenderer.bounds);
        return bounds;
    }
}
