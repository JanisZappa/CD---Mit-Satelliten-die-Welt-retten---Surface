struct v2f
{
    float4 pos  : SV_POSITION;
    float3 normal : TEXCOORD0;
    float3 wPos : TEXCOORD1;
    float4 color : TEXCOORD2;
    float2 uv : TEXCOORD3;
    
    SHADOW_COORDS(4)
};

sampler2D _Pattern;
float4 _Pattern_ST;

samplerCUBE _Tex, _LightTex;
float _Amount;
float _Tint;
float _Roughness;
float _Metalic;
float4 _EmissionColor;

fixed4 frag (v2f i) : SV_Target
{
    /*float u = (i.uv.y * 50 + sin(i.uv.x * 120) * .25) % 2;
          u = abs(u - 1);
          u = saturate((u - .4) * 10);
          u = lerp(pow(u, 2), 1.0 - pow(1.0 - u, 2), u);*/
    float u = tex2D(_Pattern, i.uv).x;
    float3 normal = normalize(i.normal);
    
    float3 camDir = normalize(i.wPos - _WorldSpaceCameraPos);
    float rim = 1.0 - saturate(dot(-camDir, normal));
    float rimB = rim;
    rim = 1.0 - pow(rim, 3);
    
    fixed shadow = SHADOW_ATTENUATION(i);
    shadow *= max(0, dot(normal, _WorldSpaceLightPos0.xyz));
   
    float4 light = texCUBElod (_Tex, float4(normal, 8));
           light = texCUBE (_LightTex, normal);
           light = light * (1 - _Metalic) * 1.5  + shadow + .25;
    //return light;
    
    float r = 1 - ((1 - _Roughness) * rim * (.25 + .75 * (1 - u)));
    
    float roughness = r * (1.7 - 0.7 * r);
    
    float4 light2 = texCUBElod (_Tex, float4(reflect(camDir, normal), roughness * 6)) * (1 - r);
    //return light2;
    
    float4 color = float4(i.color.rgb * (.8 + .2 * u), 1);
           //color = float4(lerp(color.xyz, _EmissionColor.xyz, _EmissionColor.a * rim), 1);
           
    float4 result = (light2 ) * lerp(color * _Amount, _Amount, 1 - _Tint);
    
    float4 c = color;//lerp(pow(color, 5), color, i.color.a);
    //return c;
    
    float m = (1 - _Metalic) * (.8 + .2 * u);
    result = c * light * m + result;
    result = pow(result, 1.3) * 1.3;
    
    float4 e = _EmissionColor;
    return float4(lerp(result.xyz, e.xyz, e.a * (.3 + .7 * rimB)) * (.25 + .75 * i.color.a), 1);
}