using System.Collections;
using UnityEngine;

namespace OAS.HandTracking
{
    public class MRPassthroughToggle : MonoBehaviour
    {
        [SerializeField] private OVRPassthroughLayer passthroughLayer;
        [SerializeField] private GameObject virtualFloor;
        [SerializeField] private float fadeDuration = 0.4f;

        private bool isActive;
        private Coroutine fadeCoroutine;

        private void Awake()
        {
            if (passthroughLayer == null)
                passthroughLayer = GetComponent<OVRPassthroughLayer>();

            if (passthroughLayer != null)
                passthroughLayer.textureOpacity = 0f;
        }

        public void Toggle()
        {
            isActive = !isActive;

            if (virtualFloor != null)
            {
                MeshRenderer floorRenderer = virtualFloor.GetComponent<MeshRenderer>();
                if (floorRenderer != null)
                    floorRenderer.enabled = !isActive;
                else
                    virtualFloor.SetActive(!isActive);
            }

            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            float targetOpacity = isActive ? 1f : 0f;
            fadeCoroutine = StartCoroutine(FadePassthrough(targetOpacity));
        }

        private IEnumerator FadePassthrough(float targetOpacity)
        {
            if (passthroughLayer == null) yield break;

            float startOpacity = passthroughLayer.textureOpacity;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                passthroughLayer.textureOpacity = Mathf.Lerp(startOpacity, targetOpacity, elapsed / fadeDuration);
                yield return null;
            }

            passthroughLayer.textureOpacity = targetOpacity;
        }
    }
}
