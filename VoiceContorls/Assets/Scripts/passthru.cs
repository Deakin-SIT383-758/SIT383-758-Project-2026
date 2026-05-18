using UnityEngine;
using Oculus.Haptics;
using Meta.XR.MRUtilityKit;

public class passthru : MonoBehaviour
{
    [SerializeField] private OVRPassthroughLayer passthroughLayer;
    private bool isPassthrough = true;

    public Transform rayStartpoint;
    public float rayLength = 5;
    public MRUKAnchor.SceneLabels lableFilter;
    public GameObject Stuckup;
    [SerializeField] private Vector3 up = new Vector3(0, 0.5f, 0);
    [SerializeField] private GameObject controller;

    public HapticClip hapticClip;
    float timer = 0.0f;
    private HapticClipPlayer player;

    public AudioSource Eudio;


    void Start()
    {
        player = new HapticClipPlayer(hapticClip);
        if (passthroughLayer != null)
        {
            passthroughLayer.enabled = true;
            isPassthrough = true;
        }
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            TogglePassthrough();
        }
        if (OVRInput.Get(OVRInput.RawButton.B))
        {
            Ray ray = new Ray(rayStartpoint.position, rayStartpoint.forward);
            MRUKRoom room = MRUK.Instance.GetCurrentRoom();

            bool hasHit = room.Raycast(ray, rayLength, LabelFilter.FromEnum(lableFilter), out RaycastHit hit, out MRUKAnchor anchor);

            if (hasHit)
            {
                Vector3 hitpoint = hit.point;
                Vector3 hitNormal = controller.transform.localEulerAngles;


                Stuckup.transform.position = hitpoint + up;

                //Change rotation
                Vector3 newRotation = Stuckup.transform.eulerAngles;
                newRotation.y = controller.transform.eulerAngles.y;
                Stuckup.transform.eulerAngles = newRotation;

                timer -= Time.deltaTime;
                if (timer < 0.0f)
                {
                    player.Play(Controller.Left);
                    player.Play(Controller.Right);
                    timer = player.clipDuration;
                    Eudio.Play();
                }
            }
        }

        if (OVRInput.GetUp(OVRInput.RawButton.B))
        {
            timer = player.clipDuration;
            Eudio.Stop();
        }
    }
    private void TogglePassthrough()
    {
        isPassthrough = !isPassthrough;
        passthroughLayer.enabled = isPassthrough;
    }
}
