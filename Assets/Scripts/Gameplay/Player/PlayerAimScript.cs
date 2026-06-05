using UnityEngine;

public class PlayerAimScript : MonoBehaviour
{
    [SerializeField] private Transform player; 
    [SerializeField] private Transform aimTarget;

    private void Update()
    {
        Vector3 direction = aimTarget.position - player.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            targetRotation *= Quaternion.Euler(0f, 180f, 0f);

            player.rotation = targetRotation;
        }
    }
}
