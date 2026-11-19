Shader "IUH/Stylized Foliage"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _ShadeColor ("Shade Color", Color) = (0.48, 0.63, 0.51, 1)
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
        _RampMin ("Dark Edge", Range(0, 1)) = 0.36
        _RampMax ("Light Edge", Range(0, 1)) = 0.72
        [Toggle(_WIND_ON)] _Wind ("Wind", Float) = 0
        [Toggle] _UseVertexWind ("Use Vertex Color R for Wind", Float) = 0
        _WindSpeed ("Wind Speed", Float) = 1.5
        _WindStrength ("Wind Strength", Float) = 0.07
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "TransparentCutout" "Queue" = "AlphaTest" }
        Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _ShadeColor;
            float _Cutoff;
            float _RampMin;
            float _RampMax;
            float _Wind;
            float _UseVertexWind;
            float _WindSpeed;
            float _WindStrength;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float2 uv : TEXCOORD0;
            half4 color : COLOR;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        float3 FoliagePositionWS(Attributes input)
        {
            float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
            #if defined(_WIND_ON)
            float weight = lerp(saturate(input.positionOS.y), input.color.r, _UseVertexWind);
            float phase = _Time.y * _WindSpeed + positionWS.x * 0.3 + positionWS.z * 0.2;
            positionWS.xz += float2(1, 0.43) * (sin(phase) * _WindStrength * weight);
            #endif
            return positionWS;
        }

        half4 SampleCutout(float2 uv)
        {
            half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv) * _BaseColor;
            clip(color.a - _Cutoff);
            return color;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma shader_feature_local_vertex _WIND_ON

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                half fog : TEXCOORD3;
                #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                float4 screenShadow : TEXCOORD4;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                Varyings output = (Varyings)0;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionWS = FoliagePositionWS(input);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                float viewDepth = -TransformWorldToView(output.positionWS).z;
                output.fog = unity_OrthoParams.w > 0.5
                    ? ComputeFogFactorZ0ToFar(max(0, viewDepth - _ProjectionParams.y))
                    : ComputeFogFactor(output.positionCS.z);
                #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                output.screenShadow = ComputeScreenPos(output.positionCS);
                #endif
                return output;
            }

            half4 Frag(Varyings input, FRONT_FACE_TYPE face : FRONT_FACE_SEMANTIC) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half4 albedo = SampleCutout(input.uv);
                half3 normal = NormalizeNormalPerPixel(input.normalWS) * IS_FRONT_VFACE(face, 1, -1);
                #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                Light light = GetMainLight(input.screenShadow);
                #else
                Light light = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                #endif
                half ndl = dot(normal, light.direction) * 0.5h + 0.5h;
                half ramp = smoothstep(_RampMin, max(_RampMax, _RampMin + 0.001), ndl);
                half3 direct = lerp(_ShadeColor.rgb, half3(1, 1, 1), ramp);
                half3 lighting = SampleSH(normal) + direct * light.color *
                    light.distanceAttenuation * light.shadowAttenuation;
                return half4(MixFog(albedo.rgb * lighting, input.fog), albedo.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ColorMask 0
            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma shader_feature_local_vertex _WIND_ON

            float3 _LightDirection;
            float3 _LightPosition;

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            ShadowVaryings ShadowVert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                ShadowVaryings output;
                float3 positionWS = FoliagePositionWS(input);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float3 lightDirection = _LightDirection;
                #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
                lightDirection = normalize(_LightPosition - positionWS);
                #endif
                normalWS *= dot(normalWS, lightDirection) < 0 ? -1 : 1;
                output.positionCS = ApplyShadowClamping(TransformWorldToHClip(
                    ApplyShadowBias(positionWS, normalWS, lightDirection)));
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 ShadowFrag(ShadowVaryings input) : SV_Target
            {
                SampleCutout(input.uv);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R
            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing
            #pragma shader_feature_local_vertex _WIND_ON

            struct DepthVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            DepthVaryings DepthVert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                DepthVaryings output = (DepthVaryings)0;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformWorldToHClip(FoliagePositionWS(input));
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 DepthFrag(DepthVaryings input) : SV_Target
            {
                SampleCutout(input.uv);
                return input.positionCS.z;
            }
            ENDHLSL
        }
    }
}
