using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlaneSelector : MonoBehaviour
{
    #region Inspector
    [SerializeField] private Transform  scrollContent;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private PlaneManager planeManager;

    [Header("Colors")]
    [SerializeField] private Color normalColor    = Color.white;
    [SerializeField] private Color selectedColor  = new Color(0.25f, 0.65f, 1f);
    [SerializeField] private Color emergencyColor = new Color(1f, 0.2f, 0.2f);
    [SerializeField] private Color staleColor     = new Color(0.5f, 0.5f, 0.5f);
    #endregion

    private readonly Dictionary<string, Button> _buttons = new();
    private IPlaneDataProvider _provider;

    private void Awake()
    {
        planeManager ??= PlaneManager.Instance;
    }

    private void OnEnable()
    {
        if (planeManager == null) return;
        _provider = planeManager.Provider;
        
        if (_provider != null)
        {
            _provider.OnPlaneUpdated += HandlePlaneUpdated;
            _provider.OnPlaneRemoved += HandlePlaneRemoved;
        }

        planeManager.OnPlaneSelected += HandleSelectionChanged;
        RebuildList();
    }

    private void OnDisable()
    {
        if (_provider != null)
        {
            _provider.OnPlaneUpdated -= HandlePlaneUpdated;
            _provider.OnPlaneRemoved -= HandlePlaneRemoved;
        }

        if (planeManager != null)
            planeManager.OnPlaneSelected -= HandleSelectionChanged;
    }

    #region Event Handlers

    private void HandlePlaneUpdated(PlaneData plane)
    {
        if (_buttons.TryGetValue(plane.hex, out var btn))
        {
            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = FormatLabel(plane);
            UpdateButtonColor(plane.hex, btn, plane);
        }
        else
        {
            AddButton(plane);
        }
    }

    private void HandlePlaneRemoved(string planeId)
    {
        if (!_buttons.TryGetValue(planeId, out var btn)) return;
        _buttons.Remove(planeId);
        Destroy(btn.gameObject);
    }

    private void HandleSelectionChanged(string _) => RefreshHighlights();

    #endregion

    #region List Management

    private void RebuildList()
    {
        foreach (var btn in _buttons.Values)
            if (btn != null) Destroy(btn.gameObject);
        _buttons.Clear();

        if (_provider == null) return;
        foreach (var (_, plane) in _provider.ActivePlanes)
            AddButton(plane);
    }

    private void AddButton(PlaneData plane)
    {
        if (buttonPrefab == null || scrollContent == null) return;

        var go  = Instantiate(buttonPrefab, scrollContent);
        var btn = go.GetComponent<Button>();
        if (btn == null) { Destroy(go); return; }

        var label = go.GetComponentInChildren<TMP_Text>();
        if (label != null) label.text = FormatLabel(plane);

        string capturedId = plane.hex;
        btn.onClick.AddListener(() => planeManager.SelectPlane(capturedId));

        _buttons[plane.hex] = btn;
        RefreshHighlights();
    }

    private void RefreshHighlights()
    {
        string sel = planeManager?.SelectedPlaneId;
        if (_provider == null) return;

        foreach (var (id, btn) in _buttons)
        {
            if (btn == null) continue;
            _provider.ActivePlanes.TryGetValue(id, out var plane);
            UpdateButtonColor(id, btn, plane);
        }
    }

    private void UpdateButtonColor(string id, Button btn, PlaneData plane)
    {
        string sel = planeManager?.SelectedPlaneId;
        var colors = btn.colors;

        Color baseColor;
        if (id == sel)
            baseColor = selectedColor;
        else if (plane != null && plane.status != PlaneStatus.Normal)
            baseColor = emergencyColor;
        else if (plane != null && plane.IsStale())
            baseColor = staleColor;
        else
            baseColor = normalColor;

        colors.normalColor      = baseColor;
        colors.selectedColor    = baseColor;                 
        colors.highlightedColor = baseColor * 1.1f;           //slightly brighter on hover
        colors.pressedColor     = baseColor * 0.85f;          //slightly darker on press

        btn.colors = colors;
    }

    private static string FormatLabel(PlaneData p)
    {
        string alt = $"FL{p.ASL / 100f:F0}";
        if (p.ASL < 10000f) alt = $"{p.ASL:F0}ft";

        string trend = p.VerticalTrend;
        string warn = p.status != PlaneStatus.Normal ? " \u26A0" : "";

        return $"{p.flight}  {alt} {trend}{warn}";
    }

    #endregion
}


