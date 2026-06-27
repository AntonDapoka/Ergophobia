using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerAudio : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("移动音效")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField][Range(0f, 1f)] private float footstepVolume = 0.5f;

    [Header("攻击音效")]
    [SerializeField] private AudioClip[] attackSounds; // 挥动武器的声音
    [SerializeField][Range(0f, 1f)] private float attackVolume = 0.8f;

    [Header("受击音效")]
    [SerializeField] private AudioClip[] hurtSounds; // 挨打时的惨叫/闷哼声
    [SerializeField][Range(0f, 1f)] private float hurtVolume = 1.0f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        // 玩家的声音通常不需要 3D 衰减，因为摄像机一直跟着玩家
        audioSource.spatialBlend = 0f;
    }

    // ==========================================
    // 供其他脚本调用的公开方法
    // ==========================================

    public void PlayFootstep()
    {
        if (footstepSounds != null && footstepSounds.Length > 0)
        {
            AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
            audioSource.pitch = Random.Range(0.9f, 1.1f); // 随机音调，避免单调
            audioSource.PlayOneShot(clip, footstepVolume);
        }
    }

    public void PlayAttack()
    {
        if (attackSounds != null && attackSounds.Length > 0)
        {
            AudioClip clip = attackSounds[Random.Range(0, attackSounds.Length)];
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(clip, attackVolume);
        }
    }

    public void PlayHurt()
    {
        if (hurtSounds != null && hurtSounds.Length > 0)
        {
            AudioClip clip = hurtSounds[Random.Range(0, hurtSounds.Length)];
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clip, hurtVolume);
        }
    }
}
