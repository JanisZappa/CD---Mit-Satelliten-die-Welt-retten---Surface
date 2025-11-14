Shader "Custom/VertexColorSurf"
{
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf BlinnPhong vertex:vert
		#pragma target 3.0
		
        samplerCUBE _Cube;

		struct Input {
			float4 vertColor;
			float3 normal;
		};

		void vert(inout appdata_full v, out Input o){
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.vertColor = v.color;
			//o.normal = v.normal;
		}

         
		void surf (Input IN, inout SurfaceOutput o) 
		{
			o.Albedo   = IN.vertColor.rgb * .75;
			o.Emission = IN.vertColor.rgb * .25;
			//o.Emission = 1;
            o.Specular = 1;
            o.Gloss = 1;
           // o.Normal = IN.normal;
		}
		ENDCG
    }
    FallBack "Diffuse"
}
