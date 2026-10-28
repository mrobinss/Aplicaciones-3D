using UnityEngine;

// Reproduce las animaciones hechas en Maya para el TURISTA (clips dentro de "TURISTA RIG.fbx").
// No modifica los scripts del jugador: lee los mismos parametros que ya usan Player y
// PlayerInteraction ("Running" y "Cutting") en el Animator del jugador, y mira si hay un
// item en el itemAnchor para saber si el personaje esta cargando algo.
// Reemplaza a TuristaAnimation (que queda desactivado, no borrado).
[RequireComponent(typeof(Animator))]
public class TuristaClipAnimator : MonoBehaviour
{
    // Serialized
    [SerializeField] private Animator sourceAnimator;   // Animator del jugador (padre)
    [SerializeField] private Transform itemAnchor;      // Donde PlayerInteraction pone el item agarrado
    [SerializeField] private float crossFade = 0.15f;

    [Header("Objeto en las manos")]
    [SerializeField] private bool itemOnHands = true;   // El itemAnchor sigue las manos mientras carga
    [SerializeField] private float lift = 0.25f;        // Altura sobre las palmas (en proporcion a la distancia entre manos)
    [SerializeField] private float forward = 0.15f;    // Un poco hacia adelante (misma proporcion)

    // Nombres de los estados del controlador Turista.controller
    private const string Idle = "Idle";
    private const string Walk = "Walk";
    private const string PickUp = "PickUp";
    private const string CarryIdle = "CarryIdle";
    private const string CarryWalk = "CarryWalk";
    private const string Cut = "Cut";
    private const string Knead = "Knead";

    private static readonly int RunningHash = Animator.StringToHash("Running");
    private static readonly int CuttingHash = Animator.StringToHash("Cutting");

    private Animator _anim;
    private string _current;
    private bool _wasCarrying;
    private float _pickUpUntil;
    private bool _kneading;
    private Transform _handL;
    private Transform _handR;
    private Vector3 _anchorBaseLocal;
    private Quaternion _anchorBaseRot;

    // Monobehavior Callbacks

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _anim.applyRootMotion = false;
        if (!sourceAnimator && transform.parent) sourceAnimator = transform.parent.GetComponentInParent<Animator>();
        _handL = FindDeep(transform, "Mano_L");
        _handR = FindDeep(transform, "Mano_R");
        if (itemAnchor)
        {
            _anchorBaseLocal = itemAnchor.localPosition;
            _anchorBaseRot = itemAnchor.localRotation;
        }
    }

    // Despues de que el Animator mueve los huesos: llevar el itemAnchor a las manos
    private void LateUpdate()
    {
        if (!itemAnchor || !itemOnHands || !_handL || !_handR) return;

        bool carrying = itemAnchor.childCount > 0;
        if (!carrying)
        {
            itemAnchor.localPosition = _anchorBaseLocal;
            itemAnchor.localRotation = _anchorBaseRot;
            return;
        }

        Vector3 mid = (_handL.position + _handR.position) * 0.5f;
        float span = Vector3.Distance(_handL.position, _handR.position);
        Transform player = itemAnchor.parent ? itemAnchor.parent : transform;
        itemAnchor.position = mid + player.up * (span * lift) + player.forward * (span * forward);
        itemAnchor.localRotation = _anchorBaseRot;
    }

    private void Update()
    {
        bool running = sourceAnimator && sourceAnimator.GetBool(RunningHash);
        bool cutting = sourceAnimator && sourceAnimator.GetBool(CuttingHash);
        bool carrying = itemAnchor && itemAnchor.childCount > 0;

        // Al agarrar algo se reproduce una vez la animacion de agarrar
        if (carrying && !_wasCarrying)
        {
            _pickUpUntil = Time.time + 0.4f;
            Play(PickUp, true);
        }
        _wasCarrying = carrying;

        if (Time.time < _pickUpUntil) return;

        string target;
        if (cutting) target = Cut;
        else if (_kneading) target = Knead;
        else if (carrying) target = running ? CarryWalk : CarryIdle;
        else target = running ? Walk : Idle;

        Play(target, false);
    }

    // Public

    // Para cuando exista la mecanica de amasar: llamar SetKneading(true) al empezar y false al terminar.
    public void SetKneading(bool kneading) => _kneading = kneading;

    // Private

    private static Transform FindDeep(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }

    private void Play(string state, bool restart)
    {
        if (!restart && state == _current) return;
        _current = state;
        _anim.CrossFadeInFixedTime(state, crossFade, 0, 0f);
    }
}
