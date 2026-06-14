using UnityEngine;

public class CameraBehaivourScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelTransitionManager transitionManager;
    [SerializeField] private Transform player;

    [Header("Base Offset From Room")]
    [SerializeField] private Vector3 positionOffset = new(0f, 30f, 20f);
    [SerializeField] private Vector3 rotationOffset = new(55f, -180f, 0f);

    [Header("Follow Rectangle")]
    [SerializeField] private float boundsWidth = 4f;
    [SerializeField] private float boundsDepth = 3f;

    [Header("Smoothing")]
    [SerializeField] private float smoothTime = 0.2f;

    private Vector3 currentVelocity;
    private RoomScript lastRoom;

    private void LateUpdate()
    {
        if (transitionManager == null || transitionManager.CurrentRoom == null) return;

        Transform roomTransform = transitionManager.CurrentRoom.transform;
        Quaternion baseRotation = roomTransform.rotation * Quaternion.Euler(rotationOffset);
        Vector3 targetPosition = GetTargetPosition(roomTransform);

        bool roomChanged = lastRoom != transitionManager.CurrentRoom;
        if (roomChanged)
        {
            transform.position = targetPosition;
            currentVelocity = Vector3.zero;
            lastRoom = transitionManager.CurrentRoom;
        }
        else transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

        transform.rotation = baseRotation;
    }

    private Vector3 GetTargetPosition(Transform roomTransform)
    {
        Vector3 basePosition = roomTransform.TransformPoint(positionOffset);
        if (player == null) return basePosition;

        Vector3 localPlayerOffset = roomTransform.InverseTransformPoint(player.position);
        localPlayerOffset.y = 0f;
        localPlayerOffset.x = Mathf.Clamp(localPlayerOffset.x, -boundsWidth, boundsWidth);
        localPlayerOffset.z = Mathf.Clamp(localPlayerOffset.z, -boundsDepth, boundsDepth);

        return basePosition + roomTransform.TransformVector(localPlayerOffset);
    }
}
