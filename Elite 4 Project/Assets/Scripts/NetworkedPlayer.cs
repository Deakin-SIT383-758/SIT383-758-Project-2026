using UnityEngine;
using Fusion;

/// <summary>
/// Syncs the local player's head and hand poses across the Fusion session.
/// Reads from the Meta [BuildingBlock] Camera Rig in the scene.
///
/// ── Prefab structure (create these as children) ──
///   NetworkedPlayer         (NetworkObject + this script)
///   ├── HeadTarget          assign to headTarget
///   ├── LeftHandTarget      assign to leftHandTarget
///   └── RightHandTarget     assign to rightHandTarget
///
/// ── Scene structure (already exists via Building Blocks) ──
///   [BuildingBlock] Camera Rig  (OVRCameraRig)
///   └── TrackingSpace
///       ├── CenterEyeAnchor     → localHead      (auto-assigned)
///       ├── LeftHandAnchor      → localLeftHand   (auto-assigned)
///       └── RightHandAnchor     → localRightHand  (auto-assigned)
/// </summary>
public class NetworkedPlayer : NetworkBehaviour
{
    [Header("Local Rig — auto-assigned at runtime, no need to set manually")]
    public Transform localHead;
    public Transform localLeftHand;
    public Transform localRightHand;

    [Header("Networked Targets — assign the child transforms of this prefab")]
    public Transform headTarget;
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    [Header("Visuals — shown for remote players, hidden for local player")]
    public GameObject headVisual;
    public Renderer leftHandRenderer;
    public Renderer rightHandRenderer;

    [Networked] private Vector3 HeadPosition { get; set; }
    [Networked] private Quaternion HeadRotation { get; set; }
    [Networked] private Vector3 LeftHandPos { get; set; }
    [Networked] private Quaternion LeftHandRot { get; set; }
    [Networked] private Vector3 RightHandPos { get; set; }
    [Networked] private Quaternion RightHandRot { get; set; }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            // This is the local player's instance — find the Building Block rig
            AssignOVRAnchors();

            // Hide our own avatar — we see our real hands via the Building Block
            if (headVisual != null) headVisual.SetActive(false);
            if (leftHandRenderer != null) leftHandRenderer.enabled = false;
            if (rightHandRenderer != null) rightHandRenderer.enabled = false;
        }
        else
        {
            // This is a remote player's instance — show their avatar visuals
            if (headVisual != null) headVisual.SetActive(true);
            if (leftHandRenderer != null) leftHandRenderer.enabled = true;
            if (rightHandRenderer != null) rightHandRenderer.enabled = true;
        }
    }

    private void AssignOVRAnchors()
    {
        var rig = FindFirstObjectByType<OVRCameraRig>();

        if (rig == null)
        {
            Debug.LogError("[NetworkedPlayer] Could not find OVRCameraRig in scene. " +
                           "Make sure [BuildingBlock] Camera Rig is present.");
            return;
        }

        // These properties map directly to the transforms you can see in your hierarchy:
        // CenterEyeAnchor, LeftHandAnchor, RightHandAnchor
        localHead = rig.centerEyeAnchor;
        localLeftHand = rig.leftHandAnchor;
        localRightHand = rig.rightHandAnchor;

        Debug.Log("[NetworkedPlayer] OVR anchors assigned successfully.");
    }

    public override void FixedUpdateNetwork()
    {
        // Only the local player writes pose data to the network
        if (!HasStateAuthority) return;

        if (localHead != null)
        {
            HeadPosition = localHead.position;
            HeadRotation = localHead.rotation;
        }

        if (localLeftHand != null)
        {
            LeftHandPos = localLeftHand.position;
            LeftHandRot = localLeftHand.rotation;
        }

        if (localRightHand != null)
        {
            RightHandPos = localRightHand.position;
            RightHandRot = localRightHand.rotation;
        }
    }

    public override void Render()
    {
        // Every client applies the latest synced poses to the visible rig targets
        if (headTarget != null)
        {
            headTarget.position = HeadPosition;
            headTarget.rotation = HeadRotation;
        }

        if (leftHandTarget != null)
        {
            leftHandTarget.position = LeftHandPos;
            leftHandTarget.rotation = LeftHandRot;
        }

        if (rightHandTarget != null)
        {
            rightHandTarget.position = RightHandPos;
            rightHandTarget.rotation = RightHandRot;
        }
    }
}