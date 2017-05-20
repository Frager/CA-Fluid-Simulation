// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/RigidBodySurfaceShader" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert

		#pragma target 5.0

#ifdef SHADER_API_D3D11
		StructuredBuffer<float3> newPositions;
#endif

		sampler2D _MainTex;
		fixed4 _Color;

		struct Input 
		{
			float2 uv_MainTex;
		};

		void vert(inout appdata_full v)
		{
			float4 pos = v.vertex;

#ifdef SHADER_API_D3D11
			//float4x4 transform = { 1,0,0,newPositions[0].x,
			//	0,1,0,newPositions[0].y,
			//	0,0,1,newPositions[0].z,
			//	0,0,0,1 };

			//pos = mul(transform, pos);
			//pos = mul(UNITY_MATRIX_V, pos);
			//pos = mul(UNITY_MATRIX_P, pos);

			v.vertex.xyz += newPositions[0];
#endif
		}

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
