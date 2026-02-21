using System.Collections.Generic;
using UnityEngine;

public class FxManager : MonoBehaviour
{
    private const float InstanceLifetime = 20f;

    public static FxManager Instance { get; private set; }

    [SerializeField] private List<GameObject> prefabs = new List<GameObject>();
    [SerializeField] private Vector3 positionOffset = Vector3.zero;
    [SerializeField] private Vector3 scale = Vector3.one;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

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
