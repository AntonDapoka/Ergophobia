using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CRTBloom : PostEffectbase
{
    [Header("基础参数")]
    [Range(0, 1)] public float bend = 0.1f;          // 屏幕弯曲
    [Range(0, 1)] public float scanline = 0.5f;      // 扫描线强度
    [Range(0, 0.1f)] public float scanlineSpeed = 0.02f; // 扫描线滚动速度
    [Range(0, 1)] public float chromatic = 0.02f;    // 色差（RGB分离）
    [Range(0, 1)] public float noise = 0.1f;         // 噪点强度
    [Range(0, 1)] public float vignette = 0.5f;      // 暗角
    [Range(0, 1)] public float brightness = 1.0f;    // 亮度

    //亮度阈值变量
    [Range(0, 4)]
    public float luminanceThreshoid = 0.5f;

    [Range(1, 8)]
    public int downsample = 1;
    [Range(1, 8)]
    public int iteration = 1;
    [Range(0, 3)]
    public float blurSpread = 0.6f;


    protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material != null)
        {
            // 设置Shader参数
            material.SetFloat("_Bend", bend);
            material.SetFloat("_Scanline", scanline);
            material.SetFloat("_ScanlineSpeed", scanlineSpeed);
            material.SetFloat("_Chromatic", chromatic);
            material.SetFloat("_Noise", noise);
            material.SetFloat("_Vignette", vignette);
            material.SetFloat("_Brightness", brightness);

            //渲染纹理缓冲区
            RenderTexture CRTBloom = RenderTexture.GetTemporary(source.width, source.height, 0);

            Graphics.Blit(source, CRTBloom, material, 0);
            material.SetTexture("_MainTex", CRTBloom);

            //设置亮度阈值变量
            material.SetFloat("_LuminanceThreshold", luminanceThreshoid);
            int rtw = source.width / downsample;
            int rtH = source.height / downsample;
            //渲染纹理缓冲区
            RenderTexture buffer = RenderTexture.GetTemporary(rtw, rtH, 0);
            //采样双线性模式来进行缩放 可以让缩放效果更平滑
            buffer.filterMode = FilterMode.Bilinear;
            //第一步 提取 用我们提取Pass去得到对应的亮度信息 存入到缓存区纹理
            Graphics.Blit(CRTBloom, buffer, material, 1);

            //模糊处理
            //多次执行高斯模糊逻辑
            for (int i = 0; i < iteration; i++)
            {
                //如果想要模糊效果更强烈更平滑
                //可以在迭代中进行设置 相当于每次迭代时都在增加 间隔距离
                material.SetFloat("_BlurSpread", 1 + i * blurSpread);

                //新的缓存区
                RenderTexture buffer1 = RenderTexture.GetTemporary(rtw, rtH, 0);

                //我们需要用两个Pass 处理图像两次
                //进行第一次卷积计算
                Graphics.Blit(buffer, buffer1, material, 2);
                //这时 关键内容都在buffer1中 buffer没用了 释放掉
                RenderTexture.ReleaseTemporary(buffer);

                buffer = buffer1;
                buffer1 = RenderTexture.GetTemporary(rtw, rtH, 0);
                //进行第二次卷积计算
                Graphics.Blit(buffer, buffer1, material, 3);
                //释放缓存区
                RenderTexture.ReleaseTemporary(buffer);
                //buffer与buffer1 指向的都是这一次高斯模糊的效果
                buffer = buffer1;
            }
            //把提取出来的内容进行模糊处理后存储在buffer中
            //将模糊处理后的亮度纹理和源纹理进行颜色叠加
            material.SetTexture("_Bloom", buffer);
            // 关键修复：彻底隔离读写，用双缓冲完全分离输入输出
            RenderTexture tempA = RenderTexture.GetTemporary(source.width, source.height, 0);
            tempA.filterMode = FilterMode.Bilinear;
            // 第一步：CRT+Bloom合成到tempA（输入CRTBloom，输出tempA，无冲突）
            material.SetPass(4);
            Graphics.Blit(CRTBloom, tempA, material);

            // 第二步：tempA输出到destination（输入tempA，输出destination，无冲突）
            Graphics.Blit(tempA, destination);

            // 释放所有临时纹理
            RenderTexture.ReleaseTemporary(tempA);
            RenderTexture.ReleaseTemporary(buffer);
            RenderTexture.ReleaseTemporary(CRTBloom);

        }
        else
        {
            // 如果没有材质，直接复制图像
            Graphics.Blit(source, destination);
        }

    }


}
