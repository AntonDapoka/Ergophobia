using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public bool isTurnOn = true;

    [Header("Target Settings")]
    [SerializeField] private Transform targetFollow;
    [SerializeField] private Transform targetLook;
    [SerializeField] private float followSpeed = 5f;

    [Header("Z-Axis Limits")]
   
    public float minZLimit = -100f;
    public float maxZLimit = 100f;

    private void Start()
    {
        if (targetFollow != null)
        {
            transform.position = targetFollow.position;
        }
    }

    private void LateUpdate()
    {
        if (!isTurnOn) return;

        if (targetFollow != null)
        {
            //get
            Vector3 smoothPosition = Vector3.Lerp(
                transform.position,
                targetFollow.position,
                followSpeed * Time.deltaTime
            );

            // Limit the Z position
            // Mathf.Clamp 
            float clampedZ = Mathf.Clamp(smoothPosition.z, minZLimit, maxZLimit);

            //apply
            transform.position = new Vector3(transform.position.x, transform.position.y, clampedZ);
        }
    }
}