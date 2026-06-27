using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyAudio : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("移动音效 (距离驱动)")]
    public AudioClip[] footstepSounds;
    [Range(0f, 1f)] public float footstepVolume = 0.5f;
    [Tooltip("每移动多远播放一次脚步声")]
    public float stepDistance = 1.5f;

    private Vector3 lastPosition;
    private float accumulatedDistance = 0f;

    [Header("攻击音效 (代码调用)")]
    public AudioClip attackSwingSound;
    public AudioClip attackHitSound;
    [Range(0f, 1f)] public float attackVolume = 1.0f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f; // 3D音效

        lastPosition = transform.position;
    }

    private void Update()
    {
        // ==========================================
        // 自动计算移动距离并播放脚步声
        // ==========================================
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);

        // 只有在实际发生移动时才累加距离 (忽略微小的浮点数抖动)
        if (distanceMoved > 0.01f)
        {
            accumulatedDistance += distanceMoved;

            // 当累加距离超过设定的步长时，播放声音并清零
            if (accumulatedDistance >= stepDistance)
            {
                PlayFootstep();
                accumulatedDistance = 0f;
            }
        }

        lastPosition = transform.position;
    }

    private void PlayFootstep()
    {
        if (footstepSounds != null && footstepSounds.Length > 0)
        {
            AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
            audioSource.pitch = Random.Range(0.9f, 1.1f); // 随机音调避免单调
            audioSource.PlayOneShot(clip, footstepVolume);
        }
    }

    // ==========================================
    // 供状态机调用的攻击音效接口
    // ==========================================

    /// <summary>
    /// 播放攻击挥动声音 (可设置延迟以对齐动画)
    /// </summary>
    public void PlayAttackSwing(float delay = 0f)
    {
        if (attackSwingSound == null) return;

        if (delay > 0f)
        {
            StartCoroutine(DelayedPlay(attackSwingSound, attackVolume, delay));
        }
        else
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(attackSwingSound, attackVolume);
        }
    }

    /// <summary>
    /// 播放击中声音 (在 Hitbox 判定成功时调用)
    /// </summary>
    public void PlayAttackHit()
    {
        if (attackHitSound != null)
        {
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(attackHitSound, attackVolume);
        }
    }

    private IEnumerator DelayedPlay(AudioClip clip, float volume, float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(clip, volume);
    }
}