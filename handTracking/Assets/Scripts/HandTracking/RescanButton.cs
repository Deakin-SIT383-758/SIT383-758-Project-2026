using System.Collections;
using TMPro;
using UnityEngine;

namespace OAS.HandTracking
{
    [RequireComponent(typeof(Collider))]
    public class RescanButton : MonoBehaviour
    {
        [SerializeField] private OVRSkeleton leftSkeleton;
        [SerializeField] private OVRSkeleton rightSkeleton;
        [SerializeField] private Renderer    buttonRenderer;
        [SerializeField] private TMP_Text    statusLabel;
        [SerializeField] private float       touchRadius  = 0.035f;
        [SerializeField] private float       dwellSeconds = 1.0f;

        private static readonly Color IdleColor    = new Color(0.18f, 0.38f, 0.75f);
        private static readonly Color HoverColor   = new Color(0.25f, 0.65f, 1.00f);
        private static readonly Color PressedColor = Color.white;

        private Transform _lIndex3, _rIndex3;
        private float     _dwell;
        private bool      _hovering;
        private bool      _fired;

        private readonly Collider[] _hits = new Collider[4];

        private void Start()
        {
            SetColor(IdleColor);
            if (leftSkeleton  != null) StartCoroutine(WaitBones(leftSkeleton,  true));
            if (rightSkeleton != null) StartCoroutine(WaitBones(rightSkeleton, false));
        }

        private IEnumerator WaitBones(OVRSkeleton sk, bool isLeft)
        {
            while (!sk.IsInitialized) yield return null;
            foreach (var b in sk.Bones)
                if (b.Id == OVRSkeleton.BoneId.Hand_Index3)
                {
                    if (isLeft) _lIndex3 = b.Transform;
                    else        _rIndex3 = b.Transform;
                }
        }

        private void Update()
        {
            if (_fired) return;

            bool touching = IsTouching(_lIndex3) || IsTouching(_rIndex3);

            if (touching != _hovering)
            {
                _hovering = touching;
                _dwell    = 0f;
                SetColor(touching ? HoverColor : IdleColor);
                if (statusLabel != null)
                    statusLabel.text = touching ? "Hold to rescan..." : "Rescan Room";
            }

            if (_hovering)
            {
                _dwell += Time.deltaTime;

                // Fill the label with dots as progress indicator
                if (statusLabel != null)
                {
                    int dots = Mathf.FloorToInt((_dwell / dwellSeconds) * 3f) + 1;
                    statusLabel.text = "Hold" + new string('.', Mathf.Clamp(dots, 1, 3));
                }

                if (_dwell >= dwellSeconds)
                    Press();
            }
        }

        private bool IsTouching(Transform probe)
        {
            if (probe == null) return false;
            int n = Physics.OverlapSphereNonAlloc(probe.position, touchRadius, _hits);
            for (int i = 0; i < n; i++)
                if (_hits[i].gameObject == gameObject) return true;
            return false;
        }

        private void Press()
        {
            _fired = true;
            SetColor(PressedColor);
            if (statusLabel != null) statusLabel.text = "Opening\nSpace Setup...";

            // Launch Meta's Space Setup overlay from within the app
            var go = new GameObject("_SceneCaptureRequest");
            var sm = go.AddComponent<OVRSceneManager>();
            sm.RequestSceneCapture();
            Destroy(go, 3f);

            StartCoroutine(ResetAfterDelay(4f));
        }

        private IEnumerator ResetAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            _fired    = false;
            _dwell    = 0f;
            _hovering = false;
            SetColor(IdleColor);
            if (statusLabel != null) statusLabel.text = "Rescan Room";
        }

        private void SetColor(Color c)
        {
            if (buttonRenderer != null)
                buttonRenderer.material.color = c;
        }
    }
}
