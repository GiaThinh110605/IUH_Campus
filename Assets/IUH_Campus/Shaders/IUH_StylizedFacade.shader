Shader "IUH/Stylized Facade"
{
    Properties
    {
        [KeywordEnum(Textured, Plain, Brick, Glass)] _Facade ("Surface", Float) = 0
        _BaseColor ("Wall Color", Color) = (0.8,0.76,0.65,1)
        _BaseMap ("Original UV / Lettering", 2D) = "white" {}
        _WindowCell ("Window Cell (m)", Vector) = (1.6,2.4,0,0)
        _WarmColor ("Window Light", Color) = (0.75,0.48,0.2,1)
        _NoiseStrength ("Noise Strength", Range(0,0.06)) = 0.025
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor, _WarmColor;
            float4 _BaseMap_ST, _WindowCell;
            float _Facade, _NoiseStrength;
        CBUFFER_END
        float _IUH_NightAmount;

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float2 uv : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            half3 normalWS : TEXCOORD1;
            half fog : TEXCOORD2;
            float2 uv : TEXCOORD4;
            #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
            float4 screenShadow : TEXCOORD3;
            #endif
            UNITY_VERTEX_OUTPUT_STEREO
        };

        Varyings GroundVertex(Attributes input)
        {
            UNITY_SETUP_INSTANCE_ID(input);
            Varyings output = (Varyings)0;
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
            output.uv = input.uv * _BaseMap_ST.xy + _BaseMap_ST.zw;
            output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
            output.positionCS = TransformWorldToHClip(output.positionWS);
            output.normalWS = TransformObjectToWorldNormal(input.normalOS);
            float depth = -TransformWorldToView(output.positionWS).z;
            output.fog = unity_OrthoParams.w > 0.5
                ? ComputeFogFactorZ0ToFar(max(0, depth - _ProjectionParams.y))
                : ComputeFogFactor(output.positionCS.z);
            #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
            output.screenShadow = ComputeScreenPos(output.positionCS);
            #endif
            return output;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex GroundVertex
            #pragma fragment GroundFragment
            #pragma shader_feature_local_fragment _FACADE_TEXTURED _FACADE_PLAIN _FACADE_BRICK _FACADE_GLASS
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float HashGround(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float ValueNoise(float2 p)
            {
                float2 cell = floor(p);
                float2 blend = smoothstep(0, 1, frac(p));
                return lerp(lerp(HashGround(cell), HashGround(cell + float2(1, 0)), blend.x),
                    lerp(HashGround(cell + float2(0, 1)), HashGround(cell + 1), blend.x), blend.y);
            }

            float2 FacadeProjection(float3 worldPos, half3 worldNormal)
            {
                half3 weights = abs(worldNormal);
                if (weights.y > max(weights.x, weights.z)) return worldPos.xz;
                return weights.x > weights.z ? worldPos.zy : worldPos.xy;
            }

            half3 FacadeColor(Varyings input, half3 worldNormal)
            {
                half3 wall = _BaseColor.rgb;
                #if defined(_FACADE_TEXTURED)
                wall *= SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb;
                #else
                float grain = ValueNoise(float2(input.positionWS.x + input.positionWS.y * .37, input.positionWS.z) * .23);
                wall *= 1 + (grain * 2 - 1) * min(_NoiseStrength, .06);
                #endif
                #if defined(_FACADE_BRICK)
                float2 brickUV = FacadeProjection(input.positionWS, worldNormal) / float2(.42,.19);
                brickUV.x += fmod(abs(floor(brickUV.y)),2) * .5;
                float2 brickEdge = min(frac(brickUV),1-frac(brickUV));
                float jointDist = min(brickEdge.x * .42, brickEdge.y * .19);
                float aa = max(fwidth(jointDist),.001);
                wall = lerp(wall * .63, wall, smoothstep(.008-aa,.008+aa,jointDist));
                #endif
                #if defined(_FACADE_GLASS)
                wall = lerp(half3(.055,.13,.18), half3(.30,.53,.64), saturate(input.positionWS.y / 28));
                #endif
                return wall * lerp(.90,1,saturate(input.positionWS.y / 1.5));
            }

            half4 GroundFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half3 normal = NormalizeNormalPerPixel(input.normalWS);
                #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                Light light = GetMainLight(input.screenShadow);
                #else
                Light light = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                #endif
                half diffuse = saturate(dot(normal, light.direction));
                half3 illumination = SampleSH(normal) + light.color * (0.16h + 0.84h * diffuse) *
                    light.shadowAttenuation * light.distanceAttenuation;
                half3 result = FacadeColor(input, normal) * illumination;
                #if defined(_FACADE_GLASS)
                float2 windowUV = FacadeProjection(input.positionWS, normal) / max(_WindowCell.xy,.1);
                float enabledWindow = step(.46,HashGround(floor(windowUV)));
                result = lerp(result, result * .28 + _WarmColor.rgb * enabledWindow, saturate(_IUH_NightAmount));
                #endif
                return half4(MixFog(result, input.fog), 1);
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
            #pragma vertex ShadowVertex
            #pragma fragment WriteShadow
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            float3 _LightDirection, _LightPosition;

            float4 ShadowVertex(Attributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float3 position = TransformObjectToWorld(input.positionOS.xyz);
                float3 direction = _LightDirection;
                #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
                direction = normalize(_LightPosition - position);
                #endif
                float3 normal = TransformObjectToWorldNormal(input.normalOS);
                return ApplyShadowClamping(TransformWorldToHClip(ApplyShadowBias(position, normal, direction)));
            }

            half4 WriteShadow() : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R
            HLSLPROGRAM
            #pragma vertex GroundVertex
            #pragma fragment WriteDepth
            #pragma multi_compile_instancing
            half WriteDepth(Varyings input) : SV_Target { return input.positionCS.z; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On
            HLSLPROGRAM
            #pragma vertex GroundVertex
            #pragma fragment WriteNormals
            #pragma multi_compile_instancing
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

            half4 WriteNormals(Varyings input) : SV_Target
            {
                half3 normal = NormalizeNormalPerPixel(input.normalWS);
                #if defined(_GBUFFER_NORMALS_OCT)
                float2 oct = PackNormalOctQuadEncode(normal);
                return half4(PackFloat2To888(saturate(oct * 0.5 + 0.5)), 0);
                #else
                return half4(normal, 0);
                #endif
            }
            ENDHLSL
        }
    }
}
