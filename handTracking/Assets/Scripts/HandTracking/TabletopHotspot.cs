using System.Collections;
using UnityEngine;

namespace OAS.HandTracking
{
    [RequireComponent(typeof(Collider))]
    public class TabletopHotspot : MonoBehaviour
    {
        [SerializeField] private HotspotType hotspotType;
        [SerializeField] private float feedbackDuration = 1.5f;

        private Material _mat;
        private Color _defaultColor;
        private int _hoverCount;

        private static readonly Color HoverColor    = new Color(1f, 0.85f, 0f);
        private static readonly Color CorrectColor   = Color.green;
        private static readonly Color IncorrectColor = Color.red;

        public HotspotType Type => hotspotType;

        private void Awake()
        {
            var rend = GetComponent<Renderer>();
            if (rend != null)
            {
                _mat = rend.material; // creates a per-instance copy
                _defaultColor = _mat.color;
            }
        }

        public void OnHoverEnter()
        {
            _hoverCount++;
            if (_mat != null) _mat.color = HoverColor;
        }

        public void OnHoverExit()
        {
            _hoverCount = Mathf.Max(0, _hoverCount - 1);
            if (_hoverCount == 0 && _mat != null)
                _mat.color = _defaultColor;
        }

        public void OnSelected(bool isCorrect)
        {
            StopAllCoroutines();
            StartCoroutine(ShowFeedback(isCorrect ? CorrectColor : IncorrectColor));
        }

        private IEnumerator ShowFeedback(Color feedbackColor)
        {
            if (_mat != null) _mat.color = feedbackColor;
            yield return new WaitForSeconds(feedbackDuration);
            if (_mat != null) _mat.color = _hoverCount > 0 ? HoverColor : _defaultColor;
        }
    }
}
