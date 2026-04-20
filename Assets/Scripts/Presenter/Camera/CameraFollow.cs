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
            transform.position = targetFollow.position + Vector3.forward;

        UpdateRotation();
    }

    private void LateUpdate()
    {
        if (!isTurnOn) return;

        if (targetFollow != null)
        {
            Vector3 targetPos = targetFollow.position + Vector3.forward * horizontalOffset;
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
        }

        //UpdateRotation();
    }

    private void UpdateRotation()
    {
        if (targetLook != null)
        {
            transform.rotation = Quaternion.LookRotation(targetLook.position - transform.position);
        }
    }
}