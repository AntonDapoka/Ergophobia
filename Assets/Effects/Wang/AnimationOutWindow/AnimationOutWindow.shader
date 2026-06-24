Shader "Unlit/AnimationOutWindow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        //水平和垂直滚动速度
        _ScrollSpeedU("Scroll Speed U", float) = 0.5
        _ScrollSpeedV("Scroll Speed V", float) = 0.5

        _SampleRange("Sample Range", Range(0,1)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" "IgnoreProjector"="True"}
       

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
            float _ScrollSpeedU;
            float _ScrollSpeedV;
            float _SampleRange;
            

            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //利用时间 来计算UV的偏移
               float2 scrollUV = frac(i.uv + float2(_Time.y * _ScrollSpeedU, _Time.y * _ScrollSpeedV));
               scrollUV.x *= _SampleRange;
               return tex2D(_MainTex, scrollUV);
            }
            ENDCG
        }
    }
}
