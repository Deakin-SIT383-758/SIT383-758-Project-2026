using System;
using System.Collections;
using UnityEngine;

namespace OAS.HandTracking
{
    public class TabletopHandPointer : MonoBehaviour
    {
        [SerializeField] private OVRHand rightHand;
        [SerializeField] private OVRSkeleton rightSkeleton;
        [SerializeField] private Transform rightControllerAnchor;
        [SerializeField] private LineRenderer rightRayLine;

        [SerializeField] private OVRHand leftHand;
        [SerializeField] private OVRSkeleton leftSkeleton;
        [SerializeField] private Transform leftControllerAnchor;
        [SerializeField] private LineRenderer leftRayLine;

        [SerializeField] private float maxRayDistance = 2f;

        public event Action<TabletopHotspot> OnHotspotSelected;

        private Transform rightIndexMid;
        private Transform rightIndexTip;
        private Transform leftIndexMid;
        private Transform leftIndexTip;
        private bool rightWasPinching;
        private bool leftWasPinching;
        private TabletopHotspot rightHovered;
        private TabletopHotspot leftHovered;

        private void Start()
        {
            if (rightSkeleton != null) StartCoroutine(WaitForBones(rightSkeleton, isRight: true));
            if (leftSkeleton  != null) StartCoroutine(WaitForBones(leftSkeleton,  isRight: false));
        }

        private IEnumerator WaitForBones(OVRSkeleton skeleton, bool isRight)
        {
            while (!skeleton.IsInitialized) yield return null;

            Transform indexMid = null;
            Transform indexTip = null;
            foreach (var bone in skeleton.Bones)
            {
                if      (bone.Id == OVRSkeleton.BoneId.Hand_Index2) indexMid = bone.Transform;
                else if (bone.Id == OVRSkeleton.BoneId.Hand_Index3) indexTip = bone.Transform;
            }

            string side = isRight ? "Right" : "Left";
            if (indexMid != null && indexTip != null)
            {
                if (isRight) { rightIndexMid = indexMid; rightIndexTip = indexTip; }
                else         { leftIndexMid  = indexMid; leftIndexTip  = indexTip; }
                Debug.Log($"[HandPointer] {side} hand bones ready.");
            }
            else
            {
                Debug.LogWarning($"[HandPointer] {side} hand Index2/Index3 not found. " +
                                 $"Bones: {skeleton.Bones.Count}");
            }
        }

        private void Update()
        {
            ProcessHand(rightHand, rightIndexMid, rightIndexTip, rightControllerAnchor, rightRayLine,
                        isRight: true,  ref rightHovered, ref rightWasPinching);
            ProcessHand(leftHand,  leftIndexMid,  leftIndexTip,  leftControllerAnchor,  leftRayLine,
                        isRight: false, ref leftHovered,  ref leftWasPinching);
        }

        private void ProcessHand(
            OVRHand hand, Transform indexMid, Transform indexTip,
            Transform anchor, LineRenderer rayLine, bool isRight,
            ref TabletopHotspot hovered, ref bool wasPinching)
        {
            bool usingHand = hand != null && hand.IsTracked;

            if (!TryGetRay(usingHand, indexMid, indexTip, anchor, out Ray ray))
            {
                ClearHover(ref hovered);
                SetRayVisual(rayLine, false);
                return;
            }

            SetRayVisual(rayLine, true, ray);

            TabletopHotspot newHover = Physics.Raycast(ray, out RaycastHit rayHit, maxRayDistance)
                ? rayHit.collider.GetComponent<TabletopHotspot>()
                : null;

            if (newHover != hovered)
            {
                hovered?.OnHoverExit();
                hovered = newHover;
                hovered?.OnHoverEnter();
            }

            bool selected = usingHand
                ? DetectPinchStart(hand, ref wasPinching)
                : DetectTriggerPress(isRight);

            if (selected && hovered != null)
                OnHotspotSelected?.Invoke(hovered);
        }

        private static bool TryGetRay(bool usingHand, Transform indexMid, Transform indexTip,
            Transform anchor, out Ray ray)
        {
            if (usingHand && indexMid != null)
            {
                ray = new Ray(indexTip.position, (indexTip.position - indexMid.position).normalized);
                return true;
            }
            if (!usingHand && anchor != null)
            {
                ray = new Ray(anchor.position, anchor.forward);
                return true;
            }
            ray = default;
            return false;
        }

        private void ClearHover(ref TabletopHotspot hovered)
        {
            if (hovered == null) return;
            hovered.OnHoverExit();
            hovered = null;
        }

        private static bool DetectPinchStart(OVRHand hand, ref bool wasPinching)
        {
            bool pinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
            bool started  = pinching && !wasPinching;
            wasPinching   = pinching;
            return started;
        }

        private static bool DetectTriggerPress(bool isRight) => OVRInput.GetDown(
            OVRInput.Button.PrimaryIndexTrigger,
            isRight ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch);

        private void SetRayVisual(LineRenderer rayLine, bool active, Ray ray = default)
        {
            if (rayLine == null) return;
            rayLine.enabled = active;
            if (!active) return;
            rayLine.SetPosition(0, ray.origin);
            rayLine.SetPosition(1, ray.origin + ray.direction * maxRayDistance);
        }

        private void OnDisable()
        {
            ClearHover(ref rightHovered);
            ClearHover(ref leftHovered);
            rightWasPinching = false;
            leftWasPinching  = false;
            SetRayVisual(rightRayLine, false);
            SetRayVisual(leftRayLine,  false);
        }
    }
}
