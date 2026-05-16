using System;
using UnityEngine;

public class PracticeTime : MonoBehaviour
{
    [Serializable]
    public class TimeData
    {
        public int id;
        public string name;
        public int hours;
        public int minutes;
    }
    public TimeData timeData;
}
