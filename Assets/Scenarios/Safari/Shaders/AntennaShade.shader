Shader "Safari/AntennaShade"
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
            };

            struct v2f
            {
                float2 wPos : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.wPos = mul(unity_ObjectToWorld, v.vertex).xy;
                return o;
            }
            
            
            StructuredBuffer<float3>antennas;
            int aCount;
            float antennaThresh;

            fixed4 frag (v2f i) : SV_Target
            {
                float a = 0;
                float b = 0;
                float minDist = 100000000;
                for(int e = 0; e < aCount; e++)
                {
                    float2 d = antennas[e].xy - i.wPos;
                    float dist = d.x * d.x + d.y * d.y;
                    minDist = min(minDist, dist);
                    a = max(a, step(dist * antennaThresh, 1));
                    b = max(b, step(dist * antennaThresh * 1.075, 1));
                }
                return max(a * (1 - b) * .5, (sin(sqrt(minDist) + _Time.y * -1.5) * .5 + .5) * a * .1) * 1;
            }
            ENDCG
        }
    }
}
