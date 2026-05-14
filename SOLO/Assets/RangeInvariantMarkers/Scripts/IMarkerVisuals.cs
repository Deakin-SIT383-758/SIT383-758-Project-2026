
using UnityEngine;

namespace MM.RangeInvariantMarkers
{

    public interface IMarkerVisuals
    {
        public void SetTimers(VisualTimers timers);
        public void SetMessage(string message);
        public GameObject GetPrefabGO();
        public void UpdateVisuals(Vector3 observerPosition);
        public void SetVisualsEnabled(bool enabled);
        public void ProcessGaze();

        [System.Serializable]
        public struct VisualTimers
        {
            [Tooltip("How long the gaze is on the item before fade out commences")]
            public float fadeOutTimeThreshold;
            [Tooltip("How long after gaze leaves the item before fade in commences")]
            public float fadeInTimeThreshold;

            [Tooltip("How long the fade out takes")]
            public float fadeOutDuration;
            [Tooltip("How long the fade in takes")]
            public float fadeInDuration;

            [Tooltip("How the gaze has to not be on the object to consider as not gazing (Prevents gaze loss on noise/jitter)")]
            public float gazeLossTimeout;


            public VisualTimers(float fadeOutTimeThreshold = 0.2f, 
                float fadeInTimeThreshold = 1.0f, 
                float fadeOutDuration = 0.5f, 
                float fadeInDuration = 1.0f, 
                float gazeLossTimeout = 0.2f) 
            {
                this.fadeOutTimeThreshold = fadeOutTimeThreshold;
                this.fadeInTimeThreshold = fadeInTimeThreshold;
                this.fadeOutDuration = fadeOutDuration;
                this.fadeInDuration = fadeInDuration;
                this.gazeLossTimeout = gazeLossTimeout;
            }
        }

    }
}