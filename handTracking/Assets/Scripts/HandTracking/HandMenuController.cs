using System.Collections;
using UnityEngine;

namespace OAS.HandTracking
{
    public class HandMenuController : MonoBehaviour
    {
        [SerializeField] private OVRHand     leftHand;
        [SerializeField] private OVRSkeleton leftSkeleton;

        [SerializeField] private OVRHand     rightHand;
        [SerializeField] private OVRSkeleton rightSkeleton;

        [SerializeField] private GameObject triggerButton;
        [SerializeField] private GameObject overlayMenu;

        [SerializeField] private Vector3 palmNormalAxis  = Vector3.down;
        [SerializeField] private bool    invertPalmNormal = false;
        [SerializeField, Range(0f, 1f)] private float palmThreshold = 0.55f;

        [SerializeField] private float        touchRadius     = 0.025f;
        [SerializeField] private float        dwellSeconds    = 0.4f;
        [SerializeField] private float        menuRayDistance = 0.8f;
        [SerializeField] private LineRenderer menuRayLine;

        // ── Internal state ───────────────────────────────────────────────────────

        private Camera _cam;

        private Transform _lWrist,  _lIndex3;
        private Transform _rIndex2, _rIndex3;

        private bool  _menuOpen;
        private bool  _rWasPinching;
        private float _triggerCooldown;

        private HandMenuButton _rayHovered;
        private HandMenuButton _rTouchHov, _lTouchHov;
        private float          _rDwell,    _lDwell;
        private bool           _rFired,    _lFired;

        private readonly Collider[] _hits = new Collider[8];

        private void Awake() => _cam = Camera.main;

        private void Start()
        {
            triggerButton?.SetActive(false);
            overlayMenu?.SetActive(false);

            if (menuRayLine != null)
            {
                menuRayLine.positionCount = 2;
                menuRayLine.enabled       = false;
            }

            if (leftSkeleton  != null) StartCoroutine(WaitBones(leftSkeleton,  isLeft: true));
            if (rightSkeleton != null) StartCoroutine(WaitBones(rightSkeleton, isLeft: false));
        }

        private IEnumerator WaitBones(OVRSkeleton sk, bool isLeft)
        {
            while (!sk.IsInitialized) yield return null;
            foreach (var b in sk.Bones)
            {
                if (isLeft)
                {
                    if (b.Id == OVRSkeleton.BoneId.Hand_WristRoot) _lWrist  = b.Transform;
                    if (b.Id == OVRSkeleton.BoneId.Hand_Index3)    _lIndex3 = b.Transform;
                }
                else
                {
                    if (b.Id == OVRSkeleton.BoneId.Hand_Index2) _rIndex2 = b.Transform;
                    if (b.Id == OVRSkeleton.BoneId.Hand_Index3) _rIndex3 = b.Transform;
                }
            }
        }

        private void Update()
        {
            _triggerCooldown -= Time.deltaTime;

            FollowHand();
            GestureUpdate();

            if (!_menuOpen && triggerButton != null && triggerButton.activeSelf)
                TriggerTouchCheck();

            if (_menuOpen)
            {
                RayUpdate();
                TouchUpdate();
            }
            else
            {
                if (menuRayLine != null) menuRayLine.enabled = false;
                ClearRayHover();
            }
        }

        // ── Hand follow ──────────────────────────────────────────────────────────

        private void FollowHand()
        {
            if (_lWrist == null) return;

            Vector3 palmNorm = PalmNormal();

            if (overlayMenu != null)
            {
                overlayMenu.transform.position = _lWrist.position + palmNorm * 0.05f;
                overlayMenu.transform.rotation = Quaternion.LookRotation(-palmNorm, Vector3.up);
            }

            if (triggerButton != null)
            {
                triggerButton.transform.position = _lWrist.position + palmNorm * 0.03f;
            }
        }

        // ── Gesture ──────────────────────────────────────────────────────────────

        private void GestureUpdate()
        {
            if (_menuOpen) { triggerButton?.SetActive(false); return; }

            bool canDetect = _lWrist != null && leftHand != null
                          && leftHand.IsTracked && _cam != null;

            if (!canDetect) { triggerButton?.SetActive(false); return; }

            Vector3 toCamera = (_cam.transform.position - _lWrist.position).normalized;
            bool    facing   = Vector3.Dot(PalmNormal(), toCamera) > palmThreshold;
            triggerButton?.SetActive(facing);
        }

