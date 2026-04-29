using UnityEngine;
using UnityEngine.Events;

namespace OAS.HandTracking
{
    public class HandMenuButton : MonoBehaviour
    {
        public UnityEvent onClick = new UnityEvent();

        private Material _mat;
        private Color    _defaultColor;
        private int      _hoverCount;

        private static readonly Color HoverColor = new Color(0.4f, 0.75f, 1f);

        private void Awake()
        {
            var rend = GetComponent<Renderer>();
            if (rend != null)
            {
                _mat          = rend.material;
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
            if (_hoverCount == 0 && _mat != null) _mat.color = _defaultColor;
        }

        public void Press() => onClick?.Invoke();
    }
}
