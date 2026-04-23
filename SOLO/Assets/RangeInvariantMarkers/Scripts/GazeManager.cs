using UnityEngine;

namespace MM.RangeInvariantMarkers
{
    public class GazeManager : MonoBehaviour
    {
        public Camera cam;
        public LayerMask markerLayer;
        private Ray gazeRay;

        private bool rayHit;
        private GameObject hitGO;
        private Vector3 hitPoint;


        private void Update()
        {
            gazeRay = new Ray(cam.transform.position, cam.transform.forward);
            rayOrigin = gazeRay.origin;
            rayDirection = gazeRay.direction;
            rayHit = Physics.Raycast(gazeRay, out RaycastHit hitInfo, 10000f, markerLayer);
            if (rayHit)
            {
                hitGO = hitInfo.collider.gameObject;
                hitPoint = hitInfo.point;
                Debug.Log("Hit: " + hitInfo.collider.name);
                ProcessGaze(hitGO);
            }
        }

        public void ProcessGaze(GameObject hitObject)
        {
            IMarkerVisuals markerVisuals = hitObject.GetComponent<IMarkerVisuals>();
            if (markerVisuals == null)
            {
                markerVisuals = hitObject.GetComponentInParent<IMarkerVisuals>();
            }
            if (markerVisuals == null)
            {
                rayHit = false;
                return;
            }

            markerVisuals.ProcessGaze();
        }

        public Vector3 rayOrigin, rayDirection;
        private void OnDrawGizmos()
        {
            if (rayHit)
            {
                Debug.DrawLine(gazeRay.origin, hitPoint, Color.blue);
            }
            else
            {
                Debug.DrawRay(gazeRay.origin, gazeRay.direction, Color.red);
            }
        }
    }
}