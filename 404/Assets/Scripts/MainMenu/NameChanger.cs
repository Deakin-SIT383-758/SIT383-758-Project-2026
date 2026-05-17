using TMPro;
using UnityEngine;

public class NameChanger : MonoBehaviour
{
    public TMP_InputField inputField;
    public void ChangeName()
    {
        PlayerPrefs.SetString("Name",inputField.text);
        inputField.text = ("Saved");
    }
}
