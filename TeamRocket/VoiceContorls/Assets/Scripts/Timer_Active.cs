using UnityEngine;
using System.Collections;

public class Timer_Active : MonoBehaviour
{
    public float timeToActive;
    public GameObject ActivateObject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(TimerActive());
    }

    IEnumerator TimerActive()
    {

        yield return new WaitForSeconds(timeToActive);
        ActivateObject.SetActive(true);

    }


}
