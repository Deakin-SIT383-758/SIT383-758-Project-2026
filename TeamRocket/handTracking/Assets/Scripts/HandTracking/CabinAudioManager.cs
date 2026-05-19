using UnityEngine;

namespace OAS.HandTracking
{
    public class CabinAudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource   ambientSource;
        [SerializeField] private AudioSource   alarmSource;
        [SerializeField] private AudioSource[] passengerSources;

        [SerializeField] private float minDistance  = 0.5f;  
        [SerializeField] private float maxDistance  = 10f;    
        [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

        [SerializeField] private Transform alarmAnchor;

        private void Start()
        {
            InitializeSpatialAudio();

            if (alarmAnchor == null)
            {
                foreach (var hs in FindObjectsByType<TabletopHotspot>(FindObjectsSortMode.None))
                {
                    if (hs.Type == HotspotType.EmergencyExit)
                    {
                        alarmAnchor = hs.transform;
                        break;
                    }
                }
            }

            if (ambientSource != null && ambientSource.clip != null)
                ambientSource.Play();
        }

        private void InitializeSpatialAudio()
        {
            // Alarm and passengers are point sources in 3D space
            Configure3D(alarmSource);
            foreach (var src in passengerSources)
                Configure3D(src);

            // Ambient is non-directional — keep it 2D
            if (ambientSource != null)
            {
                ambientSource.spatialBlend = 0f;
                ambientSource.rolloffMode  = AudioRolloffMode.Linear;
            }
        }

        private void Configure3D(AudioSource src)
        {
            if (src == null) return;
            src.spatialBlend = 1f;           // full 3D directional audio
            src.rolloffMode  = rolloffMode;
            src.minDistance  = minDistance;
            src.maxDistance  = maxDistance;
            src.dopplerLevel = 0f;           // no doppler for stationary cabin sources
            src.spread       = 0f;           // point source — tightest directional localisation
        }

        private void Update()
        {
            // Keep alarm source locked to emergency exit position every frame
            if (alarmAnchor != null && alarmSource != null)
                alarmSource.transform.position = alarmAnchor.position;
        }

        public void TriggerAlarm()
        {
            if (alarmSource != null && alarmSource.clip != null && !alarmSource.isPlaying)
                alarmSource.Play();
        }

        public void StopAlarm() => alarmSource?.Stop();

        public void PlayAllPassengers()
        {
            foreach (var src in passengerSources)
                if (src != null && src.clip != null && !src.isPlaying)
                    src.Play();
        }

        public void StopAllPassengers()
        {
            foreach (var src in passengerSources)
                src?.Stop();
        }

        public void PlayPassengerVoice(int index)
        {
            if ((uint)index >= (uint)passengerSources.Length) return;
            var src = passengerSources[index];
            if (src != null && src.clip != null && !src.isPlaying)
                src.Play();
        }

        public void StopAll()
        {
            StopAlarm();
            StopAllPassengers();
            ambientSource?.Stop();
        }

        // Called by AudioRangeSlider — updates hearing range on all spatial sources
        public void SetMaxHearingRange(float metres)
        {
            maxDistance = metres;
            if (alarmSource != null)
            {
                alarmSource.maxDistance = metres;
                alarmSource.minDistance = Mathf.Min(minDistance, metres * 0.1f);
            }
            foreach (var src in passengerSources)
            {
                if (src == null) continue;
                src.maxDistance = metres;
                src.minDistance = Mathf.Min(minDistance, metres * 0.1f);
            }
        }
    }
}
