using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
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
        selector.onValueChanged.AddListener(delegate{OnDropdownValueChange(selector.value);});
    }

    public void OnDropdownValueChange(int index)
    {
        int selectionIndex = selector.value;

        switch (selectionIndex)
        {
            case 0:
                ChosenScene = "City_Runway";
                break;

            case 1:
                ChosenScene = "DryLand_Runway";
                break;

            case 2:
                ChosenScene = "Grass_Runway";
                break;

            case 3:
                ChosenScene = "Marsh_Runway";
                break;

            case 4:
                ChosenScene = "RedSand_Runway";
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
        //if(scene.name == "MainScene" && MenuActive == false && OVRInput.GetDown(OVRInput.RawButton.A))  TEMP BLOCK OUT FOR TESTING PURPOSES WITHOUT HEADSETS
        //{
        //    menu.SetActive(true);
        //    MenuActive = true;
        //}
    }

    void Start()
    {
        selector.value = 0;

        OnDropdownValueChange(0);

        Debug.Log("Default runway set to: " + ChosenScene);
    }

    public void GoToMain()
    {
        Debug.Log("VIEW RUNWAY BUTTON PRESSED");

        Debug.Log("Chosen runway BEFORE save: " + ChosenScene);

        PersistanceScript.Instance.selectedRunway = ChosenScene;

        Debug.Log("Saved runway: " + PersistanceScript.Instance.selectedRunway);

        SceneManager.LoadScene("MainScene");

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
