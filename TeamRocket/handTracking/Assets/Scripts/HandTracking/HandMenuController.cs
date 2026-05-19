using System.Collections;
using UnityEngine;

namespace OAS.HandTracking
{
    public class HandMenuController : MonoBehaviour
    {
        [SerializeField] private OVRHand leftHand;
        [SerializeField] private OVRSkeleton leftSkeleton;

        [SerializeField] private OVRHand rightHand;
        [SerializeField] private OVRSkeleton rightSkeleton;

        [SerializeField] private GameObject          triggerButton;
        [SerializeField] private GameObject          overlayMenu;
        [SerializeField] private TabletopHandPointer handPointer;
        [SerializeField] private GameObject          teleportInteractor;
        [SerializeField] private MRPassthroughToggle passthroughToggle;

        [SerializeField] private Vector3 palmNormalAxis   = Vector3.down;
        [SerializeField] private bool    invertPalmNormal = false;
        [SerializeField, Range(0f, 1f)] private float palmThreshold = 0.55f;

        [SerializeField] private float        touchRadius     = 0.025f;
        [SerializeField] private float        dwellSeconds    = 0.4f;
        [SerializeField] private float        menuRayDistance = 0.8f;
        [SerializeField] private LineRenderer menuRayLine;

        private Camera mainCamera;

        private Transform leftWrist;
        private Transform leftIndexTip;
        private Transform rightIndexMid;
        private Transform rightIndexTip;

        private bool  isMenuOpen;
        private bool  rightWasPinching;
        private float triggerCooldown;

        private HandMenuButton rayHoveredButton;
        private HandMenuButton rightTouchHovered;
        private HandMenuButton leftTouchHovered;
        private float          rightDwellTimer;
        private float          leftDwellTimer;
        private bool           rightFired;
        private bool           leftFired;

        private readonly Collider[] overlapBuffer = new Collider[8];

        private void Awake() => mainCamera = Camera.main;

        private void Start()
        {
            triggerButton?.SetActive(false);
            overlayMenu?.SetActive(false);

            if (menuRayLine != null)
            {
                menuRayLine.positionCount = 2;
                menuRayLine.enabled       = false;
            }

            if (leftSkeleton  != null) StartCoroutine(WaitForBones(leftSkeleton,  isLeft: true));
            if (rightSkeleton != null) StartCoroutine(WaitForBones(rightSkeleton, isLeft: false));
        }

        private IEnumerator WaitForBones(OVRSkeleton skeleton, bool isLeft)
        {
            while (!skeleton.IsInitialized) yield return null;
            foreach (var bone in skeleton.Bones)
            {
                if (isLeft)
                {
                    if (bone.Id == OVRSkeleton.BoneId.Hand_WristRoot) leftWrist    = bone.Transform;
                    if (bone.Id == OVRSkeleton.BoneId.Hand_Index3)    leftIndexTip = bone.Transform;
                }
                else
                {
                    if (bone.Id == OVRSkeleton.BoneId.Hand_Index2) rightIndexMid = bone.Transform;
                    if (bone.Id == OVRSkeleton.BoneId.Hand_Index3) rightIndexTip = bone.Transform;
                }
            }
        }

