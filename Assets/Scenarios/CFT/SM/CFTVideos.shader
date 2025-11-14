Shader "Unlit/CFTVideos"
{
    Properties
    {
        _VideoA ("VideoA", 2D) = "white" {}
        _VideoB ("VideoB", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
        ZWrite Off
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

            sampler2D _VideoA, _VideoB;
            float GameVis;
            float2 window;
            float VideoMask;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = v.uv;
                return o;
            }
            
            
            float rectangle(float2 samplePosition, float2 halfSize)
            {
                float2 edgeDistance    = abs(samplePosition) - halfSize;
                float  outsideDistance = length(max(edgeDistance, 0));
                float  insideDistance  = min(max(edgeDistance.x, edgeDistance.y), 0);
                
                return outsideDistance + insideDistance;
            }
            

            float4 frag (v2f i) : SV_Target
            {
                float4 colA = tex2D(_VideoA, i.uv);
                float4 colB = tex2D(_VideoB, i.uv);
                
                float2 wP    = window / 9.0 + .5;
                float offset = rectangle((i.uv - wP) , float2(.135, .135) * .8) - .02;
                float      l = (1 - saturate(offset * 1000)) * VideoMask;

                float m = 1; // (.5 + .5 * l)
                return float4(lerp(colA, colB, l).xyz * m, GameVis);
            }
            ENDCG
        }
    }
}
