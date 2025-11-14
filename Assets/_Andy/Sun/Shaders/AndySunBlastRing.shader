Shader "Unlit/AndySunBlastRing"
{
    Properties
    {
       _Color("Color",  Color) = (1, 1, 1, 1)
       _Color2("Rim",  Color) = (1, 1, 1, 1)
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

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 wPos : TEXCOORD1;
            };

            float4 _Color, _Color2;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = mul(unity_ObjectToWorld, v.normal);
                o.wPos   = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal = normalize(i.normal);
                float3 camDir = normalize(i.wPos - _WorldSpaceCameraPos);
                float rim = saturate(dot(-camDir, normal));
                return lerp(_Color2, _Color, pow(rim, 2));
            }
            ENDCG
        }
    }
}
