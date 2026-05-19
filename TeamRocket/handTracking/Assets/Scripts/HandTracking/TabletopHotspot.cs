using System.Collections;
using UnityEngine;

namespace OAS.HandTracking
{
    [RequireComponent(typeof(Collider))]
    public class TabletopHotspot : MonoBehaviour
    {
        [SerializeField] private HotspotType hotspotType;
        [SerializeField] private float feedbackDuration = 1.5f;

        private Material hotspotMaterial;
        private Color defaultColor;
        private int hoverCount;

        private static readonly Color HoverColor    = new Color(1f, 0.85f, 0f);
        private static readonly Color CorrectColor   = Color.green;
        private static readonly Color IncorrectColor = Color.red;

        public HotspotType Type => hotspotType;

        private void Awake()
        {
            Renderer hotspotRenderer = GetComponent<Renderer>();
            if (hotspotRenderer != null)
            {
                hotspotMaterial = hotspotRenderer.material;
                defaultColor = hotspotMaterial.color;
            }
        }

        public void OnHoverEnter()
        {
            hoverCount++;
            if (hotspotMaterial != null) hotspotMaterial.color = HoverColor;
        }

        public void OnHoverExit()
        {
            hoverCount = Mathf.Max(0, hoverCount - 1);
            if (hoverCount == 0 && hotspotMaterial != null)
                hotspotMaterial.color = defaultColor;
        }

        public void OnSelected(bool isCorrect)
        {
            StopAllCoroutines();
            StartCoroutine(ShowFeedback(isCorrect ? CorrectColor : IncorrectColor));
        }

        private IEnumerator ShowFeedback(Color feedbackColor)
        {
            if (hotspotMaterial != null) hotspotMaterial.color = feedbackColor;
            yield return new WaitForSeconds(feedbackDuration);
            if (hotspotMaterial != null) hotspotMaterial.color = hoverCount > 0 ? HoverColor : defaultColor;
        }
    }
}
