Shader "Unlit/AndySunMagnet"
{
    Properties
    {
         _Color("Color",  Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
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
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
                float2 uv2    : TEXCOORD1;
                float2 uv3    : TEXCOORD2;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                float4 wPos : TEXCOORD2;
                float3 anormal : TEXCOORD3;
                float4 color : TEXCOORD4;
            };

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = mul(unity_ObjectToWorld, float3(v.uv2.x, v.uv2.y, v.uv3.x));
                o.anormal = mul(unity_ObjectToWorld, v.normal);
                o.wPos = float4(mul(unity_ObjectToWorld, v.vertex).xyz, length(v.vertex.xyz));
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //return float4(normalize(i.normal) * .5 + .5, 1) * .1;
                float3 camDir = normalize(i.wPos.xyz - _WorldSpaceCameraPos);
                float rim = 1.0 - abs(dot(-camDir,  normalize(i.normal)));
                     // rim = (1 - pow(1 - rim, 2)) * .5 + pow(rim,2) * .5;
                     rim = pow(1 - pow(1 - rim , 6), 2);
               // return rim * .1;//    
                float rim2 = 1 - pow(1 - pow(saturate(dot(-camDir, normalize(i.anormal))), 4), 2);
                
                float v = abs(i.uv.y * 2 - 1);
                float a = saturate(pow(1 - abs(i.uv.y * 2 - 1), 6) - (1 - rim)) * rim2;  
                
                a *= .5 + .5 * (sin(i.uv.x * (10 + i.color.x * 2) * 2 * 3.1415926535 + _Time.y * .5) * .5 + .5);
                a *= .65 + .35 * (sin(i.uv.x * (33 + i.color.x * 6) * 3.1415926535 + _Time.y * 1.5) * .5 + .5);
                a *= sin(_Time.y * .2 + i.color.y * 20) * .25 + .75;
                a *= sin(_Time.y * (.6 + i.color.y * .3)  + v * 20 + i.color.y * 50) * .25 + .75;
                a *= saturate((i.wPos.w - 90) * .5);
                
                return _Color * saturate(a);
            }
            ENDCG
        }
    }
}
