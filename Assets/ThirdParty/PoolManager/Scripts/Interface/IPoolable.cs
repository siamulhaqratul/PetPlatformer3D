public interface IPoolable
{
    /// <summary>
    /// Called when the object is returned to the pool. Implement this method to handle any cleanup or state reset required before the object is reused.
    /// </summary>
    void OnReturnToPool();

    /// <summary>
    /// Called when the object is spawned from the pool.
    /// Implement this method to initialize or reset the object's state upon activation.
    /// </summary>
    void OnSpawnFromPool();
}