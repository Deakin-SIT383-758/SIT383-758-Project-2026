using UnityEngine;
using UnityEngine.Events;

namespace OAS.HandTracking
{
    public class HandMenuButton : MonoBehaviour
    {
        public UnityEvent onClick = new UnityEvent();

        private Material buttonMaterial;
        private Color defaultColor;
        private int hoverCount;

        private static readonly Color HoverColor = new Color(0.4f, 0.75f, 1f);

        private void Awake()
        {
            Renderer buttonRenderer = GetComponent<Renderer>();
            if (buttonRenderer != null)
            {
                buttonMaterial = buttonRenderer.material;
                defaultColor = buttonMaterial.color;
            }
        }

        public void OnHoverEnter()
        {
            hoverCount++;
            if (buttonMaterial != null) buttonMaterial.color = HoverColor;
        }

        public void OnHoverExit()
        {
            hoverCount = Mathf.Max(0, hoverCount - 1);
            if (hoverCount == 0 && buttonMaterial != null) buttonMaterial.color = defaultColor;
        }

        public void Press() => onClick?.Invoke();
    }
}
