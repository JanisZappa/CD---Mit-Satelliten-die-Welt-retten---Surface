Shader "Car/VertexReflectRoads"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase"}
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma multi_compile_fwdbase
            #include "AutoLight.cginc"

           struct appdata
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color  : TEXCOORD0;
                float3 wPos : TEXCOORD1;
                float3 normal : TEXCOORD2;
                LIGHTING_COORDS(3,4)
                float2 uv : TEXCOORD5;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float3 Focus;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color  = float4(v.color.xyz, v.uv.x);
                o.wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = mul(unity_ObjectToWorld, v.normal);
                TRANSFER_VERTEX_TO_FRAGMENT(o);
                o.uv = v.vertex.xz;
                return o;
            }
            
            
            float Grid(float2 uv)
            {
                //uv.y *= 2;
                float2 boxSize  = clamp(fwidth(uv) * _MainTex_TexelSize.zw, 1e-5, 1.0);
                float2 tx       = uv * _MainTex_TexelSize.zw - .5 * boxSize; 
                float2 txOffset = saturate((frac(tx) - (1.0 - boxSize)) / boxSize);
                return -.1 * (1 - pow(saturate(1.0 - max(txOffset.x, txOffset.y)), 100));
            }


            float4 frag (v2f i) : SV_Target
            {
                float3 dir = i.wPos - _WorldSpaceCameraPos;
                float dist = length(float3(dir.x, dir.y * 1.5, dir.z));
                dir = normalize(dir);
                
                float3 normal = normalize(i.normal);
                //return normal.z;
                
                float d = max(0, dot(normal, -dir));
                float3 ref = reflect(dir, normal);
                
                //return r;
                
                float lD = max(0, dot(normal, _WorldSpaceLightPos0.xyz));
                float day = 1 - pow(1 - saturate(_WorldSpaceLightPos0.y), 2);
                
                float shadow = LIGHT_ATTENUATION(i) * lD;
                float a = saturate(i.color.a * .99);
                      a = saturate(a * 1.1375 - .1375);
                      a = 1 - pow(1 - pow(1 - a, 220), 10);
                      
                float r = pow(max(0, dot(ref, _WorldSpaceLightPos0.xyz)), 10 + a * 290);
                float light =  (shadow * .65 + .35) * (d * .5 + .75) * (day * .5 + .5) + lD * .2 + r * (.5 + a * .5) * (.2 + .8 * shadow) + pow(1 - d, 2 + a * 2) * (.25 + a * .25);
                
                float fog = pow(min(dist * .00125, 1), 3) * .5;
                
                //return fog;
                float3 color = i.color.xyz;
                
                       color = lerp(color, color * .125 + .05, a * .35);
                       color = lerp(color, float3(.3, .2, .15), .65);
                       //color += Grid(i.uv * .25) * .1 + Grid(i.uv * .25 * .25) * .1;
                float3 result = color * light;
                       //result += (1 - shadow) * float3(-.02, .002, .04);
                       result = float3(result.x - fog * .05, result.y + fog * .125, result.z + fog * .2);
                       result += lerp(float3(-.02, .002, .04) * 1.5, float3(.02, .002, 0), shadow) * (day * .6 + .4);
                
                float fDist = length(Focus - i.wPos) * .00125;
                       result += pow(result, 1.33333) * fDist;
                       result = pow(result, 1.55 + .25 * (1 - day)) * 1.6666666 * (day * .25 + .75) * 1.1;
                       
                        
                return float4(result, 1);
            }
            ENDCG
        }
        
        // shadow casting support
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
