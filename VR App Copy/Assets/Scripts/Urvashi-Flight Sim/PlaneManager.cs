using System;
using System.Collections.Generic;
using UnityEngine;

public class PlaneManager : MonoBehaviour
{
    #region Singleton
    public static PlaneManager Instance { get; private set; }
    #endregion

    [SerializeField] private MockPlaneDataProvider dataProvider;

    private IPlaneDataProvider _provider;
    private string _selectedPlaneId;

    public IPlaneDataProvider Provider => _provider;
    public IReadOnlyDictionary<string, PlaneData> ActivePlanes => _provider?.ActivePlanes;

    public event Action<string> OnPlaneSelected;

    public string SelectedPlaneId
    {
        get => _selectedPlaneId;
        set
        {
            if (_selectedPlaneId == value) return;
            _selectedPlaneId = value;
            OnPlaneSelected?.Invoke(_selectedPlaneId);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _provider = dataProvider;
    }
    

    #region Public API

    public void SelectPlane(string id) => SelectedPlaneId = id;

    public PlaneData GetSelectedPlane()
    {
        if (_selectedPlaneId == null || _provider == null) return null;
        _provider.ActivePlanes.TryGetValue(_selectedPlaneId, out var plane);
        return plane;
    }

    #endregion
}
