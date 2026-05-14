using Fusion;
using UnityEngine;

public class NetworkPlayerColor : NetworkBehaviour
{
    [Header("Renderers to Colour")]
    public Renderer[] renderersToColour;

    [Networked] private Vector3 NetworkColour { get; set; }
    [Networked] private NetworkBool HasColour { get; set; }

    public override void Spawned()
    {
        // Only the player who has state authority chooses the colour.
        if (Object.HasStateAuthority && !HasColour)
        {
            Color randomColour = Random.ColorHSV(
                0f, 1f,      // hue range
                0.6f, 1f,    // saturation range
                0.8f, 1f     // brightness range
            );

            NetworkColour = new Vector3(
                randomColour.r,
                randomColour.g,
                randomColour.b
            );

            HasColour = true;
        }
    }

    public override void Render()
    {
        if (!HasColour)
            return;

        Color colour = new Color(
            NetworkColour.x,
            NetworkColour.y,
            NetworkColour.z
        );

        ApplyColour(colour);
    }

    private void ApplyColour(Color colour)
    {
        if (renderersToColour == null)
            return;

        foreach (Renderer rend in renderersToColour)
        {
            if (rend != null)
            {
                // .material creates an instance, so we do not recolour every shared material.
                rend.material.color = colour;
            }
        }
    }
}