using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton that spawns FX prefabs by index at a position or at a target transform, with optional direction-based rotation/scale or explicit rotation/scale.
/// Instances are destroyed after a fixed lifetime.
/// </summary>
public class FxManager : MonoBehaviour
{
    /// <summary>Seconds after spawn before the FX instance is destroyed.</summary>
    private const float InstanceLifetime = 20f;

    /// <summary>Global singleton instance of the FX manager.</summary>
    public static FxManager Instance { get; private set; }

    /// <summary>List of FX prefabs; index is used by SpawnAtPosition and SpawnAtTransform.</summary>
    [SerializeField] private List<GameObject> prefabs = new List<GameObject>();

    /// <summary>Offset added to spawn position.</summary>
    [SerializeField] private Vector3 positionOffset = Vector3.zero;

    /// <summary>Default scale (used when not overridden by direction or parameter).</summary>
    [SerializeField] private Vector3 scale = Vector3.one;

    /// <summary>
    /// Registers this instance as the singleton; destroys duplicate instances.
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Spawns the prefab at the given index at a world position, with rotation and scale derived from direction.
    /// </summary>
    /// <param name="index">Index into the prefabs list.</param>
    /// <param name="position">World position (positionOffset is added).</param>
    /// <param name="direction">Direction used for Z rotation and scale.</param>
    /// <returns>The spawned instance, or null if index is invalid or prefab is null.</returns>
    public GameObject SpawnAtPosition(int index, Vector3 position, Direction direction = Direction.Left)
    {
        if (!IsValidIndex(index))
            return null;

        GameObject prefab = prefabs[index];
        if (prefab == null)
        {
            Debug.LogWarning($"[FxManager] Prefab at index {index} is null.");
            return null;
        }

        Quaternion rotation = Quaternion.Euler(0f, 0f, DirectionToZAngle(direction));
        GameObject instance = Instantiate(prefab, position + positionOffset, rotation);
        instance.transform.localScale = DirectionToScale(direction)*2.0f;
        Destroy(instance, InstanceLifetime);
        return instance;
    }

    /// <summary>
    /// Spawns the prefab at the given index at a world position with explicit rotation and scale.
    /// </summary>
    /// <param name="index">Index into the prefabs list.</param>
    /// <param name="position">World position (positionOffset is added).</param>
    /// <param name="rotation">Rotation for the instance.</param>
    /// <param name="scale">Local scale for the instance.</param>
    /// <returns>The spawned instance, or null if index is invalid or prefab is null.</returns>
    public GameObject SpawnAtPosition(int index, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (!IsValidIndex(index))
            return null;

        GameObject prefab = prefabs[index];
        if (prefab == null)
        {
            Debug.LogWarning($"[FxManager] Prefab at index {index} is null.");
            return null;
        }

        GameObject instance = Instantiate(prefab, position + positionOffset, rotation);
        instance.transform.localScale = scale;
        Destroy(instance, InstanceLifetime);
        return instance;
    }

    /// <summary>
    /// Spawns the prefab at the given index at the target's position, with rotation and scale derived from direction.
    /// </summary>
    /// <param name="index">Index into the prefabs list.</param>
    /// <param name="target">GameObject whose transform position is used (plus positionOffset).</param>
    /// <param name="direction">Direction used for Z rotation and scale.</param>
    /// <returns>The spawned instance, or null if target is null, index is invalid, or prefab is null.</returns>
    public GameObject SpawnAtTransform(int index, GameObject target, Direction direction = Direction.Left)
    {
        if (target == null)
        {
            Debug.LogWarning("[FxManager] SpawnAtTransform: target is null.");
            return null;
        }
        if (!IsValidIndex(index))
            return null;

        GameObject prefab = prefabs[index];
        if (prefab == null)
        {
            Debug.LogWarning($"[FxManager] Prefab at index {index} is null.");
            return null;
        }

        Quaternion rotation = Quaternion.Euler(0f, 0f, DirectionToZAngle(direction));
        GameObject instance = Instantiate(prefab, target.transform.position + positionOffset, rotation);
        instance.transform.localScale = DirectionToScale(direction)*2.0f;
        Destroy(instance, InstanceLifetime);
        return instance;
    }

    /// <summary>
    /// Returns the Z angle in degrees for the given direction (for FX orientation).
    /// </summary>
    /// <param name="direction">The direction.</param>
    /// <returns>Z Euler angle in degrees.</returns>
    private static float DirectionToZAngle(Direction direction)
    {
        return direction switch
        {
            Direction.Down => -90f,
            Direction.Right => 0f,
            Direction.Up => -90f,
            Direction.Left => 0f,
            Direction.Neutral => 0f,
            _ => 0f
        };
    }

    /// <summary>
    /// Returns a base scale vector for the given direction (additional scaling may be applied by callers).
    /// </summary>
    /// <param name="direction">The direction.</param>
    /// <returns>Scale vector for the FX.</returns>
    private static Vector3 DirectionToScale(Direction direction)
    {
        return direction switch
        {
            Direction.Down => new Vector3(-1f, 1f, 1f),
            Direction.Right => new Vector3(-1f, 1f, 1f),
            Direction.Up => Vector3.one,
            Direction.Left => Vector3.one,
            Direction.Neutral => Vector3.one,
            _ => Vector3.one
        };
    }

    /// <summary>
    /// Returns whether the index is within the prefabs list range.
    /// </summary>
    /// <param name="index">Prefab index to validate.</param>
    /// <returns>True if index is valid.</returns>
    private bool IsValidIndex(int index)
    {
        if (index < 0 || index >= prefabs.Count)
        {
            Debug.LogWarning($"[FxManager] Invalid prefab index: {index}. Valid range: 0-{prefabs.Count - 1}.");
            return false;
        }
        return true;
    }
}
