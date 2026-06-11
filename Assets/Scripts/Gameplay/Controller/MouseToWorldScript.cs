using UnityEngine;

public class MouseToWorldScript : MonoBehaviour
{
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Transform aimTarget;
    [SerializeField] private float fixedY = 1f;

    private Plane aimPlane;

    private void Update()
    {
        aimPlane = new Plane(Vector3.up, Vector3.up * fixedY);

        Ray ray = worldCamera.ScreenPointToRay(Input.mousePosition);

        if (aimPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            worldPos.y = fixedY;

            aimTarget.position = worldPos;
        }
    }
}