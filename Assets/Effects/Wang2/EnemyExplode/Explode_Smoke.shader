Shader "Unlit/Explode_Smoke"
{
     Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        //菲涅尔角度垂直角度反射率
        _FresnelScale("FresnelScale", Float) = 1
        //菲涅尔角度水平角度反射率 N次方
        _FresnelN("FresnelN", Float) = 1
        //自定义颜色
        _Color("FresnelColor", Color) = (1,1,1,1)
        _CenterColor("CenterColor", Color) = (1,1,1,1)
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

           
            struct v2f
            {
               
                float4 vertex : SV_POSITION;
                //世界空间 视角方向
                float3 worldViewDir:TEXCOORD0;
                //世界空间 法线
                float3 worldNormal:TEXCOORD1;
            };

          
            fixed _FresnelScale;
            fixed _FresnelN;
            fixed4 _Color;
            fixed4 _CenterColor;

            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //视角方向 法线
                o.worldViewDir = normalize(WorldSpaceViewDir(v.vertex));
                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //菲涅尔反射
                fixed alpha = _FresnelScale + 
                            (1- _FresnelScale)
                            *pow( 1- dot(normalize(i.worldViewDir),normalize( i.worldNormal)),_FresnelN);

                return fixed4(lerp(_CenterColor.rgb, _Color.rgb, alpha), alpha);
                
            }
            ENDCG
        }
      
    }
}
