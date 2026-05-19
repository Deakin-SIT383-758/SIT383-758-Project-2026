using UnityEngine;
using Oculus.Haptics;

public class HapticShake : MonoBehaviour
{
    public HapticClip hapticClip;
    public AudioSource audio;
    private HapticClipPlayer player;

    public float shakeIntensity = 0.5f;
    public float shakeSpeed = 20f;
    public GameObject VRCamera;

    float timer = 0.0f;
    private Vector3 originalPos;

    void Start()
    {
        player = new HapticClipPlayer(hapticClip);
        player.Play(Controller.Left);
        player.Play(Controller.Right);
        audio.Play();
        originalPos = VRCamera.transform.localPosition;
    }

    void Update()
    {
        float[] samples = new float[64];
        audio.GetOutputData(samples, 0);

        // Calculate average amplitude
        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
            sum += Mathf.Abs(samples[i]);
        float amplitude = sum / samples.Length;

        // Apply shake based on amplitude
        if (amplitude > 0.01f)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * (amplitude * shakeIntensity);
            VRCamera.transform.localPosition = originalPos + shakeOffset;
        }
        else
        {
            VRCamera.transform.localPosition = Vector3.Lerp(transform.localPosition, originalPos, Time.deltaTime * shakeSpeed);
        }

        //Hatic Loop
        timer -= Time.deltaTime;
        if (timer < 0.0f)
        {
            player.Play(Controller.Left);
            player.Play(Controller.Right);
            timer = player.clipDuration;
        }
    }

}
