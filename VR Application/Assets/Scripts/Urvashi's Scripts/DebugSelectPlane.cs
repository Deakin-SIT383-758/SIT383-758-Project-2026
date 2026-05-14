using System.Linq;
using UnityEngine;

public class DebugSelectFirstPlane : MonoBehaviour
{
    [SerializeField] private float delay = 1f;
    private void Start() => Invoke(nameof(Pick), delay);
    private void Pick()
    {
        var mgr = PlaneManager.Instance;
        if (mgr?.ActivePlanes == null || mgr.ActivePlanes.Count == 0) return;
        mgr.SelectPlane(mgr.ActivePlanes.Keys.First());
    }
}