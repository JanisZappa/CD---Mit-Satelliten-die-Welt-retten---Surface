Shader "Safari/DetectorShade"
{
    SubShader
    {
        Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
        LOD 100
        Blend DstColor Zero
        ZWRITE OFF
        
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
                float4 vertex : SV_POSITION;
                float2 wPos : TEXCOORD0;
            };

            float2 _CarPos;
            float _Radius;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.wPos = mul(unity_ObjectToWorld, v.vertex).xy;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float dist = 1 - pow(saturate((length(i.wPos - _CarPos) - _Radius) * 2), 2);
                
                return dist * .35 + .65;
            }
            ENDCG
        }
    }
}
