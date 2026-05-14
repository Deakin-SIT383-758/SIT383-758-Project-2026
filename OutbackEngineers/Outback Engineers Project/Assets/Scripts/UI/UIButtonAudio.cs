using UnityEngine;

public class UIButtonAudio : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clickSound;

    public void PlayClick()
    {
        Debug.Log("PLAY CLICK CALLED");

        if (audioSource != null)
        {
            Debug.Log("AudioSource OK");
        }

        if (clickSound != null)
        {
            Debug.Log("Clip OK");
        }

        audioSource.PlayOneShot(clickSound);
    }
}