using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseToWorldScript : MonoBehaviour
{
    public Camera finalCamera;        // Камера, которая показывает Quad игроку (Камера B)
    public Camera worldCamera;        // Камера, которая рендерит мир (Камера A)
    public Renderer renderQuad;       // MeshRenderer Quad’а, на который натянута RenderTexture
    public Vector2 renderTextureSize = new Vector2(320, 200); // Размер RenderTexture

    public Transform aimTarget;       // Объект, куда целимся

    void Update()
    {
        Ray ray = finalCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == renderQuad.gameObject)
            {
                // 1. Попали по Quad’у
                Vector2 uv = hit.textureCoord; // координаты попадания в текстуре (0..1)

                // 2. Переводим в пиксели RenderTexture
                float pixelX = uv.x * renderTextureSize.x;
                float pixelY = uv.y * renderTextureSize.y;

                // 3. Нормализуем пиксели для ViewportPoint
                Vector3 viewportPoint = new Vector3(pixelX / renderTextureSize.x, pixelY / renderTextureSize.y, 0f);

                // 4. Строим луч из worldCamera в эту точку
                Ray worldRay = worldCamera.ViewportPointToRay(viewportPoint);

                // 5. Пример: определим, куда указывает мышь в XZ-плоскости (y=0)
                Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                if (groundPlane.Raycast(worldRay, out float distance))
                {
                    Vector3 worldPos = worldRay.GetPoint(distance);
                    aimTarget.position = worldPos;
                }
            }
        }
    }
}