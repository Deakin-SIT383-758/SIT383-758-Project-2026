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

        private Transform leftIndexTip;
        private Transform rightIndexTip;
        private float dwellTimer;
        private bool isHovering;
        private bool hasFired;

        private readonly Collider[] overlapBuffer = new Collider[4];

        private void Start()
        {
            SetColor(IdleColor);
            if (leftSkeleton  != null) StartCoroutine(WaitForBones(leftSkeleton,  isLeft: true));
            if (rightSkeleton != null) StartCoroutine(WaitForBones(rightSkeleton, isLeft: false));
        }

        private IEnumerator WaitForBones(OVRSkeleton skeleton, bool isLeft)
        {
            while (!skeleton.IsInitialized) yield return null;
            foreach (var bone in skeleton.Bones)
            {
                if (bone.Id == OVRSkeleton.BoneId.Hand_Index3)
                {
                    if (isLeft) leftIndexTip  = bone.Transform;
                    else        rightIndexTip = bone.Transform;
                }
            }
        }

        private void Update()
        {
            if (hasFired) return;

            bool touching = IsTouching(leftIndexTip) || IsTouching(rightIndexTip);

            if (touching != isHovering)
            {
                isHovering = touching;
                dwellTimer = 0f;
                SetColor(touching ? HoverColor : IdleColor);
                if (statusLabel != null)
                    statusLabel.text = touching ? "Hold to rescan..." : "Rescan Room";
            }

            if (isHovering)
            {
                dwellTimer += Time.deltaTime;

                if (statusLabel != null)
                {
                    int dots = Mathf.FloorToInt((dwellTimer / dwellSeconds) * 3f) + 1;
                    statusLabel.text = "Hold" + new string('.', Mathf.Clamp(dots, 1, 3));
                }

                if (dwellTimer >= dwellSeconds)
                    Press();
            }
        }

        private bool IsTouching(Transform probe)
        {
            if (probe == null) return false;
            int hitCount = Physics.OverlapSphereNonAlloc(probe.position, touchRadius, overlapBuffer);
            for (int i = 0; i < hitCount; i++)
                if (overlapBuffer[i].gameObject == gameObject) return true;
            return false;
        }

        private void Press()
        {
            hasFired = true;
            SetColor(PressedColor);
            if (statusLabel != null) statusLabel.text = "Opening\nSpace Setup...";

            var requestObject = new GameObject("SceneCaptureRequest");
            var sceneManager  = requestObject.AddComponent<OVRSceneManager>();
            sceneManager.RequestSceneCapture();
            Destroy(requestObject, 3f);

            StartCoroutine(ResetAfterDelay(4f));
        }

        private IEnumerator ResetAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            hasFired   = false;
            dwellTimer = 0f;
            isHovering = false;
            SetColor(IdleColor);
            if (statusLabel != null) statusLabel.text = "Rescan Room";
        }

        private void SetColor(Color color)
        {
            if (buttonRenderer != null)
                buttonRenderer.material.color = color;
        }
    }
}
