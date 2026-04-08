using UnityEngine;
//This script is to hold onto values that you want to remain consistent across scenes
public class SettingsHolder : MonoBehaviour
{
    //Volume variable to potentially be held and used in the system 
    public float Volume = 50f;
    // Start is called once before the first execution of Update after the MonoBehaviour is create
    void Start()
    {
        //preserve the settings holder through scene changes
        DontDestroyOnLoad(this.gameObject);
    }
}
