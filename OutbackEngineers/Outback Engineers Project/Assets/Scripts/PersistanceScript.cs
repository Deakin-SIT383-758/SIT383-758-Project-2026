using UnityEngine;

public class PersistanceScript : MonoBehaviour
{
    public static PersistanceScript Instance;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
