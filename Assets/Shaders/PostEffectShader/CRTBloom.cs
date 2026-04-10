using UnityEngine;

public class CRTBloom : PostEffectbase
{
    [Header("Settings")]
    [Range(0, 1)] public float bend = 0.1f;
    [Range(0, 1)] public float scanline = 0.5f;
    [Range(0, 0.1f)] public float scanlineSpeed = 0.02f;
    [Range(0, 1)] public float chromatic = 0.02f;
    [Range(0, 1)] public float noise = 0.1f;
    [Range(0, 1)] public float vignette = 0.5f;
    [Range(0, 1)] public float brightness = 1.0f;

    [Range(0, 4)] public float luminanceThreshoid = 0.5f;

    [Range(1, 8)] public int downsample = 1;
    [Range(1, 8)] public int iteration = 1;
    [Range(0, 3)] public float blurSpread = 0.6f;

    protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
       if(material != null)
        {
            material.SetFloat("_Bend", bend);
            material.SetFloat("_Scanline", scanline);
            material.SetFloat("_ScanlineSpeed", scanlineSpeed);
            material.SetFloat("_Chromatic", chromatic);
            material.SetFloat("_Noise", noise);
            material.SetFloat("_Vignette", vignette);
            material.SetFloat("_Brightness", brightness);
         
            RenderTexture CRTBloom = RenderTexture.GetTemporary(source.width, source.height, 0);

            Graphics.Blit(source, CRTBloom, material, 0);
            material.SetTexture("_MainTex", CRTBloom);

            material.SetFloat("_LuminanceThreshold", luminanceThreshoid);
            int rtw = source.width / downsample;
            int rtH = source.height / downsample;

            RenderTexture buffer = RenderTexture.GetTemporary(rtw, rtH, 0);
            buffer.filterMode = FilterMode.Bilinear;
            Graphics.Blit(CRTBloom, buffer, material, 1);

            for (int i = 0; i < iteration; i++)
            {
                material.SetFloat("_BlurSpread", 1 + i * blurSpread);

                RenderTexture buffer1 = RenderTexture.GetTemporary(rtw, rtH, 0);
                
                Graphics.Blit(buffer, buffer1, material, 2);
                RenderTexture.ReleaseTemporary(buffer);

                buffer = buffer1;
                buffer1 = RenderTexture.GetTemporary(rtw, rtH, 0);
                Graphics.Blit(buffer, buffer1, material, 3);
                RenderTexture.ReleaseTemporary(buffer);
                buffer = buffer1;
            }
            material.SetTexture("_Bloom", buffer);

            Graphics.Blit(CRTBloom, destination, material, 4);

            RenderTexture.ReleaseTemporary(buffer);
            RenderTexture.ReleaseTemporary(CRTBloom);
        }
        else Graphics.Blit(source, destination);
    }
}
