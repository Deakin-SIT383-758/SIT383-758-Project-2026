using UnityEngine;
using UnityEngine.InputSystem;

namespace MM.RangeInvariantMarkers
{

    public class MarkerPrefab : MonoBehaviour, IMarkerVisuals
    {
        public TMPro.TextMeshProUGUI markerInfoText;
        public GameObject rootUI;
        private bool enableMarkerVisuals = true;
        public void OnEnable()
        {
            this.GazeLingerTimeEvent += MarkerPrefab_GazeLingerTimeEvent;
            this.GazeEndEvent += MarkerPrefab_GazeEndEvent;
            rootUI.SetActive(false);
        }

        private void MarkerPrefab_GazeEndEvent()
        {
            rootUI.SetActive(false);
        }

        private void MarkerPrefab_GazeLingerTimeEvent(float gazetime)
        {
            rootUI.SetActive(gazetime > this.fadeOutDuration);
        }

        public void OnDisable()
        {
            this.GazeLingerTimeEvent -= MarkerPrefab_GazeLingerTimeEvent;
            this.GazeEndEvent -= MarkerPrefab_GazeEndEvent;
        }

        GameObject IMarkerVisuals.GetPrefabGO()
        {
            return this.gameObject;
        }

        void IMarkerVisuals.SetMessage(string message)
        {
            markerInfoText.text = message;
        }

        void IMarkerVisuals.UpdateVisuals(Vector3 observerPosition)
        {
            rootUI.transform.LookAt(observerPosition);
            UpdateGazeFade();
        }

        void IMarkerVisuals.SetVisualsEnabled(bool enabled)
        {
            if (enableMarkerVisuals == enabled) return;
            enableMarkerVisuals = enabled;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(enabled);
            }
        }

        public void LateUpdate()
        {
            UpdateGazeFade();
        }


        #region GazeManagement
        public event System.Action GazeStartEvent;
        public event System.Action<float> GazeLingerTimeEvent;
        public event System.Action GazeEndEvent;

        private float fadeOutTimeThreshold = 0.2f;
        private float fadeInTimeThreshold = 1.0f;
        
        private float fadeOutDuration = 0.5f;
        private float fadeInDuration = 1.0f;

        private float gazeLossTimeout = 0.2f;

        //Gaze detection tracking
        private float lastGazeTimestamp = 0f;
        private bool currentlyLookedAt = false;
        private float lookedAtTimer = 0f, lookAwayTimer = 0f;
        void IMarkerVisuals.ProcessGaze()
        {
            lastGazeTimestamp = Time.time;
            if (currentlyLookedAt == false)
            {
                GazeStartEvent?.Invoke();
            }
            currentlyLookedAt = true;
        }

        private void UpdateGazeFade()
        {
            if (enableMarkerVisuals == false) return;

            if (Time.time - lastGazeTimestamp > gazeLossTimeout)
            {
                GazeEndEvent?.Invoke();
                currentlyLookedAt = false;
            }

            if (currentlyLookedAt)
            {
                lookAwayTimer = 0;
                lookedAtTimer += Time.deltaTime;
                GazeLingerTimeEvent?.Invoke(lookedAtTimer);
            } else
            {
                lookedAtTimer = 0;
                lookAwayTimer += Time.deltaTime;
            }

            if (lookedAtTimer > fadeOutTimeThreshold)
            {
                UpdateVisualsAlpha(0, fadeOutDuration);
            }
            else if (lookAwayTimer > fadeInTimeThreshold)
            {
                UpdateVisualsAlpha(1, fadeInDuration);
            }

            UpdateVisuals();
        }


        private float visualsAlpha = 1.0f;

        private void UpdateVisualsAlpha(float targetValue, float duration)
        {
            if (duration < 0)
            {
                visualsAlpha = targetValue;
                return;
            }

            float step = Time.deltaTime / duration;
            visualsAlpha = Mathf.MoveTowards(visualsAlpha, targetValue, step);
        }

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private void UpdateVisuals()
        {
            var _instancedMaterial = gameObject.GetComponentInChildren<Renderer>().material;
            Color color = _instancedMaterial.GetColor(BaseColorId);
            color.a = visualsAlpha;
            _instancedMaterial.SetColor(BaseColorId, color);
        }

        void IMarkerVisuals.SetTimers(IMarkerVisuals.VisualTimers timer)
        {
            this.fadeOutTimeThreshold = timer.fadeOutTimeThreshold;
            this.fadeOutDuration = timer.fadeOutDuration;
            this.fadeInTimeThreshold = timer.fadeInTimeThreshold;
            this.fadeInDuration = timer.fadeInDuration;
            this.gazeLossTimeout = timer.gazeLossTimeout;
        }

        #endregion
    }
}