Shader "Unlit/AndySun"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Gradient ("Gradient", 2D) = "white" {}
        _Color("Color",  Color) = (1, 1, 1, 1)
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                float3 wPos : TEXCOORD2;
            };

            sampler2D _MainTex, _Gradient;
            float4 _Color;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = mul(unity_ObjectToWorld, v.vertex.xyz);
                o.wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal = normalize(i.normal);
                float3 camDir = normalize(i.wPos - _WorldSpaceCameraPos);
                float rim = 1.0 - saturate(dot(-camDir, normal));
                      rim = (1 - pow(1 - rim, 2)) * .5 + pow(rim,2) * .5;
                //return rim;
                
                float4 data = tex2D(_MainTex, i.uv);
                float t = _Time.y * 2.5 + (data.x - data.y + data.z) * 1.3333 * 3.141592653589793;
            
                
                float l = data.x;
                      l = saturate(l + sin(t * .25 + data.y * 11.1231 * .5 + data.z * 31 * .5) * .25 * data.z + sin(t * .02312 * .5 + data.z * .2 + data.x * 3.121) * .15 * data.y);
                      
                float l2 = saturate(l + sin(t * .25 * .733+ data.y * -11.1231 * .25 + data.z * 31 * .25) * .25 * data.z + sin(t * .02312 * .25  * 1.233 + data.z * .2 + data.x * -3.121 * .5) * .15 * data.y);
                      l = l2;
                      l = (l - .5) * (1 - rim * .3) + .5;
                      
                      l = saturate(l * .8 + .01 + rim * .3 - (1 - rim) * .05);
                      
                
                //return l;
                return lerp(tex2D(_Gradient, float2(l, 0)), _Color, .15 + rim * .2 + l * .1);
            }
            ENDCG
        }
    }
}
