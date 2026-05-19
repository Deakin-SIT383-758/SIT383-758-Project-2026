using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CallOldText : MonoBehaviour
{
    public Text outputText;
    public Text VolumeText;
    public void Show_OldText()
    {
        outputText.text = Static_Data.WORDS_SPOKE;
        VolumeText.text = "Volume: " + Static_Data.volume;
    }
    public void ClearText()
    {
        outputText.text = "Press button to start recording...";
    }
}
