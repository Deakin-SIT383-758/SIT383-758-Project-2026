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
        if (scene.name == "MainScene" && SceneChanged == true) // Moves menu to the side of the player when in main scene for a less obstructed view of the runway.
        {
            Transform cam = Camera.main.transform;

            Vector3 sideOffset = cam.right * 2f;
            Vector3 forwardOffset = cam.forward * 1.5f;

            menu.transform.position =
                cam.position + sideOffset + forwardOffset;

            menu.transform.LookAt(cam);

            menu.transform.Rotate(0, 180, 0);

            MenuActive = true;

            SceneChanged = false;

            Debug.Log("Menu position: " + menu.transform.position);
        }
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

    public void CloseProgram()
    {
        Application.Quit();
    }
}
