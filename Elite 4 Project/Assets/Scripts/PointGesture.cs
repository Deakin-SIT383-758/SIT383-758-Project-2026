using Fusion;
using UnityEngine;

public class PointGesture : MonoBehaviour
{
    [SerializeField] private OVRHand hand;
    [SerializeField] private OVRSkeleton skeleton;
    [SerializeField] private NetworkedMarker markerPrefab;
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private NetworkRunner runner;

    private bool gestureActive = false;
    private bool hasHit = false;
    private Vector3 lastHitPoint;
    private Vector3 lastRayOrigin;

    public void OnGestureActivated()
    {
        gestureActive = true;
        hasHit = false;
    }
    public void OnGestureDeactivated()
    {
        gestureActive = false;

        if (hasHit)
        {
            runner.Spawn(
                markerPrefab,
                lastHitPoint,
                Quaternion.identity,
                runner.LocalPlayer,
                (runner, obj) =>
                {
                    obj.GetComponent<NetworkedMarker>().Initialise(lastHitPoint, lastRayOrigin);
                }
            );
        }

        hasHit = false;
    }

    void Update()
    {
        if (!gestureActive) return;

        // Get index fingertip bone
        var tip = GetBoneTransform(OVRSkeleton.BoneId.Hand_IndexTip);
        var knuckle = GetBoneTransform(OVRSkeleton.BoneId.Hand_Index1);

        if (tip == null || knuckle == null) return;

        Vector3 origin = tip.position;
        Vector3 direction = (tip.position - knuckle.position).normalized;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, 10f, terrainLayer))
        {
            lastHitPoint = hit.point;
            lastRayOrigin = origin;
            hasHit = true;
        }
    }

    private Transform GetBoneTransform(OVRSkeleton.BoneId boneId)
    {
        foreach (var bone in skeleton.Bones)
        {
            if (bone.Id == boneId) return bone.Transform;
        }

        return null;
    }
}