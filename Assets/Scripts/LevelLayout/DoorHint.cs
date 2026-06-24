using UnityEngine;

public class DoorHint : MonoBehaviour
{
    private Transform cameraTransform;

    public void SetCamera(Transform camera)
    {
        cameraTransform = camera;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;
        if (!gameObject.activeInHierarchy) return;

        transform.LookAt(
            transform.position + cameraTransform.rotation * Vector3.forward,
            cameraTransform.rotation * Vector3.up
        );
    }
}
