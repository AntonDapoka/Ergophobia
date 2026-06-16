Shader "Unlit/InteractiveUI_Pattern"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TwinkleSpeed ("Twinkle Speed", Range(0,10)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        Pass
        {
            Cull Off
            Lighting Off
            ZWrite Off
            ZTest [unity_GUIZTestMode]
            Blend One OneMinusSrcAlpha

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
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _TwinkleSpeed;

            float GetPatternValue(int index)
            {
                const float pattern[24] =
                {
                    1,1,1,1,1,1,1,1,
                    1,1,1,1,1,1,0,1,
                    0,1,0,1,0,1,1,1
                };

                return pattern[index];
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                int patternLength = 24;

                float t = frac(_Time.y * _TwinkleSpeed);
                int index = (int)(t * patternLength);

                float brightness = GetPatternValue(index);

                fixed4 col = tex2D(_MainTex, i.uv);

                clip(col.a - 0.01);

                col.rgb *= brightness;

                return col;
            }
            ENDCG
        }
    }
}