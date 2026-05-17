using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//This script is to handle the main menu UI change settings transition to scenes and exit the program
public class MainMenuUIHandler : MonoBehaviour
{
    public TMP_InputField roomNameInput;
    //the menu game objects are the collection of a menu so they can be switched between
    public GameObject Menu1;
    public GameObject Menu2;
    public NetworkRunner runner;
    public bool shutdown = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
            shutdown = true;
            Menu2.SetActive(true);
            Menu1.SetActive(false);
        }
    }
    public void LoadScene(int scene)
    {
        SceneManager.LoadScene(scene);
    }
    public void Multiplayer()
    {
        string roomName = roomNameInput.text;
        PlayerPrefs.SetString("RoomName", roomName);
        PlayerPrefs.Save();
        SceneManager.LoadScene("MultiplayerScene");
    }
    public void ExitProgram()
    {
        //NOTE apparently doesnt work in unity editor mode
        Application.Quit();
    }
}
