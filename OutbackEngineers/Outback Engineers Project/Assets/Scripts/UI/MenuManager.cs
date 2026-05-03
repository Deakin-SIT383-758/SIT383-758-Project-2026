using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class MenuManager : MonoBehaviour
{
    InputSystem_Actions actions;
    //UI elements accessing data from
    public TMP_Dropdown selector;
    public GameObject menu;

    //Variables passed to other scripts
    public bool SceneChanged;
    public string ChosenScene;

    Scene scene;
    bool MenuActive;

    void Awake()
    {
        actions = new InputSystem_Actions();
        actions.Enable();

        selector.onValueChanged.AddListener(delegate{OnDropdownValueChange(selector.value);});
    }

    public void OnDropdownValueChange(int index)
    {
        int selectionIndex = selector.value;

        switch (selectionIndex)
        {
            case 0:
                ChosenScene = "Runway_A";
                break;
            case 1:
                ChosenScene = "Runway_B";
                break;
            case 2:
                ChosenScene = "Runway_C";
                break;
            case 3:
                ChosenScene = "Runway_D";
                break;
            case 4:
                ChosenScene = "Runway_E";
                break;
            case 5:
                ChosenScene = "Runway_F";
                break;
            case 6:
                ChosenScene = "Runway_G";
                break;
            default:
                ChosenScene = "Runway_A";
                break;
        }
    }

    void Update()
    {
        scene = SceneManager.GetActiveScene();
        if (scene.name == "MainScene" && SceneChanged == true)
        {
            menu.SetActive(false);
            MenuActive = false;
        }
        if(scene.name == "MainScene" && MenuActive == false && actions.Player.Previous.ReadValue<float>() > 0.5f)
        {
            menu.SetActive(true);
            MenuActive = true;
        }
    }

    //Called when the View button is pressed
    public void GoToMain()
    {
        SceneManager.LoadScene(1);
        SceneChanged = true;
    }

    //Called when the Menu Button is pressed
    public void GoToMenu()
    {
        SceneManager.LoadScene(0);
        SceneChanged = true;
    }

    public void CloseProgram()
    {
        Application.Quit();
    }
}
