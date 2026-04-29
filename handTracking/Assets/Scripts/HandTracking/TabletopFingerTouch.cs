using System;
using System.Collections;
using UnityEngine;

namespace OAS.HandTracking
{
    public class TabletopFingerTouch : MonoBehaviour
    {
        [SerializeField] private OVRHand     rightHand;
        [SerializeField] private OVRSkeleton rightSkeleton;

        [SerializeField] private OVRHand     leftHand;
        [SerializeField] private OVRSkeleton leftSkeleton;

        [SerializeField, Min(0.005f)] private float touchRadius  = 0.03f;

        [SerializeField, Min(0f)]    private float dwellSeconds  = 0.4f;

        public event Action<TabletopHotspot> OnHotspotSelected;

        private Transform _rIndex3, _lIndex3;

        private TabletopHotspot _rHovered, _lHovered;
        private float _rDwellTimer, _lDwellTimer;
        private bool  _rFired, _lFired;

        private readonly Collider[] _overlapBuffer = new Collider[8];

        private void Start()
        {
            if (rightSkeleton != null) StartCoroutine(WaitForBones(rightSkeleton, isRight: true));
            if (leftSkeleton  != null) StartCoroutine(WaitForBones(leftSkeleton,  isRight: false));
        }

        private IEnumerator WaitForBones(OVRSkeleton skeleton, bool isRight)
        {
            while (!skeleton.IsInitialized) yield return null;

            Transform found = null;
            foreach (var bone in skeleton.Bones)
            {
                if (bone.Id == OVRSkeleton.BoneId.Hand_Index3)
                {
                    found = bone.Transform;
                    break;
                }
            }

            string side = isRight ? "Right" : "Left";
            if (found == null)
            {
                Debug.LogWarning($"[FingerTouch] {side} Index3 not found. Bones: {skeleton.Bones.Count}");
                yield break;
            }
            if (isRight) _rIndex3 = found;
            else         _lIndex3 = found;
            Debug.Log($"[FingerTouch] {side} Index3 found.");
        }

        private void Update()
        {
            ProcessHand(rightHand, _rIndex3, ref _rHovered, ref _rDwellTimer, ref _rFired);
            ProcessHand(leftHand,  _lIndex3, ref _lHovered, ref _lDwellTimer, ref _lFired);
        }

        private void ProcessHand(
            OVRHand hand, Transform index3,
            ref TabletopHotspot hovered, ref float dwellTimer, ref bool fired)
        {
            bool tracked = hand != null && hand.IsTracked && index3 != null;
            if (!tracked)
            {
                SetHovered(ref hovered, ref dwellTimer, ref fired, null);
                return;
            }

            int count = Physics.OverlapSphereNonAlloc(index3.position, touchRadius, _overlapBuffer);
            TabletopHotspot found = null;
            for (int i = 0; i < count; i++)
            {
                found = _overlapBuffer[i].GetComponent<TabletopHotspot>();
                if (found != null) break;
            }

            SetHovered(ref hovered, ref dwellTimer, ref fired, found);

            if (hovered != null)
            {
                dwellTimer += Time.deltaTime;
                if (!fired && dwellTimer >= dwellSeconds)
                {
                    fired = true;
                    OnHotspotSelected?.Invoke(hovered);
                }
            }
        }

        private void SetHovered(
            ref TabletopHotspot hovered, ref float dwellTimer, ref bool fired,
            TabletopHotspot next)
        {
            if (next == hovered) return;
            hovered?.OnHoverExit();
            hovered    = next;
            dwellTimer = 0f;
            fired      = false;
            hovered?.OnHoverEnter();
        }
    }
}
