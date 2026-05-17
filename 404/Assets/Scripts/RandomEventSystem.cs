using UnityEngine;
using System.Collections;

public class RandomEventSystem : MonoBehaviour
{
    [Header("Turbulence Timing")]
    public float minDelay = 5f;
    public float maxDelay = 15f;

    [Header("Camera Shake")]
    public Transform playerCamera;       // Assign your camera here
    public int minBursts = 1;
    public int maxBursts = 3;
    public float minStrength = 0.02f;
    public float maxStrength = 0.08f;
    public float minDuration = 0.4f;
    public float maxDuration = 1f;

    [Header("Audio Settings")]
    public AudioSource audioSource;      // Assign AudioSource here
    public AudioClip turbulenceClip;
    public float fadeOutDuration = 1.5f;

    void Start()
    {
        StartCoroutine(RandomEventLoop());
    }

    // Main loop to trigger turbulence at random intervals
    IEnumerator RandomEventLoop()
    {
        while (true)
        {
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            TriggerTurbulence();
        }
    }

    void TriggerTurbulence()
    {
        Debug.Log("TURBULENCE!");

        // Start audio
        if (audioSource != null && turbulenceClip != null)
        {
            audioSource.clip = turbulenceClip;
            audioSource.loop = true;
            audioSource.volume = 1f;
            audioSource.Play();
        }

        // Start camera shake burst
        StartCoroutine(TurbulenceBurst());
    }

    // Handles bursts of turbulence
    IEnumerator TurbulenceBurst()
    {
        int bursts = Random.Range(minBursts, maxBursts + 1);

        for (int i = 0; i < bursts; i++)
        {
            float strength = Random.Range(minStrength, maxStrength);
            float duration = Random.Range(minDuration, maxDuration);

            StartCoroutine(TurbulenceShake(duration, strength));

            yield return new WaitForSeconds(Random.Range(0.3f, 1f));
        }

        // Fade out audio smoothly when turbulence ends
        if (audioSource != null)
        {
            StartCoroutine(FadeOutAudio(fadeOutDuration));
        }
    }

    // Camera shake coroutine
    IEnumerator TurbulenceShake(float duration, float strength)
    {
        float time = 0f;
        Vector3 originalPos = playerCamera.localPosition;

        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;
            float fade = Mathf.Sin(progress * Mathf.PI); // smooth in/out

            Vector3 randomOffset = Random.insideUnitSphere * strength * fade;
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, originalPos + randomOffset, Time.deltaTime * 5f);

            yield return null;
        }

        playerCamera.localPosition = originalPos;
    }

    // Smooth audio fade-out
    IEnumerator FadeOutAudio(float duration)
    {
        float startVolume = audioSource.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            t = t * t; // ease-out for smoother fade
            audioSource.volume = Mathf.Lerp(startVolume, 0, t);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume; // reset for next turbulence
    }
}