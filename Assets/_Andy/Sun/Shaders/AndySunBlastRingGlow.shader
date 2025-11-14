Shader "Unlit/AndySunBlastRingGlow"
{
    Properties
    {
       _Color("Color",  Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags {"Queue"="Transparent-1" "IgnoreProjector"="True" "RenderType"="Transparent"}
        Blend One One
        LOD 100
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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 wPos : TEXCOORD1;
            };

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = mul(unity_ObjectToWorld, v.normal);
                o.wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
               
                float3 normal = normalize(i.normal);
                float3 camDir = normalize(i.wPos - _WorldSpaceCameraPos);
                float rim = saturate(dot(-camDir, -normal));
                      rim = pow(saturate(rim * 1.2 - .3), 4);
                return _Color * rim * 2;
            }
            ENDCG
        }
    }
}
