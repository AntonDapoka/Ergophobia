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
    public float stepDistance = 1f;
    private Vector3 lastPosition;
    private float accumulatedDistance = 0f;

    [Header("基础战斗音效")]
    public AudioClip attackSound;
    public AudioClip[] deathSounds; // 死亡惨叫声 (数组随机)
    [Range(0f, 1f)] public float attackVolume = 1.0f;
    [Range(0f, 1f)] public float deathVolume = 1.0f;

    [Header("特例化音效 (按需配置)")]
    [Tooltip("自爆敌人的爆炸声")]
    public AudioClip explosionSound;
    [Tooltip("盾牌敌人的格挡弹刀声")]
    public AudioClip blockSound;
    public AudioClip dashSound;
    public AudioClip specialSound;
    [Range(0f, 1f)] public float specialVolume = 1.0f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        lastPosition = transform.position;
    }

    private void Update()
    {
        // 自动计算移动距离并播放脚步声
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);

        if (distanceMoved > 0.01f)
        {
            accumulatedDistance += distanceMoved;
            if (accumulatedDistance >= stepDistance)
            {
                PlayFootstep();
                accumulatedDistance = 0f;
            }
        }
        lastPosition = transform.position;
    }

    // ==========================================
    // 基础音效接口
    // ==========================================

    private void PlayFootstep()
    {
        if (footstepSounds != null && footstepSounds.Length > 0)
        {
            AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
            audioSource.PlayOneShot(clip, footstepVolume);
        }
    }

    public void PlayAttack(float delay = 0f)
    {
        if (attackSound == null) return;
        if (delay > 0f) StartCoroutine(DelayedPlay(attackSound, attackVolume, delay));
        else
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(attackSound, attackVolume);
        }
    }


    // ==========================================
    // 死亡与特例化音效接口 (解决销毁吞音问题)
    // ==========================================

    /// <summary>
    /// 播放死亡音效 (生成独立音源，防止敌人销毁导致声音中断)
    /// </summary>
    public void PlayDeath()
    {
        if (deathSounds != null && deathSounds.Length > 0)
        {
            AudioClip clip = deathSounds[Random.Range(0, deathSounds.Length)];
            PlaySoundAtLocation(clip, deathVolume);
        }
    }

    /// <summary>
    /// 播放自爆音效
    /// </summary>
    public void PlayExplosion()
    {
        if (explosionSound != null)
        {
            PlaySoundAtLocation(explosionSound, specialVolume);
        }
    }

    /// <summary>
    /// 播放盾牌格挡音效 (格挡时敌人通常不会死，直接用 OneShot 即可)
    /// </summary>
    public void PlayBlock()
    {
        if (blockSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(blockSound, specialVolume);
        }
    }

    public void PlayDash()
    {       
        PlaySoundAtLocation(dashSound, specialVolume);
    }

    public void PlaySpecial()
    {
        audioSource.PlayOneShot(specialSound, specialVolume);
    }
    // ==========================================
    // 内部辅助方法
    // ==========================================

    private IEnumerator DelayedPlay(AudioClip clip, float volume, float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(clip, volume);
    }

    /// <summary>
    /// 在指定位置生成一个临时音源播放声音，播完自动销毁。
    /// 完美保留了 AudioMixer 的混音设置！
    /// </summary>
    private void PlaySoundAtLocation(AudioClip clip, float volume)
    {
        if (clip == null) return;

        // 创建一个空的 GameObject 作为临时播放器
        GameObject tempAudioObj = new GameObject("TempAudio_" + clip.name);
        tempAudioObj.transform.position = transform.position;

        // 添加并配置 AudioSource
        AudioSource tempSource = tempAudioObj.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.volume = volume;

        tempSource.rolloffMode = AudioRolloffMode.Linear;

        // 播放并在音频结束后销毁临时物体
        tempSource.Play();
        Destroy(tempAudioObj, clip.length);
    }
}