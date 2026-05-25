using UnityEngine;

/// <summary>
/// Owns rotation for a plane visual.
///   - Root carries yaw (heading), eased toward target via Mathf.LerpAngle.
///   - meshRoot (child) carries pitch + bank as direct localEulerAngles.
///
/// Inputs are now continuous functions of arc length (provided by FlightPath via
/// PlaneData), so the smoothing here is purely aesthetic lag — not papering over
/// step-function targets like the old design did.
/// </summary>
public class PlaneNoseRotator : MonoBehaviour
{
    [Header("Visual root")]
    [SerializeField] private Transform meshRoot;

    [Header("Yaw")]
    [SerializeField] private float yawSmoothing = 5f;

    [Header("Pitch")]
    [SerializeField] private float pitchPerFpm   = 0.015f;
    [SerializeField] private float maxPitch      = 15f;
    [SerializeField] private float pitchSmoothing = 4f;

    [Header("Banking")]
    [SerializeField] private bool  enableBanking      = true;
    [Tooltip("Bank degrees per (degree per second) of turn rate. e.g. 1.0 → a 10°/s turn produces 10° bank.")]
    [SerializeField] private float bankPerDegPerSec   = 1.0f;
    [SerializeField] private float maxBankAngle       = 25f;
    [SerializeField] private float bankSmoothing      = 3f;

    private float _currentYaw;
    private float _targetYaw;
    private bool  _firstHeading = true;

    private float _currentPitch;
    private float _targetPitch;

    private float _currentBank;
    private float _targetBank;

    private void Awake()
    {
        _currentYaw = transform.eulerAngles.y;
        _targetYaw  = _currentYaw;
    }

    public void SetHeading(float trackDeg)
    {
        if (_firstHeading)
        {
            _currentYaw = trackDeg;
            transform.eulerAngles = new Vector3(0f, trackDeg, 0f);
            _firstHeading = false;
        }
        _targetYaw = trackDeg;
    }

    public void SetVerticalRate(float baroRateFtPerMin)
    {
        _targetPitch = Mathf.Clamp(-baroRateFtPerMin * pitchPerFpm, -maxPitch, maxPitch);
    }

    public void SetTurnRate(float turnRateDegPerSec)
    {
        _targetBank = enableBanking
            ? Mathf.Clamp(turnRateDegPerSec * bankPerDegPerSec, -maxBankAngle, maxBankAngle)
            : 0f;
    }

    private void LateUpdate()
    {
        float dt = Time.deltaTime;

        _currentYaw = Mathf.LerpAngle(_currentYaw, _targetYaw, dt * yawSmoothing);
        transform.eulerAngles = new Vector3(0f, _currentYaw, 0f);

        if (meshRoot == null) return;

        _currentPitch = Mathf.Lerp(_currentPitch, _targetPitch, dt * pitchSmoothing);
        _currentBank  = Mathf.Lerp(_currentBank,  _targetBank,  dt * bankSmoothing);

        meshRoot.localEulerAngles = new Vector3(_currentPitch, 0f, -_currentBank);
    }
}

