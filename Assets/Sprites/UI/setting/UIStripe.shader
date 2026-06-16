Shader "Unlit/UIStripe"
{
    Properties
    {
       _XrayTex("Xray Texture", 2D) = "white" {}
       _XrayColor("XrayColor", Color) = (1,1,1,1)
       _XrayScale("Xray Scale", Range(0,100)) = 0

    }
    SubShader
    {
         // 标签：必须设置这四个
    Tags { 
        "Queue"="Transparent"        // UI必须在透明队列渲染
        "IgnoreProjector"="True"     // 忽略投影器影响
        "RenderType"="Transparent"   // 渲染类型为透明
        "PreviewType"="Plane"        // 预览为平面
    }
        LOD 100

          Pass
        {
        Name "Xray"
   
    
    // 以下状态一个都不能错
    Cull Off          // 关闭背面剔除（UI正反面都要显示）
    Lighting Off      // 关闭光照计算（UI不需要光照）
    ZWrite Off        // 关闭深度写入（保证UI按层级顺序渲染）
    ZTest[unity_GUIZTestMode]  // 使用UI系统的深度测试模式
    Blend One OneMinusSrcAlpha // 标准Alpha混合模式
    
       
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

           
            struct v2f
            {
               
                float4 vertex : SV_POSITION;
                float4 scrPos : TEXCOORD0;
            };

          
            sampler2D _XrayTex;
            float _XrayScale;
            fixed4 _XrayColor;

            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.scrPos = ComputeScreenPos(o.vertex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
               float2 screenUV = i.scrPos.xy / i.scrPos.w;//turen screenUV to 0-1 range UV coordinates
               screenUV.y += _Time.y * 0.1;//animate the texture by scrolling it vertically over time
               fixed4 Color = tex2D(_XrayTex, screenUV * _XrayScale);
                 if(Color.r >= 0.1)
                   clip(-1);//discard the pixel
                return _XrayColor;
                
            }
        ENDCG
        }
    }
}
