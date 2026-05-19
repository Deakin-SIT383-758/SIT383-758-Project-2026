using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Volume : MonoBehaviour
{
    public float volume { get; private set; }
    private AudioClip _clipRecord;
    private const int SampleWindow = 128; // Small window for analysis [3]

    public Text VolumeText;

    void Start()
    {
        _clipRecord = Microphone.Start(null, true, 1, 44100);
    }

    void Update()
    {
        volume = GetMicVolume();
        VolumeText.text = "Volume: " + volume * 1000;
        Static_Data.volume += volume * 1000 + " ";
        print(Static_Data.volume);
    }

    public float GetMicVolume()
    {
        float[] waveData = new float[SampleWindow];
        int micPosition = Microphone.GetPosition(null) - (SampleWindow + 1);
        if (micPosition < 0) return 0;

        _clipRecord.GetData(waveData, micPosition);

        float maxVolume = 0;
        for (int i = 0; i < SampleWindow; i++)
        {
            maxVolume = Mathf.Max(maxVolume, Mathf.Abs(waveData[i])); // Peak calculation [3]
        }
        return maxVolume;
    }
}
