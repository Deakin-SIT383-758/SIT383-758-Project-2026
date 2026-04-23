using UnityEngine;

namespace MM.RangeInvariantMarkers
{
    public static class UnityMarkerConversion
    {

        public static bool RenderAtUnityWorldPosition(MarkerData markerData, Vector3 playerPos, out Vector3 renderPos, 
            float renderDistance = Mathf.Infinity)
        {
            return RenderAtUnityWorldPosition(markerData, playerPos, out renderPos, renderDistance, new Vector2(0, float.MaxValue));
        }
        public static bool RenderAtUnityWorldPosition(MarkerData markerData, Vector3 playerPos, out Vector3 renderPos, 
            float renderDistance, Vector2 minMaxDisplayDistance)
        {
            renderPos = Vector3.zero;

            Vector3 unityPos = new Vector3((float)markerData.X, (float)markerData.Alt, (float)markerData.Y);
            Vector3 markerDirVec = unityPos - playerPos;
            float dist = markerDirVec.magnitude;

            if (dist < minMaxDisplayDistance.x || dist > minMaxDisplayDistance.y)
            {
                
                return false;
            }

            renderPos = unityPos;
            if (dist < renderDistance) 
                return true;

            //Without the delta everything is rendered at the same distance so we can have some z fighting
            float delta = dist * 0.00001f;
            renderPos = playerPos + markerDirVec.normalized * renderDistance * (1+ delta);
            return true;
        }
    }

    [System.Serializable]
    public class MarkerData
    {
        [SerializeField]
        private double x, y, alt;
        public double X { get => x; }
        public double Y { get => y; }
        public double Alt { get => alt; }


        [SerializeField]
        private string info;

        public string MarkerInfo { get => info; }

        public MarkerData(double x, double y, double alt, string info)
        {
            this.x = x;
            this.y = y;
            this.alt = alt;
            this.info = info;
        }
        public MarkerData(double x, double y, double alt)
        {
            this.x = x;
            this.y = y;
            this.alt = alt;
            this.info = "";
        }

        public void SetMarkerinfo(string info)
        { 
            this.info = info; 
        }

    }

    public class MarkerManager : MonoBehaviour
    {
        public GameObject playerObject;
        public MarkerData[] allMarkerData;
        private System.Collections.Generic.Dictionary<MarkerData, IMarkerVisuals> markers = new System.Collections.Generic.Dictionary<MarkerData, IMarkerVisuals>();
        private System.Collections.Generic.Dictionary<MarkerData, GameObject> markerGOs = new System.Collections.Generic.Dictionary<MarkerData, GameObject>();
        [SerializeField] private GameObject markerPrefab;
        [SerializeField] private IMarkerVisuals.VisualTimers visualEffectTimers;

        public float renderDistance = 15f;
        public Vector2 minMaxDisplayDistance = new Vector2 (10, 10000);

        private void Start()
        {
            if (markerPrefab == null)
            {
                Debug.LogError("No prefab assigned");
                this.enabled = false;
                return;
            }

            foreach (var markerData in allMarkerData)
            {
                SetupMarker(markerData);
            }
        }

        private void SetupMarker(MarkerData markerData)
        {
            var markerObject = Instantiate(markerPrefab);
            markerObject.name = "Marker!";

            IMarkerVisuals markerVisualsInterface = markerObject.GetComponent<IMarkerVisuals>();
            if (markerVisualsInterface == null)
            {
                Debug.LogError($"No IMarkerVisuals on {markerObject.gameObject.name}");
                Destroy(markerObject);
                return;
            }

            markerVisualsInterface.SetMessage(markerData.MarkerInfo);
            markerVisualsInterface.SetTimers(visualEffectTimers);
            markers.Add(markerData, markerVisualsInterface);
            markerGOs.Add(markerData, markerObject);

        }


        private void LateUpdate()
        {
            foreach (var markerData in allMarkerData)
            {
                var markerObject = markerGOs[markerData];
                Vector3 renderPos = Vector3.zero;
                bool render = UnityMarkerConversion.RenderAtUnityWorldPosition(markerData, playerObject.transform.position, 
                    out renderPos, renderDistance, minMaxDisplayDistance);
                if (render)
                {
                    markers[markerData].SetVisualsEnabled(true);
                    markerObject.transform.position = renderPos;
                    markers[markerData].UpdateVisuals(playerObject.transform.position);
                } else
                {
                    markers[markerData].SetVisualsEnabled(false);
                }
            }
        }

        private void OnValidate()
        {
            if (markerPrefab != null)
            {
                IMarkerVisuals markerVisualsInterface = markerPrefab.GetComponent<IMarkerVisuals>();
                if (markerVisualsInterface != null) return;

                Debug.LogError($"Error: {markerPrefab.gameObject.name} is not of type IMarkerVisuals. Please ensure it implements this interface");
                markerPrefab = null;
            }
        }
    }

}