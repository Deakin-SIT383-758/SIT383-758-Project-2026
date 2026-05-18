using System.Collections;
using UnityEngine;

namespace OAS.HandTracking
{
    public class MRPassthroughToggle : MonoBehaviour
    {
        [SerializeField] private OVRPassthroughLayer passthroughLayer;
        [SerializeField] private GameObject          virtualFloor;
        [SerializeField] private float               fadeDuration = 0.4f;

        private bool      _active;
        private Coroutine _fade;

        private void Awake()
        {
            if (passthroughLayer == null)
                passthroughLayer = GetComponent<OVRPassthroughLayer>();

            if (passthroughLayer != null)
                passthroughLayer.textureOpacity = 0f;
        }

        public void Toggle()
        {
            _active = !_active;

            if (virtualFloor != null)
            {
                var rend = virtualFloor.GetComponent<MeshRenderer>();
                if (rend != null)
                    rend.enabled = !_active;
                else
                    virtualFloor.SetActive(!_active);
            }

            if (_fade != null) StopCoroutine(_fade);
            _fade = StartCoroutine(Fade(_active ? 1f : 0f));
        }

        private IEnumerator Fade(float target)
        {
            if (passthroughLayer == null) yield break;

            float start   = passthroughLayer.textureOpacity;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                passthroughLayer.textureOpacity = Mathf.Lerp(start, target, elapsed / fadeDuration);
                yield return null;
            }

            passthroughLayer.textureOpacity = target;
        }
    }
}
