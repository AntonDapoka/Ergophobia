using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public bool isTurnOn = true;

    [SerializeField] private Transform targetFollow;
    [SerializeField] private Transform targetLook;

    [SerializeField] private float followSpeed = 5f;

    [SerializeField] private float horizontalOffset = 0f;

    private void Start()
    {
        if (targetFollow != null)
        {
            transform.position = targetFollow.position;
        }

        UpdateRotation();
    }

    private void LateUpdate()
    {
        if (!isTurnOn) return;

        if (targetFollow != null)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetFollow.position,
                followSpeed * Time.deltaTime
            );
        }

        UpdateRotation();
    }

    private void UpdateRotation()
    {
        if (targetLook != null)
        {
            Vector3 offset = transform.right * horizontalOffset;
            Vector3 finalLookPoint = targetLook.position + offset;

            transform.rotation = Quaternion.LookRotation(finalLookPoint - transform.position);
        }
    }
}