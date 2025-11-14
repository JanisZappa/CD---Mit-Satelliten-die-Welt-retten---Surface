Shader "PBR/Mimic_Backup"
{
    Properties
    {
        _Albedo      ("Albedo Color", Color) = (1,1,1,1)
        _AlbedoTex   ("Albedo Texture", 2D) = "white" {}
        _Metallic    ("Metallic", Range(0,1)) = 0
        _MetallicTex ("Metallic Texture", 2D) = "white" {}
        _Smoothness  ("Smoothness", Range(0,1)) = 0.5
        _NormalMap   ("Normal Map", 2D) = "bump" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 300
        Cull Back
        ZWrite On
        ZTest LEqual

        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   3.0

            #pragma multi_compile_fwdbase
            #pragma multi_compile __ UNITY_SPECCUBE_BOX_PROJECTION

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex  : POSITION;
                float3 normal  : NORMAL;
                float4 tangent : TANGENT;
                float2 uv      : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos          : SV_POSITION;
                float3 worldPos     : TEXCOORD0;
                float3 worldNormal  : TEXCOORD1;
                float3 worldTangent : TEXCOORD2;
                float3 worldBitan   : TEXCOORD3;
                float2 uv           : TEXCOORD4;
                float4 sPos : TEXCOORD5;
            };

            sampler2D _NormalMap;
            float4 _NormalMap_ST;

            sampler2D _AlbedoTex;
            sampler2D _MetallicTex;

            fixed4 _Albedo;
            half _Metallic;
            half _Smoothness;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos          = UnityObjectToClipPos(v.vertex);
                o.worldPos     = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal  = UnityObjectToWorldNormal(v.normal);
                o.worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                o.worldBitan   = cross(o.worldNormal, o.worldTangent) * v.tangent.w;
                o.uv           = TRANSFORM_TEX(v.uv, _NormalMap);
                o.sPos = ComputeScreenPos(o.pos);
                return o;
            }

            inline float3 FresnelSchlick(float cosTheta, float3 F0)
            {
                return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
            }

            inline float3 FresnelSchlickRoughness(float cosTheta, float3 F0, float roughness)
            {
                float3 oneMinusR = float3(1.0 - roughness, 1.0 - roughness, 1.0 - roughness);
                return F0 + (max(oneMinusR, F0) - F0) * pow(1.0 - cosTheta, 5.0);
            }

            inline float DistributionGGX(float3 N, float3 H, float roughness)
            {
                float a  = roughness * roughness;
                float a2 = a * a;
                float NdotH  = saturate(dot(N, H));
                float NdotH2 = NdotH * NdotH;
                float denom  = (NdotH2 * (a2 - 1.0) + 1.0);
                return a2 / (UNITY_PI * denom * denom + 1e-5);
            }

            inline float GeometrySchlickGGX(float NdotX, float roughness)
            {
                float r = roughness + 1.0;
                float k = (r * r) / 8.0;
                return NdotX / (NdotX * (1.0 - k) + k);
            }

            inline float GeometrySmith(float3 N, float3 V, float3 L, float roughness)
            {
                float NdotV = saturate(dot(N, V));
                float NdotL = saturate(dot(N, L));
                return GeometrySchlickGGX(NdotV, roughness) * GeometrySchlickGGX(NdotL, roughness);
            }

            #ifndef UNITY_SPECCUBE_LOD_STEPS
            #define UNITY_SPECCUBE_LOD_STEPS 7.0
            #endif

            float3 BoxProjectedDir(float3 R, float3 worldPos)
            {
                #if defined(UNITY_SPECCUBE_BOX_PROJECTION)
                float3 rbmin = unity_SpecCube0_BoxMin.xyz - worldPos;
                float3 rbmax = unity_SpecCube0_BoxMax.xyz - worldPos;
                float3 rpos  = worldPos - unity_SpecCube0_ProbePosition.xyz;

                float3 invR = 1.0 / max(abs(R), 1e-5);
                float3 tmin = (rbmin - rpos) * invR;
                float3 tmax = (rbmax - rpos) * invR;
                float3 t1   = min(tmin, tmax);
                float2 t    = max(t1.xx, t1.yz);
                float  tc   = max(t.x, t.y);

                float3 posOnBox = rpos + R * tc;
                return normalize(posOnBox);
                #else
                return R;
                #endif
            }

            float3 SampleSpecularIBL(float3 R, float roughness, float3 worldPos)
            {
                float3 Rproj = BoxProjectedDir(R, worldPos);
                float mip    = saturate(roughness) * UNITY_SPECCUBE_LOD_STEPS;
                half4 c      = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, Rproj, mip);
                return DecodeHDR(c, unity_SpecCube0_HDR);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // TBN normal mapping
                float3 T = normalize(i.worldTangent);
                float3 B = normalize(i.worldBitan);
                float3 N0= normalize(i.worldNormal);
                float3x3 TBN = float3x3(T, B, N0);
                float3 nTS   = UnpackNormal(tex2D(_NormalMap, i.uv));
                // nTS = normalize(float3(nTS.xy, nTS.z * .2));
                float3 N     = normalize(mul(nTS, TBN));
              

                float3 V = normalize(_WorldSpaceCameraPos - i.worldPos);

                // Albedo and metallic
                float3 albedoTex = tex2D(_AlbedoTex, i.uv).rgb;
                float3 albedo = albedoTex * _Albedo.rgb;

                float metallicTex = tex2D(_MetallicTex, i.uv).r;
                float metallic = saturate(lerp(_Metallic, metallicTex, metallicTex));

                float roughness = saturate(1.0 - _Smoothness);
                float3 F0 = lerp(float3(0.04,0.04,0.04), albedo, metallic);

                // Main directional ligh
                float3 L = normalize(float3(-.2, 1, -1));
                float3 H = normalize(V + L);
                float  NdotL = saturate(dot(N,L));
                float  NdotV = saturate(dot(N,V));

                float  D = DistributionGGX(N,H,roughness);
              
                float  G = GeometrySmith(N,V,L,roughness);
                float3 F = FresnelSchlick(saturate(dot(H,V)), F0);

                float3 kS = F;
                float3 kD = (1.0 - kS) * (1.0 - metallic);

                float3 specular = (D*G*F)/max(4.0*NdotL*NdotV,1e-4);
                float3 Lo_dir   = (kD*albedo/UNITY_PI + specular) * NdotL;

                // IBL diffuse
                float3 diffuseIBL = ShadeSH9(float4(N,1.0)) * 10;
                float3 Ld_ibl     = diffuseIBL * albedo * (1.0 - metallic);

                // IBL specular
                float3 R = reflect(-V,N);
                float3 prefilteredColor = SampleSpecularIBL(R, roughness, i.worldPos);
                float3 F_ibl = FresnelSchlickRoughness(NdotV,F0,roughness);
                float3 Ls_ibl = prefilteredColor * F_ibl;

                float3 color = Lo_dir + (kD*Ld_ibl/UNITY_PI) + Ls_ibl + D * .025;
                      // color = pow(color, 1.5);

                float4 result = float4(color,1.0);
                float2 uv = i.sPos.xy / i.sPos.w;
                float l = abs(uv.x - .5) * 2.35;
                      l = lerp(l, 0, step(10, i.worldPos.z));
                clip(1 - l * _Color.a);
                l = 1.0 - pow(1.0 - pow(saturate(l), 10), 2);
                return lerp(result, _Color, l * _Color.a);
            }
            ENDCG
        }
    }

    FallBack Off
}