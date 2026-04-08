using TMPro;
using UnityEngine;
using UnityEngine.UI;
//This script is to handle the main menu UI change settings transition to scenes and exit the program
public class MainMenuUIHandler : MonoBehaviour
{
    public Slider VolumeSlider;
    public TMP_Text VolumeInfo;
    //the menu game objects are the collection of a menu so they can be switched between
    public GameObject Menu1;
    public GameObject Menu2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //starts the volume slider at 50 and gets the volume info text working
        VolumeSlider.value = 50f;
        VolumeInfo.text = ("Volume: " + VolumeSlider.value);
    }
    public void VolumeUpdate()
    {
        //updates the volume and the settings holder depending on the volume slider
        VolumeInfo.text = ("Volume: " + Mathf.Round(VolumeSlider.value * 10.0f) * 0.1f);
        SettingsHolder Settings = FindAnyObjectByType<SettingsHolder>();
        Settings.Volume = VolumeSlider.value;
    }
    public void MenuChange(int State)
    {
        //Press a button and it will switch the menu group
        //Will potentially switch to switch rather than if, if many more menus get added
        if (State == 1)
        {
            Menu1.SetActive(true);
            Menu2.SetActive(false);
        }
        else if (State == 2)
        {
            Menu2.SetActive(true);
            Menu1.SetActive(false);
        }
    }
    public void LoadScene(int scene)
    {
        //to be implemented further down the line once the scenes to be transitioned to are created
    }
    public void ExitProgram()
    {
        //NOTE apparently doesnt work in unity editor mode
        Application.Quit();
    }
}
