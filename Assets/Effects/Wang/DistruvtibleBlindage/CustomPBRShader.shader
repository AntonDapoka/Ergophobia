Shader "Unlit/CustomPBRShader"
{
    Properties
    {
        _Aldebo ("Albedo", 2D) = "white" {}
        _preFilterMap("PreFilterMap", Cube) = "skybox" {}
        _Normal ("Normal", 2D) = "bump" {}
        _NormalIntensity("NormalIntensity",Range(0, 2)) = 1
        _Roughness("Roughness", Range(0, 1)) = 0.5
        _Metalness("Metalness", Range(0, 1)) = 0.0

        _Noise("Noise",2D) = "white"{}      
        _Gradient("Gradient",2D) = "white"{}     
        _Dissolve("Dissolve",Range(0,1)) = 0   
        _EdgeRange("EdgeRange",Range(0,1)) = 0.1
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Geometry"
            "RenderType" = "Opaque"
        }

        CGINCLUDE
        #include "UnityCG.cginc"
        #include "AutoLight.cginc"
        #include "UnityPBSLighting.cginc"

        float _Roughness;
        float _Metalness;
        
        sampler2D _Noise;
        float4 _Noise_ST;
        sampler2D _Gradient;
        fixed _Dissolve;
        fixed _EdgeRange;

        #define TBN_WORLD_COORDS(idx1, idx2, idx3)\
        float4 tangentToWorld0 : TEXCOORD##idx1;\
        float4 tangentToWorld1 : TEXCOORD##idx2;\
        float4 tangentToWorld2 : TEXCOORD##idx3;

        #define TRANSFER_WORLD_POS(o, worldPos)\
        o.tangentToWorld0.w = worldPos.x;\
        o.tangentToWorld1.w = worldPos.y;\
        o.tangentToWorld2.w = worldPos.z;

        #define TRANSFER_TBN_ROTATION(o, worldTangent, worldBinormal, worldNormal)\
        o.tangentToWorld0.xyz = worldTangent;\
        o.tangentToWorld1.xyz = worldBinormal;\
        o.tangentToWorld2.xyz = worldNormal;

        inline float3 UnPackWorldPos(float4 T, float4 B, float4 N)
        {
            return float3(T.w, B.w, N.w);
        }

        
        struct v2f
        {
            float4 pos : SV_POSITION;
            float4 uv : TEXCOORD0;
            TBN_WORLD_COORDS(2, 3, 4)
            UNITY_LIGHTING_COORDS(5, 6)
            UNITY_FOG_COORDS(7)
            float2 uvNoise:TEXCOORD8;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        sampler2D _Aldebo;
        float4 _Aldebo_ST;
        sampler2D _Normal;
        float4 _Normal_ST;
        samplerCUBE _preFilterMap;
        float _NormalIntensity;

        
        inline float3 GetWorldNormal(v2f i)
        {
            float2 nUV = TRANSFORM_TEX(i.uv.xy, _Normal);
            half3 normalTangent = UnpackNormalWithScale(tex2D(_Normal, nUV), _NormalIntensity);
            
           
            return normalize(
                normalTangent.x * i.tangentToWorld0.xyz + 
                normalTangent.y * i.tangentToWorld1.xyz + 
                normalTangent.z * i.tangentToWorld2.xyz
            );
        }

        #define HALF_MIN 6.103515625e-5

        float TrowbridgeReitz_D(float NdotH, float roughness)
        {
            float a = roughness * roughness;
            float a2 = max(a * a, HALF_MIN);
            float d = (NdotH * a2 - NdotH) * NdotH + 1.0f;
            return a2 / (UNITY_PI * d * d); 
        }

        float GeometrySchlickGGX_G(float NdotV, float NdotL, float roughness)
        {
            
            float a = roughness;
            float k = (a + 1.0) * (a + 1.0) / 8.0;
            
            float ggx1 = NdotV / (NdotV * (1.0 - k) + k);
            float ggx2 = NdotL / (NdotL * (1.0 - k) + k);
            return ggx1 * ggx2;
        }

        float3 fresnelSchlick_F(float cosTheta, float3 F0)
        {
            return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
        }

        float2 LUT_Approx(float roughness, float NoV)
        {
            const float4 c0 = {-1, -0.0275, -0.572, 0.022};
            const float4 c1 = {1, 0.0425, 1.04, -0.04};
            float4 r = roughness * c0 + c1;
            float a004 = min(r.x * r.x, exp2(-9.28 * NoV)) * r.x + r.y;
            float2 AB = float2(-1.04, 1.04) * a004 + r.zw;
            return saturate(AB);
        }

       
        float3 CalculateDirectLighting(float3 albedo, float3 nDirWS, float3 vDirWS, 
                                      float roughness, float metalness, float3 lightDir, 
                                      float3 lightColor, float attenuation)
        {
           
            float3 halfDir = normalize(lightDir + vDirWS);
            float NDotV = max(dot(nDirWS, vDirWS), 0.0001);
            float NDotL = max(dot(nDirWS, lightDir), 0.0001);
            float NdotH = saturate(dot(nDirWS, halfDir));
            float HdotV = saturate(dot(halfDir, vDirWS));

            float3 F0_CONST = float3(0.04, 0.04, 0.04);
            float3 f0 = lerp(F0_CONST, albedo, metalness);

            float3 F = fresnelSchlick_F(HdotV, f0);
            float D = TrowbridgeReitz_D(NdotH, roughness);
            float G = GeometrySchlickGGX_G(NDotV, NDotL, roughness);

            float3 kS = F;
            float3 kD = (1 - kS) * (1 - metalness);

            float3 directDiffuse = kD * albedo * (1 / UNITY_PI);
           
            float3 cookTorranceSpecular = (D * F * G) / max(4.0 * NDotV * NDotL, HALF_MIN);
            float3 directSpecular = cookTorranceSpecular;

            return (directDiffuse + directSpecular) * lightColor * attenuation * NDotL;
        }

        v2f vert_pbr(appdata_full v)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_OUTPUT(v2f, o);
            o.pos = UnityObjectToClipPos(v.vertex);
            
            
            o.uv.xy = TRANSFORM_TEX(v.texcoord.xy, _Aldebo);
            #ifndef LIGHTMAP_OFF
            o.uv.zw = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
            #endif
            float3 posWs = mul(unity_ObjectToWorld, v.vertex).xyz;
            float3 normalWorld = UnityObjectToWorldNormal(v.normal);
            float3 tangentWorld = UnityObjectToWorldDir(v.tangent.xyz);
            float sign = v.tangent.w * unity_WorldTransformParams.w;
            float3 binormalWorld = cross(normalWorld, tangentWorld) * sign;

            o.uvNoise = TRANSFORM_TEX(v.texcoord.xy, _Noise);

            TRANSFER_WORLD_POS(o, posWs);
            TRANSFER_TBN_ROTATION(o, tangentWorld, binormalWorld, normalWorld);
            
            UNITY_TRANSFER_LIGHTING(o, v.texcoord1);
            UNITY_TRANSFER_FOG(o, o.pos);
            return o;
        }
        ENDCG

        
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_pbr
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile_instancing
            #pragma multi_compile_fwdbase
           
            float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                //dissolve clip
                fixed3 noiseColor = tex2D(_Noise, i.uvNoise).rgb;
                clip(_Dissolve >= 1.0 ? -1 : noiseColor.r - _Dissolve);
                
                // base color and material properties
                float4 albedo = tex2D(_Aldebo, i.uv);
                float roughness = _Roughness;
                float metalness = _Metalness;
                float3 posWS = UnPackWorldPos(i.tangentToWorld0, i.tangentToWorld1, i.tangentToWorld2);
                float3 vDirWS = normalize(UnityWorldSpaceViewDir(posWS));
                float3 nDirWS = GetWorldNormal(i);
                float3 reflectDir = reflect(-vDirWS, nDirWS);

                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0;
                float NDotV = max(dot(nDirWS, vDirWS), 0.0001);

                // directional light doesn't need attenuation, point and spot lights do
                UNITY_LIGHT_ATTENUATION(attenuation, i, posWS);
                float3 directLighting = CalculateDirectLighting(albedo.rgb, nDirWS, vDirWS, 
                                                               roughness, metalness, lightDir, 
                                                               lightColor, attenuation);

                // Ambient lighting (IBL)
                float3 F0 = lerp(float3(0.04, 0.04, 0.04), albedo.rgb, metalness);
                float3 F = fresnelSchlick_F(NDotV, F0);
                float3 oneMinusReflectivity = (1 - metalness) * (1 - F);
                
                #ifndef LIGHTMAP_OFF
                fixed4 lightMapTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uv.zw);
                fixed3 lightmap = DecodeLightmap(lightMapTex);
                float3 indirectDiffuse = lightmap * albedo.rgb * oneMinusReflectivity;
                #else
                float3 indirectDiffuse = ShadeSH9(float4(nDirWS, 1)) * albedo.rgb * oneMinusReflectivity;
                #endif

                const float MAX_REFLECTION_LOD = 4.0;
                float3 prefilterColor = texCUBElod(_preFilterMap, float4(reflectDir, roughness * MAX_REFLECTION_LOD)).rgb;
                float2 envBRDF = LUT_Approx(roughness, NDotV);
                float3 indirectSpecular = prefilterColor * (F * envBRDF.x + envBRDF.y);

                float3 indirectLighting = indirectDiffuse + indirectSpecular;
                float3 finalcol = directLighting + indirectLighting;
                UNITY_APPLY_FOG(i.fogCoord, finalcol);

                // Dissolve edge effect
                fixed edgeFactor = smoothstep(0, _EdgeRange, noiseColor.r - _Dissolve);
                fixed value = 1 - edgeFactor;
                fixed3 gradientColor = tex2D(_Gradient, i.uv).rgb; 
                fixed3 finalColor = lerp(finalcol, gradientColor, value * step(0.0001, _Dissolve));

                return float4(finalColor, 1.0);
            }
            ENDCG
        }

       
        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }
            Blend One One
            ZWrite Off
            ZTest LEqual 
            Fog { Color (0,0,0,0) }

            CGPROGRAM
            #pragma vertex vert_pbr
            #pragma fragment fragAdd
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma multi_compile_fwdadd_fullshadows
           
            float4 fragAdd(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
               
                fixed3 noiseColor = tex2D(_Noise, i.uvNoise).rgb;
                clip(_Dissolve >= 1.0 ? -1 : noiseColor.r - _Dissolve);
                
                // BaseSample
                float4 albedo = tex2D(_Aldebo, i.uv);
                float roughness = _Roughness;
                float metalness = _Metalness;
                float3 posWS = UnPackWorldPos(i.tangentToWorld0, i.tangentToWorld1, i.tangentToWorld2);
                float3 vDirWS = normalize(UnityWorldSpaceViewDir(posWS));
                float3 nDirWS = GetWorldNormal(i);

                // All Light Types diractions and colors are provided by AutoLight.cginc macros
                float3 lightDir = _WorldSpaceLightPos0.xyz - posWS * _WorldSpaceLightPos0.w;
                lightDir = normalize(lightDir);
                float3 lightColor = _LightColor0.rgb;

                // caculate attenuation for point and spot lights (attenuation is 1 for directional lights)
                UNITY_LIGHT_ATTENUATION(attenuation, i, posWS);

                //calculate direct
                float3 directLighting = CalculateDirectLighting(albedo.rgb, nDirWS, vDirWS, 
                                                               roughness, metalness, lightDir, 
                                                               lightColor, attenuation);

                UNITY_APPLY_FOG(i.fogCoord, directLighting);
                return float4(directLighting, 1.0);
            }
            ENDCG
        }

     
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            CGPROGRAM
            #pragma vertex vertShadow
            #pragma fragment fragShadow
            #pragma multi_compile_shadowcaster

            struct v2fShadow
            {
                V2F_SHADOW_CASTER;
                float2 uvNoise : TEXCOORD1;
            };

            v2fShadow vertShadow(appdata_full v)
            {
                v2fShadow o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                o.uvNoise = TRANSFORM_TEX(v.texcoord.xy, _Noise);
                return o;
            }

            float4 fragShadow(v2fShadow i) : SV_Target
            {
               
                fixed3 noiseColor = tex2D(_Noise, i.uvNoise).rgb;
                clip(_Dissolve >= 1.0 ? -1 : noiseColor.r - _Dissolve);
                
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    
}