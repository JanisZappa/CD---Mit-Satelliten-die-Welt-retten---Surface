Shader "Andy/Atmosphere"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWRITE ON
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Andy.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float3 normal : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 wPos : TEXCOORD1;
            };

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = mul(unity_ObjectToWorld, v.normal);
                o.wPos   = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal = normalize(i.normal);
                float3 camDir = normalize(i.wPos - _WorldSpaceCameraPos);
                float rim = 1.0 - saturate(dot(-camDir, normal) + .585);
                
                float extra = 0;//Extra();
                rim = 1 - pow(1 - pow(1 - rim, lerp(100, 1, extra)), 2);
                //return rim;
                float3 sundir = normalize(Sun - _WorldSpaceCameraPos);
                float sundot = dot(sundir, camDir);
                
                float3 col =  _Color.xyz * 1;//(.9 +.1 * saturate(dot(-sundir, normal)));
               // return fixed4(col, 1);
                
                float3 c = col + (1 - pow(1 - pow(saturate(sundot), 1500), 2)) * .5;
                       c += (1 - pow(1 - pow(saturate(sundot), 6000), 2)) * .3;
                       c += (pow(sundot * .5 + .5, 20) * 2 - 1) * .1;
                
                //return pow(extra, 14);
                return float4(c, saturate(rim + pow(extra, 20) * .15));
            }
            ENDCG
        }
    }
}
