Shader "Andy/Earth"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Clouds("Clouds", 2D) = "white" {}
        _Spec ("SpecMap", 2D) = "white" {}
        _NormalMap ("Normals", 2D) = "bump" {}
        
        _Color ("Atmosphere", Color) = (1, 1, 1, 1)
        _CloudColor ("CloudColor", Color) = (1, 1, 1, 1)
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
             #include "Lighting.cginc"
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #include "AutoLight.cginc"
            #include "Andy.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 wPos : TEXCOORD1;
                
                float3 tspace0 : TEXCOORD2;
                float3 tspace1 : TEXCOORD3;
                float3 tspace2 : TEXCOORD4;
                
                float3 normal : TEXCOORD5;
            };

            sampler2D _MainTex, _NormalMap, _Spec, _Clouds;
            float4 _Color, _CloudColor;
            float Angle;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.wPos    = mul(unity_ObjectToWorld, v.vertex);
                
                float3 wNormal  = UnityObjectToWorldNormal(v.normal);
                o.normal = wNormal;
                float3 wTangent = UnityObjectToWorldDir(v.tangent);
                float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                float3 wBitangent = cross(wNormal, wTangent) * tangentSign;
                o.tspace0 = float3(wTangent.x, wBitangent.x, wNormal.x);
                o.tspace1 = float3(wTangent.y, wBitangent.y, wNormal.y);
                o.tspace2 = float3(wTangent.z, wBitangent.z, wNormal.z);
                
                return o;
            }
            
            
            float3 NMap(v2f i, float3 tnormal)
            {
                float3 normal;
                normal.x = dot(normalize(i.tspace0), tnormal);
                normal.y = dot(normalize(i.tspace1), tnormal);
                normal.z = dot(normalize(i.tspace2), tnormal);
                
                return normal;
            }
            

            fixed4 frag (v2f i) : SV_Target
            {
                float extra = Extra();
            
                float2 anim = float2(Angle, 0);
                float2 animUV = i.uv + anim;
            
                fixed4 col = tex2D(_MainTex, animUV);
                
                float3 earthN    = NMap(i, UnpackNormal(tex2D(_NormalMap, animUV)));
                
                float3 straightN = normalize(i.normal);
                
                float3 camDir = normalize(i.wPos - _WorldSpaceCameraPos);
                float rim = pow(1.0 - saturate(dot(-camDir, straightN)), 4);
                rim = rim * .9 + .1;
                
                float3 lightDir = normalize(Sun - i.wPos);
                
                float4 light = max(.05, dot(earthN, lightDir)) * .875 + .125;
                
                
                float3 ray = reflect(lightDir, earthN);
                float specDot = saturate(dot( ray, camDir));
                
                float specMulti = tex2D(_Spec, animUV).x;
                float4 spec = pow(specDot, 5) * .25 * _Color + pow(specDot, 100) * (.2 + specMulti * .8) *.4;
                
                
                float4 earth = col * light + spec * .9;
                float cloudMap = tex2D(_Clouds, i.uv + anim * 1.1).x;
                float cloudDot = max(0, dot(straightN, lightDir)) * .7 + .3;
                float4 result = lerp(earth, _CloudColor * cloudDot, cloudMap * .8);
                
                float3 color =  _Color.xyz * 1;//(.9 +.1 * saturate(dot(lightDir, straightN)));
                
                return pow(lerp(result, fixed4(color, 1), rim), 1.4) * 1.1;
            }
            ENDCG
        }
    }
}
