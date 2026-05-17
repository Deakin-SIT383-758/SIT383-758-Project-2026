using TMPro;
using UnityEngine;

public class KeyboardInput : MonoBehaviour
{
    public TMP_InputField inputField;

    public void AddLetter(string letter)
    {
        inputField.text += letter;
    }

    public void Backspace()
    {
        if (inputField.text.Length > 0)
        {
            inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
        }
    }
}
