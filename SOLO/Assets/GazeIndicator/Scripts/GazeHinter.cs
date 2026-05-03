using UnityEngine;

public class GazeHinter : MonoBehaviour
{
    public GameObject gazeIndicator;
    [SerializeField] private OVRInput.Button gazeToggleButton = OVRInput.Button.SecondaryIndexTrigger;
    void Update()
    {
        gazeIndicator.SetActive(OVRInput.Get(gazeToggleButton));
    }
}
