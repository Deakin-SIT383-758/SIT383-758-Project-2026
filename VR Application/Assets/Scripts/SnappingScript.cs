using UnityEngine;

public class SnapEdge : MonoBehaviour
{
    public GameObject target; // object to snap to
    public enum Side { Left, Right, Top, Bottom, Front, Back }
    public Side targetSide = Side.Right; // default side
    public Side mySide = Side.Left;       // which side of this object to align

    [ContextMenu("Snap To Target Side")]
    public void SnapToTargetSide()
    {
        if (target == null) return;

        Renderer myRend = GetComponent<Renderer>();
        Renderer targetRend = target.GetComponent<Renderer>();
        if (myRend == null || targetRend == null) return;

        Vector3 offset = GetSideOffset(myRend.bounds, mySide) - GetSideOffset(targetRend.bounds, targetSide);
        transform.position = targetRend.bounds.center + offset;
    }

    private Vector3 GetSideOffset(Bounds b, Side side)
    {
        switch (side)
        {
            case Side.Left:   return new Vector3(-b.extents.x, 0, 0);
            case Side.Right:  return new Vector3(b.extents.x, 0, 0);
            case Side.Top:    return new Vector3(0, b.extents.y, 0);
            case Side.Bottom: return new Vector3(0, -b.extents.y, 0);
            case Side.Front:  return new Vector3(0, 0, b.extents.z);
            case Side.Back:   return new Vector3(0, 0, -b.extents.z);
        }
        return Vector3.zero;
    }
}