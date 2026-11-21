Shader "IUH/Stylized Ground"
{
    Properties
    {
        [KeywordEnum(Grass, Paving)] _Ground ("Surface", Float) = 0
        _ColorA ("Base", Color) = (0.28, 0.40, 0.14, 1)
        _ColorB ("Variation", Color) = (0.31, 0.43, 0.16, 1)
        _ColorC ("Highlights", Color) = (0.34, 0.46, 0.18, 1)
        _TileSize ("Paver Size in Metres", Vector) = (1.12, 0.72, 0, 0)
        _GroutColor ("Grout", Color) = (0.44, 0.43, 0.37, 1)
        _GroutWidth ("Joint Width in Metres", Range(0.006, 0.06)) = 0.025
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _ColorA, _ColorB, _ColorC, _GroutColor;
            float4 _TileSize;
            float _GroutWidth, _Ground;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            half3 normalWS : TEXCOORD1;
            half fog : TEXCOORD2;
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
            #pragma shader_feature_local_fragment _GROUND_GRASS _GROUND_PAVING
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH

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

            half3 GroundColor(float2 metres)
            {
                #if defined(_GROUND_PAVING)
                float2 tileSize = max(_TileSize.xy, 0.05);
                float2 tile = metres / tileSize;
                tile.x += fmod(abs(floor(tile.y)), 2) * 0.5;
                float2 local = frac(tile);
                float2 edge = min(local, 1 - local) * tileSize;
                float line = min(edge.x, edge.y);
                float aa = max(fwidth(line), 0.001);
                float joint = smoothstep(_GroutWidth * 0.5 - aa, _GroutWidth * 0.5 + aa, line);
                float variation = HashGround(floor(tile));
                half3 stone = lerp(_ColorA.rgb, _ColorB.rgb, variation);
                float grain = ValueNoise(metres * 15);
                stone *= lerp(0.96, 1.04, grain);
                return lerp(_GroutColor.rgb, stone, joint);
                #else
                float field = ValueNoise(metres / 11);
                float tuft = ValueNoise(metres / 1.45);
                half3 baseGrass = lerp(_ColorA.rgb, _ColorB.rgb, smoothstep(0.15, 0.85, field));
                return lerp(baseGrass, _ColorC.rgb, smoothstep(0.48, 0.90, tuft) * 0.42);
                #endif
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
                return half4(MixFog(GroundColor(input.positionWS.xz) * illumination, input.fog), 1);
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
