using UnityEngine;

public class VRMenuToggle : MonoBehaviour
{
    public GameObject menuRoot;

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            menuRoot.SetActive(!menuRoot.activeSelf);
        }
    }
}