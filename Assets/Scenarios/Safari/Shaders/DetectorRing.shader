Shader "Safari/DetectorRing"
{
    SubShader
    {
        Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha

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
            
            float _Radius, _Thickness;

            v2f vert (appdata v)
            {
                v2f o;
                
                float3 vert = v.vertex.xyz;
                       vert *= _Radius + v.uv.y * .15 * .5 * .75;
                       vert.z = -50;
                o.vertex = UnityObjectToClipPos(float4(vert, 1));
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float r = _Radius * 20 / 6;
                      //r = lerp(r, round(r), .25);
                      r = round(r);
                float v = i.uv.x * r + _Time.y * .25;
                      v = fmod(v, 1);
                      v = abs(.5 - v) * 2;
                      v = saturate(v - .5) * (1.0 / .5);
                      v = 1 - v;
                      v = pow(v, 8);
                return float4(1, 1, 1, v);
            }
            ENDCG
        }
    }
}
