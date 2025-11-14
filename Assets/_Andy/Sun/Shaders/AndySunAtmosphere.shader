Shader "Unlit/AndySunAtmosphere"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color",  Color) = (1, 1, 1, 1)
        _Color2("Color2",  Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags {"Queue"="Transparent-1" "IgnoreProjector"="True" "RenderType"="Transparent"}
        Blend One One
        LOD 100
        CULL Front
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                float3 wPos : TEXCOORD2;
                //float3 cPos : TEXCOORD3;
            };

            sampler2D _MainTex, _Gradient;
            float4 _Color, _Color2;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = mul(unity_ObjectToWorld, v.vertex.xyz).xyz;
                o.wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                // o.cPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //float cDist = length(i.cPos - _WorldSpaceCameraPos);
                float3 camDir = i.wPos - _WorldSpaceCameraPos;
                float pDist = length(camDir);
                
                //clip(pDist - cDist - 60);
                
                //return 1;
                camDir /= pDist;
                
                
                float rim = saturate(abs(dot(camDir,  normalize(i.normal))));
                
                
                
                //clip(rim - .5);
                //return rim;
         
                float rim2 = pow(rim , 8) * .99;
                
                float4 data = tex2D(_MainTex, i.uv);
                float t = _Time.y * 2.5 + (data.x - data.y + data.z) * 4;
            
                
                float l = data.x;
                      l = saturate(l + sin(t * .25 + data.y * 11.1231 * .5 + data.z * 31 * .5) * .25 * data.z + sin(t * .02312 * .5 + data.z * .2 + data.x * 3.121) * .15 * data.y);
                      
                float l2 = saturate(l + sin(t * .25 * .733+ data.y * -11.1231 * .25 + data.z * 31 * .25) * .25 * data.z + sin(t * .02312 * .25  * 1.233 + data.z * .2 + data.x * -3.121 * .5) * .15 * data.y);
                      l = l2;
                // return l;
                      
                float rim3 = pow(saturate(rim * 20 - 15.65), 10) * .99;
                return (_Color * (1 - pow(1 - pow(rim2 * (.96 + .04 * l), 2), 8)) * .75) + rim3 * _Color2;
                // return lerp(tex2D(_Gradient, float2(l, 0)), _Color, .15 + rim * .05 + l * .1);
            }
            ENDCG
        }
    }
}
