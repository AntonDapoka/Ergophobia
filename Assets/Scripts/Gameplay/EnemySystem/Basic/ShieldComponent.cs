using System;
using System.Collections;
using UnityEngine;

public class ShieldComponent : MonoBehaviour
{
    [Header("Shield Settings")]
    [SerializeField] private float blockAngle = 120f;

    [Header("Stun Mechanic (破防机制)")]
    [SerializeField] private int maxBlockCount = 3;       // 挡几次后破防
    [SerializeField] private float stunDuration = 3f;     // 晕眩持续时间

    private int currentBlockCount = 0;
    private bool isStunned = false;

    // 【关键】定义事件，通知状态机或移动脚本
    public event Action OnStunStart;
    public event Action OnStunEnd;

    private Animator anim;
    private EnemyAudio enemyAudio;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        enemyAudio = GetComponent<EnemyAudio>();
    }

    public bool TryBlock(Vector3 damageSourcePosition)
    {
        // 如果组件被禁用，或者正在晕眩中，则格挡失效（直接受伤害）
        if (!this.enabled || isStunned) return false;

        Vector3 dirToAttacker = (damageSourcePosition - transform.position).normalized;
        dirToAttacker.y = 0;
        Vector3 enemyForward = transform.forward;
        enemyForward.y = 0;

        float angle = Vector3.Angle(enemyForward, dirToAttacker);
        if (angle <= blockAngle / 2f)
        {
            currentBlockCount++;

            if (currentBlockCount >= maxBlockCount)
            {
                // 达到最大格挡次数，触发破防晕眩！
                // 注意：这最后一下攻击依然算被挡住了（免伤），但敌人会陷入晕眩
                StartCoroutine(StunRoutine());
            }
            else
            {
                // 正常格挡
                PlayBlockFeedback();
            }
            return true;
        }
        return false;
    }

    private void PlayBlockFeedback()
    {
        if (anim != null) anim.SetTrigger("Block");
        if (enemyAudio != null) enemyAudio.PlayBlock();
    }

    private IEnumerator StunRoutine()
    {
        isStunned = true;

        if (anim != null) anim.SetBool("IsStunned", true); 
        if (enemyAudio != null) enemyAudio.PlaySpecial();     

        OnStunStart?.Invoke();
        Debug.Log("<color=yellow>Shield Enemy is STUNNED!</color>");

        yield return new WaitForSeconds(stunDuration);

        isStunned = false;
        currentBlockCount = 0; 

        if (anim != null) anim.SetBool("IsStunned", false);

        OnStunEnd?.Invoke();
        Debug.Log("<color=green>Shield Enemy recovered from stun.</color>");
    }
}