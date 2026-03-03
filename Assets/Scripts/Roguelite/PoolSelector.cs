using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Picks at random from pools for roguelite artistic/asset fields (enemy, music, environment).
/// </summary>
public static class PoolSelector
{
    /// <summary>
    /// Returns a random element from the list, or null if list is null or empty.
    /// </summary>
    /// <param name="pool">List to pick from (e.g. enemy pool, music pool).</param>
    /// <returns>Random element or null if pool is null or empty.</returns>
    public static T Pick<T>(IReadOnlyList<T> pool) where T : Object
    {
        if (pool == null || pool.Count == 0)
            return null;
        return pool[Random.Range(0, pool.Count)];
    }
}
