using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("SFX Clips Library")]
    public AudioClip resourceMoveClip;
    public AudioClip cashPickupClip;
    public AudioClip miningClip;  // 일반 타격음 (깡!)
    public AudioClip drillClip;   // 드릴 지속음 (드르륵)
    public AudioClip purchaseClip;
    public AudioClip convertClip;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float drillVolume = 0.3f;      // 드릴 소리 볼륨
    [Range(0f, 1f)] public float miningVolume = 0.7f;     // 일반 타격음 볼륨
    [Range(0f, 1f)] public float cashPickupVolume = 0.5f;
    [Range(0f, 1f)] public float defaultVolume = 1.0f;

    private float lastCashPlayTime;
    private float cashSoundCooldown = 0.1f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Play3DSFX(AudioSource source, AudioClip clip, float volume = 1.0f)
    {
        if (source != null && clip != null)
        {
            source.spatialBlend = 1.0f;
            source.PlayOneShot(clip, volume);
        }
    }

    public bool CanPlayCashSound()
    {
        if (Time.time - lastCashPlayTime > cashSoundCooldown)
        {
            lastCashPlayTime = Time.time;
            return true;
        }
        return false;
    }
}