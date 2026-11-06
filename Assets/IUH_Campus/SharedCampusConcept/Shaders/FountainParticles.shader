Shader "IUH/Fountain Particles"
{
    Properties { _Ring("Ring", Float) = 0 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A { float4 positionOS:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
            struct V { float4 positionCS:SV_POSITION; half4 color:COLOR; float2 uv:TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
            float _Ring;
            CBUFFER_END
            V vert(A i) { V o; o.positionCS=TransformObjectToHClip(i.positionOS.xyz); o.color=i.color; o.uv=i.uv; return o; }
            half4 frag(V i):SV_Target
            {
                float radius=length(i.uv*2-1);
                float disc=saturate(1-radius*radius);
                float ring=saturate(1-abs(radius-.78)*14);
                return half4(i.color.rgb, i.color.a*lerp(disc*disc,ring,_Ring));
            }
            ENDHLSL
        }
    }
}
