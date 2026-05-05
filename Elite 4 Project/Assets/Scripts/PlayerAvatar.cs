using Fusion;
using UnityEngine;
public class PlayerAvatar : NetworkBehaviour
{
    [Header("Avatar Anchors (assign in Inspector)")]
    [SerializeField] private Transform headAnchor;
    [SerializeField] private Transform leftHandAnchor;
    [SerializeField] private Transform rightHandAnchor;
    // Networked properties — Fusion automatically syncs these 
    // to every player in the session every network tick. 
    [Networked] private Vector3 HeadPos { get; set; }
    [Networked] private Quaternion HeadRot { get; set; }
    [Networked] private Vector3 LeftHandPos { get; set; }
    [Networked] private Quaternion LeftHandRot { get; set; }
    [Networked] private Vector3 RightHandPos { get; set; }
    [Networked] private Quaternion RightHandRot { get; set; }

    // Local references to the OVRCameraRig anchors 
    private Transform ovrHead;
    private Transform ovrLeftHand;
    private Transform ovrRightHand;

    public override void Spawned()
    {
        // Only the local player needs to find the OVRCameraRig. 
        // Remote players just read the synced Networked values. 
        if (!HasStateAuthority) return;

        OVRCameraRig rig = FindFirstObjectByType<OVRCameraRig>();
        if (rig != null)
        {
            // CenterEyeAnchor is the head position in Meta SDK 
            ovrHead = rig.centerEyeAnchor;
            ovrLeftHand = rig.leftHandAnchor;
            ovrRightHand = rig.rightHandAnchor;

            Debug.LogError($"Head: {ovrHead.name}, L: {ovrLeftHand.name}, R: {ovrRightHand.name}"); 
        }
        else
        {
            // Find whatever camera exists in the scene regardless of tag
            Camera cam = FindFirstObjectByType<Camera>();

            if (cam != null)
            {
                ovrHead = cam.transform;
                ovrLeftHand = cam.transform;
                ovrRightHand = cam.transform;
                Debug.Log($"PC fallback using camera: {cam.name} at {cam.transform.position}");
            }
            else
            {
                Debug.LogError("No camera found at all!");
            }
        }

        // Hide this avatar's visuals for the local player 
        // so you do not see a head floating in front of you. 
        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Only the local player writes their own tracking data. 
        if (!HasStateAuthority) return;

        if (ovrHead != null)
        {
            HeadPos = ovrHead.position;
            HeadRot = ovrHead.rotation;
        }
        if (ovrLeftHand != null)
        {
            LeftHandPos = ovrLeftHand.position;
            LeftHandRot = ovrLeftHand.rotation;
        }
        if (ovrRightHand != null)
        {
            RightHandPos = ovrRightHand.position;
            RightHandRot = ovrRightHand.rotation;
        }
    }
    public override void Render()
    {
        // Every player (local and remote) applies the synced data 
        // to the avatar visuals every rendered frame. 
        headAnchor.SetPositionAndRotation(HeadPos, HeadRot);
        leftHandAnchor.SetPositionAndRotation(LeftHandPos, LeftHandRot);
        rightHandAnchor.SetPositionAndRotation(RightHandPos, RightHandRot);
    }
}