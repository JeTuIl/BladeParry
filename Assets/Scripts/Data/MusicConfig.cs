using UnityEngine;

/// <summary>
/// ScriptableObject for fight music: clip to play and pitch speeds at full/empty enemy life.
/// Used by FightConfig; applied at fight setup in GameplayLoopController.
/// </summary>
[CreateAssetMenu(fileName = "MusicConfig", menuName = "BladeParry/Music Config")]
public class MusicConfig : ScriptableObject
{
    [Tooltip("Music clip to play during the fight. When set, applied at fight start via MusciSwitchManager.")]
    [SerializeField] private AudioClip musicClip;

    [Tooltip("Music pitch when enemy is at full life.")]
    [SerializeField] private float fullLifeMusicSpeed = 1f;

    [Tooltip("Music pitch when enemy is at empty life.")]
    [SerializeField] private float emptyLifeMusicSpeed = 1f;

    /// <summary>Music clip to play during the fight.</summary>
    public AudioClip MusicClip => musicClip;

    /// <summary>Music pitch when enemy is at full life.</summary>
    public float FullLifeMusicSpeed => fullLifeMusicSpeed;

    /// <summary>Music pitch when enemy is at empty life.</summary>
    public float EmptyLifeMusicSpeed => emptyLifeMusicSpeed;

    /// <summary>Sets the music clip (for runtime-created config).</summary>
    public void SetMusicClip(AudioClip value) => musicClip = value;

    /// <summary>Sets the full life music speed (for runtime-created config).</summary>
    public void SetFullLifeMusicSpeed(float value) => fullLifeMusicSpeed = value;

    /// <summary>Sets the empty life music speed (for runtime-created config).</summary>
    public void SetEmptyLifeMusicSpeed(float value) => emptyLifeMusicSpeed = value;
}
