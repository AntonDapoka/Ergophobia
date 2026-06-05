Shader "Unlit/RadialBiur"
{
    Properties
    {
        _BlurAmount("Blur Amount", Range(0, 1)) = 0.5
        _SampleSteps("Sample Steps", Range(1, 30)) = 5
    }
    SubShader
    {
         Tags{"RenderType" = "Opaque" "Queue" = "Transparent"}
        LOD 100

        GrabPass{"_MyGrabTexture"}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
         
            #include "UnityCG.cginc"

             sampler2D _MyGrabTexture;
                float _BlurAmount;
                int _SampleSteps;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 grabPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.vertex);     
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
               float2 center = (0.5,0.5);
               float2 delta = (i.grabPos.xy/i.grabPos.w) - center;
               float4 col = tex2D(_MyGrabTexture, i.grabPos.xy / i.grabPos.w);
               float4 sum = col;
               float2 offest = delta  * _BlurAmount;
               for (int j = 1; j <= _SampleSteps; j++)
               {
                   float2 off = offest * j;
                   sum += tex2D(_MyGrabTexture, (i.grabPos.xy / i.grabPos.w)+off);
                   sum += tex2D(_MyGrabTexture, (i.grabPos.xy / i.grabPos.w)-off);

               }

               
                return float4((sum / (1 + 2 * _SampleSteps)).rgb, 1);
            }
            ENDCG
        }
    }
}
