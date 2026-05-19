using UnityEngine;
using Meta.XR.MRUtilityKit;

namespace OAS.HandTracking
{
    public class QRTableAnchor : MonoBehaviour
    {
        [SerializeField] private Transform  tableRoot;
        [SerializeField] private GameObject mrukGameObject;
        [SerializeField] private GameObject virtualFloor;
        [SerializeField] private float      tableSurfaceLocalY = 0.785f;

        private void OnEnable()
        {
            if (MRUK.Instance == null) return;
            MRUK.Instance.SceneLoadedEvent.AddListener(OnSceneLoaded);
            MRUK.Instance.RoomUpdatedEvent.AddListener(OnRoomUpdated);
            var room = MRUK.Instance.GetCurrentRoom();
            if (room != null) ScanRoom(room);
        }

        private void OnDisable()
        {
            if (MRUK.Instance == null) return;
            MRUK.Instance.SceneLoadedEvent.RemoveListener(OnSceneLoaded);
            MRUK.Instance.RoomUpdatedEvent.RemoveListener(OnRoomUpdated);
        }

        private void OnSceneLoaded()
        {
            var room = MRUK.Instance?.GetCurrentRoom();
            if (room != null) ScanRoom(room);
        }

        private void OnRoomUpdated(MRUKRoom room) => ScanRoom(room);

        private void ScanRoom(MRUKRoom room)
        {
            if (room == null) return;

            if (tableRoot != null)
            {
                var        camPos   = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
                MRUKAnchor best     = null;
                float      bestDist = float.MaxValue;

                foreach (var anchor in room.Anchors)
                {
                    if (!anchor.HasLabel("TABLE")) continue;
                    float d = Vector3.Distance(anchor.transform.position, camPos);
                    if (d < bestDist) { bestDist = d; best = anchor; }
                }

                if (best != null)
                {
                    var pos = best.transform.position;
                    var rot = best.transform.rotation;
                    tableRoot.position = new Vector3(pos.x, pos.y - tableSurfaceLocalY, pos.z);
                    tableRoot.rotation = Quaternion.Euler(0f, rot.eulerAngles.y, 0f);
                    Debug.Log($"[OAS] Table anchored to physical surface at {pos}");
                }
            }

            if (virtualFloor != null)
            {
                foreach (var anchor in room.Anchors)
                {
                    if (!anchor.HasLabel("FLOOR")) continue;
                    var p = virtualFloor.transform.position;
                    virtualFloor.transform.position = new Vector3(p.x, anchor.transform.position.y, p.z);
                    break;
                }
            }
        }
    }
}
