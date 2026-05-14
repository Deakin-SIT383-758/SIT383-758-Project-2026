using UnityEngine;

/// <summary>
/// Drives a single plane prefab in world space from its PlaneData.
/// One PlaneVisual instance per active plane.
///
/// Path samples are now continuous (track, baroRate, turnRate all CÂ¹), so we forward
/// them every frame and let PlaneNoseRotator do mild visual smoothing on top. The
/// previous "only push heading on segment transition" trick is no longer needed.
/// </summary>
[RequireComponent(typeof(PlaneNoseRotator))]
public class PlaneVisual : MonoBehaviour
{
    [Tooltip("Auto-fetched if left blank. Owns root yaw + meshRoot pitch/bank.")]
    [SerializeField] private PlaneNoseRotator noseRotator;

    public string Hex { get; private set; }
    public void Bind(string hex) => Hex = hex;

    private void Awake()
    {
        if (noseRotator == null) noseRotator = GetComponent<PlaneNoseRotator>();
    }

    public void ApplyData(PlaneData data)
    {
        if (data == null) return;

        transform.position = data.mapPosition;

        if (noseRotator == null) return;

        noseRotator.SetHeading(data.track);
        noseRotator.SetVerticalRate(data.baroRate);
        noseRotator.SetTurnRate(data.turnRateDegPerSec);
    }
}