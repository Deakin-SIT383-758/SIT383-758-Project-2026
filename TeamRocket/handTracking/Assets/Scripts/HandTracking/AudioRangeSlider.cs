using System.Collections;
using TMPro;
using UnityEngine;

namespace OAS.HandTracking
{
    public class AudioRangeSlider : MonoBehaviour
    {
        [SerializeField] private CabinAudioManager audioManager;
        [SerializeField] private OVRSkeleton leftSkeleton;
        [SerializeField] private OVRSkeleton rightSkeleton;

        [SerializeField] private Transform trackTransform;
        [SerializeField] private Transform fill;
        [SerializeField] private Transform handle;
        [SerializeField] private TMP_Text  valueLabel;

        [SerializeField] private float minRange     = 0.5f;
        [SerializeField] private float maxRange     = 10.0f;
        [SerializeField] private float initialRange = 5.0f;
        [SerializeField] private float trackHalfLen = 0.06f;
        [SerializeField] private float touchRadius  = 0.025f;

        private Transform rightIndexTip;
        private Transform leftIndexTip;
        private float sliderValue;

        private readonly Collider[] overlapBuffer = new Collider[8];

        private void Start()
        {
            sliderValue = Mathf.InverseLerp(minRange, maxRange, initialRange);

            if (leftSkeleton  != null) StartCoroutine(WaitForBones(leftSkeleton,  isLeft: true));
            if (rightSkeleton != null) StartCoroutine(WaitForBones(rightSkeleton, isLeft: false));

            UpdateVisuals();
        }

        private IEnumerator WaitForBones(OVRSkeleton skeleton, bool isLeft)
        {
            while (!skeleton.IsInitialized) yield return null;
            foreach (var bone in skeleton.Bones)
            {
                if (bone.Id != OVRSkeleton.BoneId.Hand_Index3) continue;
                if (isLeft) leftIndexTip  = bone.Transform;
                else        rightIndexTip = bone.Transform;
                break;
            }
        }

        private void Update()
        {
            if (TrySample(rightIndexTip) || TrySample(leftIndexTip))
                UpdateVisuals();
        }

        private bool TrySample(Transform probe)
        {
            if (probe == null || trackTransform == null) return false;

            int hitCount = Physics.OverlapSphereNonAlloc(probe.position, touchRadius, overlapBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                if (overlapBuffer[i].transform != trackTransform) continue;

                Vector3 trackLeft  = trackTransform.position - trackTransform.right * trackHalfLen;
                float   projected  = Vector3.Dot(probe.position - trackLeft, trackTransform.right);
                sliderValue = Mathf.Clamp01(projected / (trackHalfLen * 2f));
                return true;
            }
            return false;
        }

        private void UpdateVisuals()
        {
            float range     = Mathf.Lerp(minRange, maxRange, sliderValue);
            float fillWidth = Mathf.Max(0.001f, sliderValue * trackHalfLen * 2f);

            if (trackTransform != null)
            {
                Vector3 trackLeft  = trackTransform.position - trackTransform.right * trackHalfLen;
                Vector3 rightAxis  = trackTransform.right;

                if (fill != null)
                {
                    fill.position   = trackLeft + rightAxis * (fillWidth * 0.5f);
                    Vector3 scale   = fill.localScale;
                    fill.localScale = new Vector3(fillWidth, scale.y, scale.z);
                }

                if (handle != null)
                    handle.position = trackLeft + rightAxis * fillWidth;
            }

            if (valueLabel != null)
                valueLabel.text = $"Range: {range:F2} m";

            audioManager?.SetMaxHearingRange(range);
        }
    }
}
