using System.Collections;
using TMPro;
using UnityEngine;

namespace OAS.HandTracking
{
    public class AudioRangeSlider : MonoBehaviour
    {
        [SerializeField] private CabinAudioManager audioManager;
        [SerializeField] private OVRSkeleton        leftSkeleton;
        [SerializeField] private OVRSkeleton        rightSkeleton;

        [SerializeField] private Transform trackTransform;
        [SerializeField] private Transform fill;
        [SerializeField] private Transform handle;
        [SerializeField] private TMP_Text  valueLabel;

        [SerializeField] private float minRange     = 0.5f;
        [SerializeField] private float maxRange     = 10.0f;
        [SerializeField] private float initialRange = 5.0f;
        [SerializeField] private float trackHalfLen = 0.06f; 
        [SerializeField] private float touchRadius  = 0.025f;

        private Transform _rIndex3, _lIndex3;
        private float _t;
        private readonly Collider[] _hits = new Collider[8];

        private void Start()
        {
            _t = Mathf.InverseLerp(minRange, maxRange, initialRange);

            if (leftSkeleton  != null) StartCoroutine(WaitBones(leftSkeleton,  isLeft: true));
            if (rightSkeleton != null) StartCoroutine(WaitBones(rightSkeleton, isLeft: false));

            UpdateVisuals();
        }

        private IEnumerator WaitBones(OVRSkeleton sk, bool isLeft)
        {
            while (!sk.IsInitialized) yield return null;
            foreach (var b in sk.Bones)
            {
                if (b.Id != OVRSkeleton.BoneId.Hand_Index3) continue;
                if (isLeft) _lIndex3 = b.Transform;
                else        _rIndex3 = b.Transform;
                break;
            }
        }

        private void Update()
        {
            if (TrySample(_rIndex3) || TrySample(_lIndex3))
                UpdateVisuals();
        }

        private bool TrySample(Transform probe)
        {
            if (probe == null || trackTransform == null) return false;

            int n = Physics.OverlapSphereNonAlloc(probe.position, touchRadius, _hits);
            for (int i = 0; i < n; i++)
            {
                if (_hits[i].transform != trackTransform) continue;

                // Project finger position onto the track's world-space axis
                Vector3 trackLeft = trackTransform.position - trackTransform.right * trackHalfLen;
                float   projected = Vector3.Dot(probe.position - trackLeft, trackTransform.right);
                _t = Mathf.Clamp01(projected / (trackHalfLen * 2f));
                return true;
            }
            return false;
        }

        private void UpdateVisuals()
        {
            float range     = Mathf.Lerp(minRange, maxRange, _t);
            float fillWidth = Mathf.Max(0.001f, _t * trackHalfLen * 2f);

            if (trackTransform != null)
            {
                Vector3 trackLeft = trackTransform.position - trackTransform.right * trackHalfLen;
                Vector3 right     = trackTransform.right;

                if (fill != null)
                {
                    fill.position   = trackLeft + right * (fillWidth * 0.5f);
                    var s           = fill.localScale;
                    fill.localScale = new Vector3(fillWidth, s.y, s.z);
                }

                if (handle != null)
                    handle.position = trackLeft + right * fillWidth;
            }

            if (valueLabel != null)
                valueLabel.text = $"Range: {range:F2} m";

            audioManager?.SetMaxHearingRange(range);
        }
    }
}
