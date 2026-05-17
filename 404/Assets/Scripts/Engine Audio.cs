using UnityEngine;
using System.Collections;

public class AudioCrossfade : MonoBehaviour
{
    public AudioSource startSource;
    public AudioSource loopSource;

    public AudioClip startClip;
    public AudioClip loopClip;

    public float fadeDuration = 1f;

    public void Play()
    {
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        // Play the starting sound
        startSource.clip = startClip;
        startSource.volume = 1f;
        startSource.loop = false;
        startSource.Play();

        // Wait until near the end (optional tweak)
        yield return new WaitForSeconds(startClip.length - fadeDuration);

        // Start loop sound quietly
        loopSource.clip = loopClip;
        loopSource.volume = 0f;
        loopSource.loop = true;
        loopSource.Play();

        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            startSource.volume = 1 - t;
            loopSource.volume = t;

            yield return null;
        }

        startSource.Stop();
        loopSource.volume = 1f;
    }
}