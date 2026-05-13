using UnityEngine;
using Fusion;

public class MapHighlight : NetworkBehaviour
{
    private const float LIFETIME = 10f;
    [Networked] private TickTimer LifeTimer { get; set; }

    public override void Spawned()
    {
        Debug.Log($"MapHighlight Spawned - HasStateAuthority: {Object.HasStateAuthority}");
        if (Object.HasStateAuthority)
        {
            LifeTimer = TickTimer.CreateFromSeconds(Runner, LIFETIME);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if (LifeTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }
}