        private void Update()
        {
            triggerCooldown -= Time.deltaTime;

            FollowHand();
            GestureUpdate();

            if (!isMenuOpen && triggerButton != null && triggerButton.activeSelf)
                TriggerTouchCheck();

            if (isMenuOpen)
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

        private void FollowHand()
        {
            if (leftWrist == null) return;

            Vector3 palmNormal = GetPalmNormal();

            if (overlayMenu != null)
            {
                overlayMenu.transform.position = leftWrist.position + palmNormal * 0.05f;
                if (mainCamera != null)
                {
                    Vector3 awayFromCamera = (overlayMenu.transform.position - mainCamera.transform.position).normalized;
                    overlayMenu.transform.rotation = Quaternion.LookRotation(awayFromCamera, Vector3.up);
                }
            }

            if (triggerButton != null)
                triggerButton.transform.position = leftWrist.position + palmNormal * 0.03f;
        }

        private void GestureUpdate()
        {
            if (isMenuOpen) { triggerButton?.SetActive(false); return; }

            bool canDetect = leftWrist != null && leftHand != null
                          && leftHand.IsTracked && mainCamera != null;

            if (!canDetect) { triggerButton?.SetActive(false); return; }

            Vector3 toCamera = (mainCamera.transform.position - leftWrist.position).normalized;
            bool    facing   = Vector3.Dot(GetPalmNormal(), toCamera) > palmThreshold;
            triggerButton?.SetActive(facing);
        }

        private Vector3 GetPalmNormal()
        {
            Vector3 axis = invertPalmNormal ? -palmNormalAxis : palmNormalAxis;
            return leftWrist.TransformDirection(axis).normalized;
        }

        private void TriggerTouchCheck()
        {
            if (triggerCooldown > 0f || triggerButton == null) return;

            if (HitsObject(rightIndexTip, triggerButton) || HitsObject(leftIndexTip, triggerButton))
                OpenMenu();
        }

        private bool HitsObject(Transform probe, GameObject target)
        {
            if (probe == null) return false;
            int hitCount = Physics.OverlapSphereNonAlloc(probe.position, touchRadius, overlapBuffer);
            for (int i = 0; i < hitCount; i++)
                if (overlapBuffer[i].gameObject == target) return true;
            return false;
        }

        private void RayUpdate()
        {
            HandMenuButton hit       = null;
            bool           pinchStart = false;

            bool rightHandReady = rightHand != null && rightHand.IsTracked
                               && rightIndexMid != null && rightIndexTip != null;

            if (rightHandReady)
            {
                Vector3 rayDirection = (rightIndexTip.position - rightIndexMid.position).normalized;
                Ray     ray          = new Ray(rightIndexTip.position, rayDirection);

                if (Physics.Raycast(ray, out RaycastHit rayHit, menuRayDistance))
                    hit = rayHit.collider.GetComponent<HandMenuButton>();

                if (menuRayLine != null)
                {
                    menuRayLine.enabled = true;
                    menuRayLine.SetPosition(0, ray.origin);
                    menuRayLine.SetPosition(1, ray.origin + rayDirection * menuRayDistance);
                }

                bool pinching    = rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
                pinchStart       = pinching && !rightWasPinching;
                rightWasPinching = pinching;
            }
            else
            {
                if (menuRayLine != null) menuRayLine.enabled = false;
                rightWasPinching = false;
            }

            if (hit != rayHoveredButton)
            {
                rayHoveredButton?.OnHoverExit();
                rayHoveredButton = hit;
                rayHoveredButton?.OnHoverEnter();
            }

            if (pinchStart && rayHoveredButton != null) rayHoveredButton.Press();
        }

        private void ClearRayHover()
        {
            if (rayHoveredButton == null) return;
            rayHoveredButton.OnHoverExit();
            rayHoveredButton = null;
        }

        private void TouchUpdate()
        {
            bool rightTracked = rightHand != null && rightHand.IsTracked && rightIndexTip != null;

            DoTouch(rightTracked, rightIndexTip, ref rightTouchHovered, ref rightDwellTimer, ref rightFired);
            DoTouch(false,        null,          ref leftTouchHovered,  ref leftDwellTimer,  ref leftFired);
        }

        private void DoTouch(bool tracked, Transform probe,
                              ref HandMenuButton hoveredButton, ref float dwellTimer, ref bool hasFired)
        {
            HandMenuButton found = null;
            if (tracked)
            {
                int hitCount = Physics.OverlapSphereNonAlloc(probe.position, touchRadius, overlapBuffer);
                for (int i = 0; i < hitCount; i++)
                {
                    found = overlapBuffer[i].GetComponent<HandMenuButton>();
                    if (found != null) break;
                }
            }

            if (found != hoveredButton)
            {
                hoveredButton?.OnHoverExit();
                hoveredButton = found;
                dwellTimer    = 0f;
                hasFired      = false;
                hoveredButton?.OnHoverEnter();
            }

            if (hoveredButton != null)
            {
                dwellTimer += Time.deltaTime;
                if (!hasFired && dwellTimer >= dwellSeconds)
                {
                    hasFired = true;
                    hoveredButton.Press();
                }
            }
        }

        public void OpenMenu()
        {
            if (isMenuOpen) return;
            isMenuOpen      = true;
            triggerCooldown = 1f;
            triggerButton?.SetActive(false);
            overlayMenu?.SetActive(true);
        }

        public void CloseMenu()
        {
            isMenuOpen = false;
            overlayMenu?.SetActive(false);
            ClearRayHover();
        }

        public void OnOption1Pressed()
        {
            if (handPointer != null)
                handPointer.enabled = !handPointer.enabled;
            CloseMenu();
        }

        public void OnOption2Pressed() => Debug.Log("[HandMenu] Option 2 pressed.");

        public void OnOption3Pressed()
        {
            if (teleportInteractor != null)
                teleportInteractor.SetActive(!teleportInteractor.activeSelf);
            CloseMenu();
        }

        public void OnOption4Pressed()
        {
            passthroughToggle?.Toggle();
            CloseMenu();
        }

        private void OnDisable()
        {
            ClearRayHover();
            if (menuRayLine != null) menuRayLine.enabled = false;
        }
    }
}
