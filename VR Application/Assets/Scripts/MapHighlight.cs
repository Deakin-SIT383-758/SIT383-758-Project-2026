using UnityEngine;
using Fusion;

public class MapHighlight : NetworkBehaviour
{
    private const float LIFETIME = 10f;

    // TickTimer is Fusion's built-in networked timer, synced across all players
    [Networked] private TickTimer LifeTimer { get; set; }

    public override void Spawned()
    {
        // Only the state authority (the player who spawned it) starts the timer
        // Other players just wait and react when it despawns
        if (Object.HasStateAuthority)
            LifeTimer = TickTimer.CreateFromSeconds(Runner, LIFETIME);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        // Once the timer runs out, despawn the object for all players
        if (LifeTimer.Expired(Runner))
            Runner.Despawn(Object);
    }
}