
using UnityEngine;

public class MouseToWorldScript : MonoBehaviour
{
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Transform aimTarget;


    private void Update()
    {
        Ray ray = worldCamera.ScreenPointToRay(Input.mousePosition);

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            aimTarget.position = worldPos;
        }
    }
}