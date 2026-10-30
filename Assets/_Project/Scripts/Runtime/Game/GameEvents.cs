using System;
using UnityEngine;

public static class GameEvents
{
    // ============================ EVENTS =============================

    public static event Action OnPetHit;
    public static event Action<Vector3> OnCheckpointReached;
    public static event Action OnFinishReached;
    public static event Action OnRunReset;
    // The position is where the coin was picked up.
    public static event Action<Vector3> OnCoinCollected;
    public static event Action<float> OnPetRespawned;

    // ========================= STATIC METHODS ========================

    // Domain reload is disabled in Enter Play Mode Options, so stale listeners are cleared here.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ClearListeners()
    {
        OnPetHit = null;
        OnCheckpointReached = null;
        OnFinishReached = null;
        OnRunReset = null;
        OnCoinCollected = null;
        OnPetRespawned = null;
    }

    public static void RaisePetHit()
    {
        OnPetHit?.Invoke();
    }

    public static void RaiseCheckpointReached(Vector3 respawnPosition)
    {
        OnCheckpointReached?.Invoke(respawnPosition);
    }

    public static void RaiseFinishReached()
    {
        OnFinishReached?.Invoke();
    }

    public static void RaiseRunReset()
    {
        OnRunReset?.Invoke();
    }

    public static void RaiseCoinCollected(Vector3 position)
    {
        OnCoinCollected?.Invoke(position);
    }

    public static void RaisePetRespawned(float respawnZ)
    {
        OnPetRespawned?.Invoke(respawnZ);
    }
}
