using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public float startTime;
    public PracticeTime timeSave;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startTime = Time.time;
        timeSave.timeData.minutes = PlayerPrefs.GetInt("Minutes", 0);
        timeSave.timeData.hours = PlayerPrefs.GetInt("Hours", 0);
    }

    // Update is called once per frame
    void Update()
    {
        float elapsedTime = Time.time - startTime;
        if (elapsedTime > 60)
        {
            timeSave.timeData.minutes = timeSave.timeData.minutes + 1;
            startTime = Time.time;
            if(timeSave.timeData.minutes == 60)
            {
                timeSave.timeData.minutes = 0;
                timeSave.timeData.hours = timeSave.timeData.hours + 1;
            }
            PlayerPrefs.SetInt("Minutes", timeSave.timeData.minutes);
            PlayerPrefs.SetInt("Hours", timeSave.timeData.hours);
            PlayerPrefs.Save();
        }

    }
}
