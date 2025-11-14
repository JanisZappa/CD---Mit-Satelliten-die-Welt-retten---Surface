Shader "Unlit/SimpleTopDownLight"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "BlueFade.cginc"

            float4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 n : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 n : TEXCOORD0;
                float4 sPos : TEXCOORD1;
            };


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.n = mul(unity_ObjectToWorld, v.n);
                o.sPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 n = normalize(i.n);
                float d = dot(n, normalize(float3(0, 1, -1.5)));

                float4 result = pow(d * .5 + .5, 4);
                float2 uv = i.sPos.xy / i.sPos.w;
                float l = abs(uv.x - .5) * 2.35;
                clip(1 - l * _Color.a);
                l = 1.0 - pow(1.0 - pow(saturate(l), 10), 2);
                return lerp(result, _Color, l * _Color.a);
            }
            ENDCG
        }
    }
}
