using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRButtonSimpleAudio : MonoBehaviour
{
    [Header("References")]
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    [Header("Audio")]
    public AudioSource firstAudioSource;
    public AudioSource secondAudioSource;

    private bool isPlayingChain = false;

    private void Start()
    {
        if (interactable != null)
            interactable.activated.AddListener(OnPressed);

        // Ensure second audio doesn't play early
        if (secondAudioSource != null)
            secondAudioSource.Stop();
    }

    private void OnDestroy()
    {
        if (interactable != null)
            interactable.activated.RemoveListener(OnPressed);
    }

    private void OnPressed(ActivateEventArgs args)
    {
        Debug.Log("Button Pressed");

        if (isPlayingChain) return;

        StartAudioChain();
    }

    private void StartAudioChain()
    {
        isPlayingChain = true;

        // Play first audio
        if (firstAudioSource != null)
        {
            firstAudioSource.Play();

            // Start waiting for it to finish
            Invoke(nameof(PlaySecondAudio), firstAudioSource.clip.length);
        }
        else
        {
            PlaySecondAudio();
        }
    }

    private void PlaySecondAudio()
    {
        if (secondAudioSource != null)
        {
            secondAudioSource.Play();
        }

        isPlayingChain = false;
    }
}