using UnityEngine;

public class LockButtonState : MonoBehaviour
{
    public Material unlockedMat;
    public Material lockedMat;

    private Vector3 unlockedPos = new Vector3(-2.64f, 0.0f, 0.0f);
    private Vector3 lockedPos = new Vector3(-2.0f, 0.0f, 0.0f);

    private bool locked = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Unlock();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeLockState(bool newLock)
    {
        locked = newLock;
        if (locked) Lock(); else Unlock();
    }

    private void Unlock()
    {
        Renderer[] r = GetComponentsInChildren<Renderer>();
        foreach (Renderer c in r) c.material = unlockedMat;
        transform.Find("Shackle").localPosition = unlockedPos;
    }

    private void Lock()
    {
        Renderer[] r = GetComponentsInChildren<Renderer>();
        foreach (Renderer c in r) c.material = lockedMat;
        transform.Find("Shackle").localPosition = lockedPos;
    }
}
