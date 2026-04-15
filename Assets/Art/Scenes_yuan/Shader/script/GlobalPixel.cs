using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalPixel : PostEffectbase
{
    [Range(0, 1024)] public float pixelSize = 64f; // 像素化程度
    [ExecuteInEditMode]
    protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material != null)
        {
            // 设置Shader参数
            material.SetFloat("_PixelRange", pixelSize);
            // 应用后处理效果
            Graphics.Blit(source, destination, material);
        }
        else
        {
            // 如果没有材质，直接复制图像
            Graphics.Blit(source, destination);
        }
    }


   
}
