using UnityEngine;

public class ShieldComponent : MonoBehaviour
{
    [Header("Shield Settings")]
    [SerializeField] private float blockAngle = 120f;

    //[Header("Feedback")]
    //public AudioClip blockSound; 

    public bool TryBlock(Vector3 damageSourcePosition)
    {
        if (!this.enabled) return false;

        Vector3 dirToAttacker = (damageSourcePosition - transform.position).normalized;
        dirToAttacker.y = 0;
        Vector3 enemyForward = transform.forward;
        enemyForward.y = 0;

        float angle = Vector3.Angle(enemyForward, dirToAttacker);
        if (angle <= blockAngle / 2f)
        {
            PlayBlockFeedback(damageSourcePosition);
            return true;
        }
        return false;
    }
    private void PlayBlockFeedback(Vector3 hitPos)
    {
        // GetComponent<Animator>().SetTrigger("Block");
    }
}