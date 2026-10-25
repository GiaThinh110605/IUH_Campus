Shader "IUH/Concept Fountain Water"
{
    Properties { _BaseColor("Water",Color)=(.12,.53,.65,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct A{float4 positionOS:POSITION;};
            struct V{float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;};
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            CBUFFER_END
            V vert(A i){V o;o.world=TransformObjectToWorld(i.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);return o;}
            float4 frag(V i):SV_Target
            {
                float2 p=i.world.xz;float t=_Time.y;
                float ripple=sin(p.x*5+p.y*3+t*.8)*sin(p.y*6-p.x*2-t*.7);
                float caustic=pow(saturate(ripple),12);
                float3 normal=normalize(float3(cos(p.x*4+t)*.08,1,sin(p.y*5-t)*.08));
                float3 view=GetWorldSpaceNormalizeViewDir(i.world);
                float fresnel=pow(1-saturate(dot(view,normal)),4);
                float3 reflection=SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0,samplerunity_SpecCube0,reflect(-view,normal),1).rgb;
                Light sun=GetMainLight();float spec=pow(saturate(dot(reflect(-sun.direction,normal),view)),180);
                float3 color=lerp(_BaseColor.rgb*(.90+ripple*.10)+caustic*.12,reflection,.22+fresnel*.45)+sun.color*spec*.7;
                return float4(color,1);
            }
            ENDHLSL
        }
    }
}
