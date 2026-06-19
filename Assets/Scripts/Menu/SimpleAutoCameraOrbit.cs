using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SimpleAutoCameraOrbit : MonoBehaviour
{
    public Transform rotationTarget;
    public float rotationSpeed = 30f;

    private void Start()
    {
        if (rotationTarget == null)
        {
            Debug.LogWarning("!!!!!!!!");
            GameObject defaultTarget = new GameObject("AutoRotationTarget");
            rotationTarget = defaultTarget.transform;
            rotationTarget.position = Vector3.zero;
        }
    }

    private void LateUpdate()
    {
        if (rotationTarget == null) return;
        transform.RotateAround(rotationTarget.position, Vector3.up, rotationSpeed * Time.deltaTime);
    }
}