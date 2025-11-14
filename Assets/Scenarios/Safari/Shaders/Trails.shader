Shader "Safari/Trails"
{               
    SubShader
    {
        Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
        LOD 100
        Blend One One

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex + float4(0, 0, 1, 0));
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float w = 1 - abs(i.uv.x - .5) * 2;
                float v = i.uv.y * 512 + w * .35;
                      v = fmod(v, 1);
                      v = abs(.5 - v) * 2;
                      v = saturate(v - .75) * (1.0 / .25);
                      v = 1 - v;
                      v = pow(v, 8);
                      v = v * 3 + (1 - pow(1 - w, 2));
                      v *= .25;
                      v *= 1 - pow(i.uv.y, 2);
                return pow(v, 2) * -.045;
            }
            ENDCG
        }
    }
}
