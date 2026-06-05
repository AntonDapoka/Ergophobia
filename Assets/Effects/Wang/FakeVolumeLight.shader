Shader "Unlit/FakeVolumeLight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Shade("Shade",2D) = "white" {}
        _AlphaIntensity("AlphaIntensity", Range(0,1)) = 1
        _Color("Color", Color) = (1,1,1,1)
        //used to control offset intensity of vertical
        _VerticalBillboarding("VerticalBillboarding", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" "DisableBatching"="True" }

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _VerticalBillboarding;

            sampler2D _Shade;
            float _AlphaIntensity;

            v2f vert (appdata_base v)
            {
                v2f o;
                //新坐标系的中心点（默认我们还是使用的模型空间原定）
                float3 center = float3(0,0,0);
                //计算Z轴（normal）
                float3 cameraInObjectPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1));
                //得到Z轴对应的向量
                float3 normalDir = cameraInObjectPos - center;
                //相当于把y往下压，如果_VerticalBillboarding是0 就代表把我们Z轴压到了xz平面 如果是1 那么就是正常的视角方向
                normalDir.y *= _VerticalBillboarding;
                //单位化Z轴
                normalDir = normalize(normalDir);
                //模型空间下的Y轴正方向 作为它的 old up
                //为了避免z轴和010重合 ，因为重合后再计算叉乘 可能会得到0向量
                float3 upDir = normalDir.y > 0.999 ? float3(0,0,1) : float3(0,1,0);
                //利用叉乘计算X轴（right）
                float3 rightDir = normalize(cross(upDir, normalDir));
                //去计算我们的Y轴 也就是newup
                upDir = normalize(cross(normalDir, rightDir));

                //得到顶点相对于新坐标系中心点的偏移位置
                float3 centerOffset = v.vertex.xyz - center;
                //利用3个轴向进行最终的顶点位置的计算
                float3 newVertexPos = center + rightDir * centerOffset.x + upDir * centerOffset.y + normalDir * centerOffset.z;
                //把新顶点转换到裁剪空间
                o.vertex = UnityObjectToClipPos(float4(newVertexPos, 1));

                //uv坐标偏移缩放
                o.uv = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //float2 uv = i.uv+_Time.y;
                float4 color = tex2D(_MainTex, float2(i.uv.x+_Time.y*0.2, i.uv.y));
                color.rgb *= _Color.rgb;
                color.a *= _AlphaIntensity * tex2D(_Shade, i.uv).a;
                return color;
            }
            ENDCG
        }
    }
}
