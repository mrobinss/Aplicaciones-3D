using UnityEngine;

// Animacion procedural para el modelo TURISTA (no trae clips de animacion).
// Lee los mismos parametros del Animator que usan Player y PlayerInteraction ("Running" y "Cutting")
// y mueve los huesos del rig: rebote y balanceo al correr, golpe de brazo al cortar y respiracion en reposo.
public class TuristaAnimation : MonoBehaviour
{
    // Serialized
    [SerializeField] private Animator sourceAnimator;
    [SerializeField] private Transform body;
    [SerializeField] private Transform head;
    [SerializeField] private Transform armLeft;
    [SerializeField] private Transform armRight;

    [Header("Correr")]
    [SerializeField] private float runFrequency = 9f;
    [SerializeField] private float runBob = 0.08f;
    [SerializeField] private float runRoll = 7f;
    [SerializeField] private float armSwing = 35f;

    [Header("Cortar")]
    [SerializeField] private float cutFrequency = 11f;
    [SerializeField] private float cutAngle = 45f;

    [Header("Reposo")]
    [SerializeField] private float idleFrequency = 2f;
    [SerializeField] private float idleBob = 0.015f;

    [Header("Transicion")]
    [SerializeField] private float blendSpeed = 10f;

    // private *****
    private static readonly int RunningHash = Animator.StringToHash("Running");
    private static readonly int CuttingHash = Animator.StringToHash("Cutting");

    private Vector3 _bodyBasePosition;
    private Quaternion _bodyBaseRotation;
    private Quaternion _headBaseRotation;
    private Quaternion _armLeftBaseRotation;
    private Quaternion _armRightBaseRotation;
    private float _run;
    private float _cut;
    private float _time;

    // Monobehavior Callbacks

    private void Awake()
    {
        if (!sourceAnimator) sourceAnimator = GetComponentInParent<Animator>();

        _bodyBasePosition = body.localPosition;
        _bodyBaseRotation = body.localRotation;
        _headBaseRotation = head.localRotation;
        _armLeftBaseRotation = armLeft.localRotation;
        _armRightBaseRotation = armRight.localRotation;
    }

    private void LateUpdate()
    {
        bool running = sourceAnimator && sourceAnimator.GetBool(RunningHash);
        bool cutting = sourceAnimator && sourceAnimator.GetBool(CuttingHash);

        float t = 1f - Mathf.Exp(-blendSpeed * Time.deltaTime);
        _run = Mathf.Lerp(_run, running ? 1f : 0f, t);
        _cut = Mathf.Lerp(_cut, cutting ? 1f : 0f, t);
        _time += Time.deltaTime;

        // Volver a la pose base antes de aplicar el movimiento de este frame
        body.localPosition = _bodyBasePosition;
        body.localRotation = _bodyBaseRotation;
        head.localRotation = _headBaseRotation;
        armLeft.localRotation = _armLeftBaseRotation;
        armRight.localRotation = _armRightBaseRotation;

        Transform root = transform;
        float runWave = Mathf.Sin(_time * runFrequency);
        float idle = 1f - Mathf.Max(_run, _cut);

        // Rebote del cuerpo (en espacio del personaje, sin importar los ejes de los huesos)
        float bob = Mathf.Abs(runWave) * runBob * _run
                    + Mathf.Sin(_time * idleFrequency) * idleBob * idle
                    + Mathf.Abs(Mathf.Sin(_time * cutFrequency)) * runBob * 0.3f * _cut;
        body.position += root.up * bob;

        // Balanceo lateral al correr, con la cabeza compensando un poco
        body.rotation = Quaternion.AngleAxis(runWave * runRoll * _run, root.forward) * body.rotation;
        head.rotation = Quaternion.AngleAxis(-runWave * runRoll * 0.5f * _run, root.forward) * head.rotation;

        // Brazos: alternados al correr; el derecho golpea hacia abajo al cortar
        float swing = runWave * armSwing * _run;
        float chop = (Mathf.Sin(_time * cutFrequency) * 0.5f + 0.5f) * cutAngle * _cut;
        armLeft.rotation = Quaternion.AngleAxis(swing, root.right) * armLeft.rotation;
        armRight.rotation = Quaternion.AngleAxis(-swing + chop, root.right) * armRight.rotation;
    }
}
