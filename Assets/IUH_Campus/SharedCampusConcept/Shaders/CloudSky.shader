Shader "IUH/Concept Cloud Sky"
{
    Properties { _Zenith("Zenith",Color)=(.14,.42,.80,1) _Horizon("Horizon",Color)=(.70,.84,.94,1) }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float4 _Zenith,_Horizon;
            struct v2f { float4 pos:SV_POSITION; float3 dir:TEXCOORD0; };
            v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.dir=v.vertex.xyz;return o;}
            float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
            float cloudNoise(float2 p){return noise(p)*.5+noise(p*2.04)*.25+noise(p*4.12)*.125+noise(p*8.25)*.0625+noise(p*16.6)*.03125;}
            float4 frag(v2f i):SV_Target
            {
                float3 d=normalize(i.dir);float h=saturate(d.y);
                float3 sky=lerp(_Horizon.rgb,_Zenith.rgb,pow(h,.6));
                float2 uv=d.xz/max(.13,d.y+.08)*3.3+float2(_Time.y*.003,0);
                uv+=float2(noise(uv*1.7),noise(uv*1.7+12))*.7;
                float n=cloudNoise(uv);
                float cloud=smoothstep(.51,.63,n)*smoothstep(.02,.18,d.y);
                float light=saturate(.65+(n-cloudNoise(uv+float2(.08,-.08)))*4);
                sky=lerp(sky,lerp(float3(.72,.79,.86),float3(1.15,1.16,1.16),light),cloud);
                return float4(sky,1);
            }
            ENDCG
        }
    }
}
