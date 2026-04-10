Shader "Unlit/CRTBloom"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Bend ("Bend", Float) = 0.1
        _Scanline ("Scanline", Float) = 0.5
        _ScanlineSpeed ("Scanline Speed", Float) = 0.02
        _Chromatic ("Chromatic Aberration", Float) = 0.02
        _Noise ("Noise", Float) = 0.1
        _Vignette ("Vignette", Float) = 0.5
        _Brightness ("Brightness", Float) = 1.0



         //用于存储亮度纹理模糊后的结果
        _Bloom("Bloom",2D) = ""{}
        //亮度阈值 控制亮度纹理 亮度区域的
        _LuminanceThreshold("LuminanceThreshold",Float) = 0.5
        //模糊半径
        _BlurSize("BlurSize",Float) = 1

    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        LOD 100
        ZTest Always Cull Off ZWrite Off

       //crt的pass
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
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Bend;
            float _Scanline;
            float _ScanlineSpeed;
            float _Chromatic;
            float _Noise;
            float _Vignette;
            float _Brightness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // 屏幕弯曲
            float2 CurveUV(float2 uv)
            {
                float2 center = uv - 0.5;
                float dist = dot(center, center);
                uv += center * dist * _Bend;
                return uv;
            }

            // 扫描线
            fixed Scanline(float2 uv)
            {
                float s = sin(uv.y * 600 + _Time.y * _ScanlineSpeed * 100);
                return 1 + s * _Scanline;
            }

            // 噪点
            fixed Noise(float2 uv)
            {
                float n = frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
                return n * _Noise;
            }

            // 暗角
            fixed Vignette(float2 uv)
            {
                float2 center = uv - 0.5;
                float v = 1 - dot(center, center) * 10 * _Vignette;
                return v;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = CurveUV(i.uv);

                // 超出范围返回黑
                if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
                    return fixed4(0,0,0,1);

                // RGB 分离
                fixed4 colR = tex2D(_MainTex, uv + float2(_Chromatic, 0));
                fixed4 colG = tex2D(_MainTex, uv);
                fixed4 colB = tex2D(_MainTex, uv - float2(_Chromatic, 0));
                fixed4 col = fixed4(colR.r, colG.g, colB.b, 1);

                // 效果叠加
                col.rgb *= Scanline(uv);
                col.rgb += Noise(uv);
                col.rgb *= Vignette(uv);
                col.rgb *= _Brightness;

                return col;
            }
            ENDCG
        }

      
        //提取的pass
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
              #include "UnityCG.cginc"
 
              struct v2f
               {
                        float2 uv : TEXCOORD0;              
                        float4 vertex : SV_POSITION;
               };

               fixed Luminance(fixed4 color)
               {
                   return 0.2125*color.r + 0.7154*color.g + 0.0721*color.b;
               }
           
             v2f vert(appdata_base v)
             {
                 v2f o;
                 o.vertex = UnityObjectToClipPos(v.vertex);
                 o.uv = v.texcoord;
                 return o;
             }

             sampler2D _MainTex;
             float _LuminanceThreshold;


             fixed4 frag(v2f i):SV_Target
             {
                 //采样原文里颜色
                 fixed4 color = tex2D(_MainTex,i.uv);
                 //得到亮度贡献值
                 fixed value = clamp(Luminance(color) - _LuminanceThreshold, 0 , 1);
                 //返回亮度*亮度贡献值
                 return color*value;
             }
             ENDCG
       }

        //复用高斯模糊的两个Pass
       //Horizontal
        pass
        {
             CGPROGRAM
             #pragma vertex vertBlurHorizontal
            #pragma fragment fragBlur
        #include "UnityCG.cginc"
        sampler2D _MainTex;
        //纹素  x = 1/宽  y = 1/高
        half4 _MainTex_TexelSize;
        //纹理偏移间隔单位
        float _BlurSpread;

         struct v2f
         {
             //五个像素的uv坐标偏移
                half2 uv[5] : TEXCOORD0; 
             //顶点再裁剪空间下坐标
                float4 vertex : SV_POSITION;
         };

         //水平方向的 顶点着色器函数
         v2f vertBlurHorizontal(appdata_base v)
         {
             v2f o;
             o.vertex = UnityObjectToClipPos(v.vertex);

             //5个像素的uv偏移了
             half2 uv = v.texcoord;

             //去进行5个像素 水平位置的偏移获取
             o.uv[0] = uv;
             o.uv[1] = uv + half2(_MainTex_TexelSize.x *1 ,0)*_BlurSpread;
             o.uv[2] = uv - half2(_MainTex_TexelSize.x *1 ,0)*_BlurSpread;
             o.uv[3] = uv + half2(_MainTex_TexelSize.x*2,0)*_BlurSpread;
             o.uv[4] = uv - half2(_MainTex_TexelSize.x*2,0)*_BlurSpread; 
             
             return o;
         }

         //片元着色器函数
         //两个Pass可以使用同一个 我们把里面的逻辑写的通用即可
         fixed4 fragBlur(v2f i):SV_Target
         {
             //卷积运算
             //卷积核 其中三个 因为只有这三个数 没有必要申明为5个单位的卷积和
             float weight[3] = {0.4026,0.2442,0.0545};
             //先计算当前像素点
             fixed3 sum = tex2D(_MainTex,i.uv[0]).rgb * weight[0];

             //去计算左右偏移一个单位的 和 左右偏移两个单位的 对位相乘 累加
            for (int it = 1; it < 3; it++)
            {
                
                //要和右元素相乘
                sum += tex2D(_MainTex,i.uv[it*2 - 1]).rgb * weight[it];
                //和左元素相乘
                sum += tex2D(_MainTex,i.uv[it*2]).rgb * weight[it];
            }

            return fixed4(sum,1);

         }
            ENDCG
        }
        //vertical
        pass
        {
             CGPROGRAM
              #pragma vertex vertBlurVertical
            #pragma fragment fragBlur
        #include "UnityCG.cginc"
        sampler2D _MainTex;
        //纹素  x = 1/宽  y = 1/高
        half4 _MainTex_TexelSize;
        //纹理偏移间隔单位
        float _BlurSpread;

         struct v2f
         {
             //五个像素的uv坐标偏移
                half2 uv[5] : TEXCOORD0; 
             //顶点再裁剪空间下坐标
                float4 vertex : SV_POSITION;
         };

        

         //垂直方向的 顶点着色器函数
         v2f vertBlurVertical(appdata_base v)
         {
             v2f o;
             o.vertex = UnityObjectToClipPos(v.vertex);

             //5个像素的uv偏移了
             half2 uv = v.texcoord;

             //去进行5个像素 水平位置的偏移获取
             o.uv[0] = uv;
             o.uv[1] = uv + half2(0,_MainTex_TexelSize.x *1)*_BlurSpread;
             o.uv[2] = uv - half2(0,_MainTex_TexelSize.x *1 )*_BlurSpread;
             o.uv[3] = uv + half2(0,_MainTex_TexelSize.x*2)*_BlurSpread;
             o.uv[4] = uv - half2(0,_MainTex_TexelSize.x*2)*_BlurSpread; 
             
             return o;
         }

         //片元着色器函数
         //两个Pass可以使用同一个 我们把里面的逻辑写的通用即可
         fixed4 fragBlur(v2f i):SV_Target
         {
             //卷积运算
             //卷积核 其中三个 因为只有这三个数 没有必要申明为5个单位的卷积和
             float weight[3] = {0.4026,0.2442,0.0545};
             //先计算当前像素点
             fixed3 sum = tex2D(_MainTex,i.uv[0]).rgb * weight[0];

             //去计算左右偏移一个单位的 和 左右偏移两个单位的 对位相乘 累加
            for (int it = 1; it < 3; it++)
            {
                
                //要和右元素相乘
                sum += tex2D(_MainTex,i.uv[it*2 - 1]).rgb * weight[it];
                //和左元素相乘
                sum += tex2D(_MainTex,i.uv[it*2]).rgb * weight[it];
            }

            return fixed4(sum,1);
        }
            ENDCG
        }

          //用于合成的Pass
        pass
        {
            CGPROGRAM
             #pragma vertex vertBloom
            #pragma fragment fragBloom
            #pragma multi_compile __ UNITY_UV_STARTS_AT_TOP
             #include "UnityCG.cginc"
             sampler2D _MainTex;
             float4 _MainTex_TexelSize;
             sampler2D _Bloom;

            struct v2fBloom
            {
                float4 pos:SV_POSITION;
                //xy主要用于对主纹理进行采样
                //zw主要用于对模糊后的亮度纹理进行采样
                half4 uv:TEXCOORD0;
            };

            v2fBloom vertBloom(appdata_base v)
            {
                v2fBloom o;
                o.pos = UnityObjectToClipPos(v.vertex);
                //亮度纹理和主纹理要 采样相同的坐标的纹理叠加
                o.uv.xy = v.texcoord;
                o.uv.zw = v.texcoord;

                //用宏去判断uv坐标是否被反转
                #if UNITY_UV_STARTS_AT_TOP
                //如果文素的y<0 就说明被反转了 需要进行调整
                if (_MainTex_TexelSize.y < 0)
                    o.uv.w = 1 - o.uv.w;
                
                #endif
                  

                return o;
            }
            fixed4 fragBloom(v2fBloom i):SV_Target
            {
                //采样主纹理和亮度纹理
                fixed4 color = tex2D(_MainTex,i.uv.xy);
                fixed4 bloom = tex2D(_Bloom,i.uv.zw);
                //直接把两者相加进行叠加
                return color + bloom;
            }


            ENDCG
        }
          
    }
    FallBack "Unlit/Texture"
}