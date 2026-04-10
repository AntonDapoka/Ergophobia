using UnityEngine;

public class GlobalPixel : PostEffectbase
{
    [Range(0, 1024)] public float pixelSize = 64f;
    [ExecuteInEditMode]
    protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material != null)
        {
            material.SetFloat("_PixelRange", pixelSize);
            Graphics.Blit(source, destination, material);
        }
        else Graphics.Blit(source, destination);
    }
}
