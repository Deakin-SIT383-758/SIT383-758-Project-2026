using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public void LoadMenu()
    {
        StartCoroutine(LoadMenuCoroutine());
    }

    IEnumerator LoadMenuCoroutine()
    {
        yield return new WaitForSeconds(0.15f);

        SceneManager.LoadScene("MenuScene");
    }
}