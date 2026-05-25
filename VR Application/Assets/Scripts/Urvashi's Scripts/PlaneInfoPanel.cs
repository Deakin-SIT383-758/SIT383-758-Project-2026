using UnityEngine;
using TMPro;

public class PlaneInfoPanel : MonoBehaviour
{
    #region UI References
    [Header("Identity")]
    [SerializeField] private TMP_Text callsignText;
    [SerializeField] private TMP_Text icaoText;
    [SerializeField] private TMP_Text statusText;

    [Header("Flight Data")]
    [SerializeField] private TMP_Text altitudeText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text trackText;
    [SerializeField] private TMP_Text vertRateText;

    [Header("Position")]
    [SerializeField] private TMP_Text coordinatesText;
    [SerializeField] private TMP_Text lastUpdateText;
    #endregion

    [SerializeField] private PlaneManager planeManager;

    private void Awake()
    {
        planeManager ??= PlaneManager.Instance;
    }

    private void OnEnable()
    {
        if (planeManager == null) return;
        planeManager.OnPlaneSelected += HandlePlaneSelected;

        if (planeManager.Provider != null)
            planeManager.Provider.OnPlaneUpdated += HandlePlaneUpdated;

        Refresh(planeManager.GetSelectedPlane());
    }

    private void OnDisable()
    {
        if (planeManager == null) return;
        planeManager.OnPlaneSelected -= HandlePlaneSelected;

        if (planeManager.Provider != null)
            planeManager.Provider.OnPlaneUpdated -= HandlePlaneUpdated;
    }

    private void HandlePlaneSelected(string _) => Refresh(planeManager.GetSelectedPlane());

    private void HandlePlaneUpdated(PlaneData plane)
    {
        if (plane.hex == planeManager.SelectedPlaneId)
            Refresh(plane);
    }

    #region Refresh Logic

    private void Refresh(PlaneData data)
    {
        if (data == null)
        {
            callsignText?.SetText("");
            icaoText?.SetText("");
            statusText?.SetText("");
            altitudeText?.SetText("");
            speedText?.SetText("");
            trackText?.SetText("");
            vertRateText?.SetText("");
            coordinatesText?.SetText("");
            lastUpdateText?.SetText("");
            return;
        }

        gameObject.SetActive(true);

        callsignText?.SetText(data.flight);
        icaoText?.SetText($"ICAO: {data.hex}");
        altitudeText?.SetText($"{data.ASL:F0} ft ASL  {data.VerticalTrend}");
        speedText?.SetText($"{data.gs:F0} kts GS");
        trackText?.SetText($"{data.track:F0}\u00B0");
        vertRateText?.SetText($"{data.baroRate:F0} ft/min");
        coordinatesText?.SetText($"{data.lat:F4}\u00B0  {data.lon:F4}\u00B0");
        lastUpdateText?.SetText(data.IsStale() ? "STALE" : $"{data.seenPos:F0}s ago");

        if (statusText != null)
        {
            statusText.SetText(data.status.ToString());
            statusText.color = data.status switch
            {
                PlaneStatus.Emergency   => new Color(1f, 0.2f, 0.2f),
                PlaneStatus.CommFailure => new Color(1f, 0.6f, 0f),
                PlaneStatus.Hijack      => new Color(1f, 0.2f, 0.2f),
                _                       => new Color(0.3f, 1f, 0.5f),
            };
        }

        if (lastUpdateText != null)
            lastUpdateText.color = data.IsStale() ? new Color(1f, 0.6f, 0f) : Color.white;
    }

    #endregion

    public void Close() => gameObject.SetActive(false);
}
