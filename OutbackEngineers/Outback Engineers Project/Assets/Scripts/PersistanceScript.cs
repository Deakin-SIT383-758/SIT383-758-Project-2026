using UnityEngine;

public class PersistanceScript : MonoBehaviour
{
    public static PersistanceScript Instance;

    public string selectedRunway;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }
}