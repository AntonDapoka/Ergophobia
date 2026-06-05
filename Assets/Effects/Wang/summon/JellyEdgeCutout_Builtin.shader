Shader "Custom/JellyEdgeCutout_Builtin"
{
    Properties
    {
        _MainTex ("_MainTex", 2D) = "white" {}
        _NoiseTex ("_NoiseTex ", 2D) = "gray" {}
        _BaseCutoff ("_BaseCutoff", Range(0,1)) = 0.5
        _EdgeRange (" _EdgeRange", Range(0,0.4)) = 0.15
        _NoiseScale ("_NoiseScale", Float) = 4
        _NoiseSpeed ("_NoiseSpeed", Float) = 0.8
        _DistortIntensity ("_DistortIntensity", Range(0,0.3)) = 0.12
    }

    SubShader
    {
        Tags 
        {
            "RenderType"="TransparentCutout"
            "Queue"="AlphaTest"
            "IgnoreProjector"="True"
        }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float2 noiseUV : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;

            float _BaseCutoff;
            float _EdgeRange;
            float _NoiseScale;
            float _NoiseSpeed;
            float _DistortIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // 噪声UV时间流动
                float2 noiseOffset = _Time.y * float2(0.2, 0.3) * _NoiseSpeed;
              
                // 手动计算噪声UV，避免TRANSFORM_TEX的宏问题
                float2 scaledUV = v.uv * _NoiseScale;
                o.noiseUV = scaledUV + noiseOffset;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 mainCol = tex2D(_MainTex, i.uv);

                // 计算圆形边缘权重，仅边缘产生扰动
                float2 centerUV = i.uv - 0.5;
                float radius = length(centerUV);
                float edgeMask = smoothstep(0.5 - _EdgeRange, 0.5, radius);

                // 采样噪声做动态轮廓偏移
                float noiseVal = tex2D(_NoiseTex, i.noiseUV).r;
                float dynamicOffset = noiseVal * edgeMask * _DistortIntensity;
                float realCutoff = _BaseCutoff + dynamicOffset;

                // Alpha剔除，塑造晃动果冻边缘
                clip(mainCol.a - realCutoff);

                return mainCol;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Cutout/Soft Edge Unlit"
}