Shader "Unlit/FaceIt"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Gradient ("Gradient", 2D) = "white" {}
        _Color("Color",  Color) = (1, 1, 1, 1)
        
    }
    SubShader
    {
        Tags {"Queue"="Transparent+2" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float2 uv3 : TEXCOORD2;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 c: TEXCOORD1;
            };

            sampler2D _MainTex, _Gradient;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                
                float anim = fmod(_Time.y * .02 + v.color.y, v.color.z) / v.color.z;
                
                float4 vert = v.vertex;
                       vert.xyz *= anim * .85 *  v.color.z;
                       
                float3 up = float3(v.uv2.xy, v.uv3.x);
                       vert.xyz += up * (anim * 100 + 10 + sin(v.color.a * (1 + v.color.y) * 2 + _Time.y * .2) * 10 * anim) *  v.color.z;
                       
                
                
                float4 wPos = mul(unity_ObjectToWorld, vert);
                float3 camDir = wPos.xyz - _WorldSpaceCameraPos;
                float dist = length(camDir);
                camDir = normalize(camDir);
                
                float3 n = mul(unity_ObjectToWorld, v.normal).xyz;
                       n = -cross(camDir, n);
                    
                
                o.c = float4(length(n), anim, v.color.z, 0);
                // o.c.x = 1;
                n = normalize(n);       
                
                float3 spreadN = n * (1 - 2 * v.color.x);  
                
                       
                wPos += float4(spreadN * 22 * .35 * (.5 + .5 * anim), 0);
                
                vert = mul(unity_WorldToObject, wPos);
                
                o.c.w = length(vert.xyz); 
                o.vertex = UnityObjectToClipPos(vert);
                o.uv = v.uv + float2(0, v.color.z * 111 + v.color.y * _Time.y * .02 + v.color.z * _Time.y * -.02 );
                o.uv.x = .2 + o.uv.x * .6;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float c = 1.0 - pow(1.0 - pow(i.c.x, 10), 2);
                
                float time = _Time.y * .03333;
                float t = tex2D(_MainTex, float2(i.uv.x, i.uv.y * .35 + time * .2));
                float t2 = tex2D(_MainTex, float2(i.uv.x, i.uv.y * -.15 + time * .0324));
                float t3 = tex2D(_MainTex, float2(i.uv.x, i.uv.y * .22 + time * .311));
                t = (t * t2 * t3);
                t = 1 - pow(1 - t, 6);
                
                float sphere = max(0, i.c.w - 90);
                
                float4 col = tex2D(_Gradient, float2(t * .15 + .75 - i.c.y * .15 - sphere * .005, 0)) * 3;
                
                t = saturate(t);
                t = saturate(1 - pow(1 - ((1 - pow(1 - c * t, 2)) * pow(1 - i.c.y, 2)),2));
                t *= saturate(sphere * .5);
              
                return float4(col.xyz, t);
            }
            ENDCG
        }
    }
}
