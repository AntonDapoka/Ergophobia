using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    [SerializeField] private Vector3 offset = new Vector3(0f, 15f, -10f);

    [Range(0f, 1f)] [SerializeField] private float smoothTime = 0.1f;

    [SerializeField] private bool lookAtTarget = true;

    //store the velocity of the camera for SmoothDamp
    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

        if (lookAtTarget)
        {
            transform.LookAt(target.position);
        }
    }
}