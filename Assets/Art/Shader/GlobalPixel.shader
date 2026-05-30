Shader "Unlit/GlobalPixel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PixelRange("PixelSize", float) = 64
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
       
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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _PixelRange;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
             float2 uv = i.uv;
             uv *= _PixelRange;
             uv.xy = floor(uv.xy);
             uv.xy /= _PixelRange;


                fixed4 col = tex2D(_MainTex, uv);
              
                return col;
            }
            ENDCG
        }
    }
}