        private Vector3 PalmNormal()
        {
            var axis = invertPalmNormal ? -palmNormalAxis : palmNormalAxis;
            return _lWrist.TransformDirection(axis).normalized;
        }

        // ── Trigger button ───────────────────────────────────────────────────────

        private void TriggerTouchCheck()
        {
            if (_triggerCooldown > 0f || triggerButton == null) return;

            if (HitsObject(_rIndex3, triggerButton) || HitsObject(_lIndex3, triggerButton))
                OpenMenu();
        }

        private bool HitsObject(Transform probe, GameObject target)
        {
            if (probe == null) return false;
            int n = Physics.OverlapSphereNonAlloc(probe.position, touchRadius, _hits);
            for (int i = 0; i < n; i++)
                if (_hits[i].gameObject == target) return true;
            return false;
        }

        // ── Ray interaction (right hand, menu open) ──────────────────────────────

        private void RayUpdate()
        {
            HandMenuButton hit       = null;
            bool           pinchStart = false;

            bool rReady = rightHand != null && rightHand.IsTracked
                       && _rIndex2  != null && _rIndex3 != null;

            if (rReady)
            {
                var dir = (_rIndex3.position - _rIndex2.position).normalized;
                var ray = new Ray(_rIndex3.position, dir);

                if (Physics.Raycast(ray, out RaycastHit rh, menuRayDistance))
                    hit = rh.collider.GetComponent<HandMenuButton>();

                if (menuRayLine != null)
                {
                    menuRayLine.enabled = true;
                    menuRayLine.SetPosition(0, ray.origin);
                    menuRayLine.SetPosition(1, ray.origin + dir * menuRayDistance);
                }

                bool pinching = rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
                pinchStart    = pinching && !_rWasPinching;
                _rWasPinching = pinching;
            }
            else
            {
                if (menuRayLine != null) menuRayLine.enabled = false;
                _rWasPinching = false;
            }

            if (hit != _rayHovered)
            {
                _rayHovered?.OnHoverExit();
                _rayHovered = hit;
                _rayHovered?.OnHoverEnter();
            }

            if (pinchStart && _rayHovered != null) _rayHovered.Press();
        }

        private void ClearRayHover()
        {
            if (_rayHovered == null) return;
            _rayHovered.OnHoverExit();
            _rayHovered = null;
        }

        // ── Touch interaction (both hands, menu open) ────────────────────────────

        private void TouchUpdate()
        {
            bool rT = rightHand != null && rightHand.IsTracked && _rIndex3 != null;
            bool lT = leftHand  != null && leftHand.IsTracked  && _lIndex3 != null;

            DoTouch(rT, _rIndex3, ref _rTouchHov, ref _rDwell, ref _rFired);
            DoTouch(lT, _lIndex3, ref _lTouchHov, ref _lDwell, ref _lFired);
        }

        private void DoTouch(bool tracked, Transform probe,
                              ref HandMenuButton hov, ref float dwell, ref bool fired)
        {
            HandMenuButton found = null;
            if (tracked)
            {
                int n = Physics.OverlapSphereNonAlloc(probe.position, touchRadius, _hits);
                for (int i = 0; i < n; i++)
                {
                    found = _hits[i].GetComponent<HandMenuButton>();
                    if (found != null) break;
                }
            }

            if (found != hov)
            {
                hov?.OnHoverExit();
                hov   = found;
                dwell = 0f;
                fired = false;
                hov?.OnHoverEnter();
            }

            if (hov != null)
            {
                dwell += Time.deltaTime;
                if (!fired && dwell >= dwellSeconds) { fired = true; hov.Press(); }
            }
        }

        // ── Public API ───────────────────────────────────────────────────────────

        public void OpenMenu()
        {
            if (_menuOpen) return;
            _menuOpen        = true;
            _triggerCooldown = 1f;
            triggerButton?.SetActive(false);
            overlayMenu?.SetActive(true);
        }

        public void CloseMenu()
        {
            _menuOpen = false;
            overlayMenu?.SetActive(false);
            ClearRayHover();
        }

        public void OnOption1Pressed() => Debug.Log("[HandMenu] Option 1 pressed.");
        public void OnOption2Pressed() => Debug.Log("[HandMenu] Option 2 pressed.");
        public void OnOption3Pressed() => Debug.Log("[HandMenu] Option 3 pressed.");

        private void OnDisable()
        {
            ClearRayHover();
            if (menuRayLine != null) menuRayLine.enabled = false;
        }
    }
}
