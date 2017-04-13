Shader "Unlit/MarchingCubesVisualization"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM

			#pragma target 5.0
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			struct Triangle
			{
				float3 vertex[3];
			};

			StructuredBuffer<Triangle> triangles;

			#include "UnityCG.cginc"

			//http://stackoverflow.com/questions/20728757/unity-compute-shader-array-indexing-through-sv-dispatchthreadid

			struct VS_INPUT
			{
				uint vertexid : SV_VertexID;
			};

			struct GS_INPUT
			{
				float4 position : SV_POSITION;
				uint vertexid : BLENDINDICES;
			};

			struct PS_INPUT
			{
				float4 position : SV_POSITION;
				float3 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			GS_INPUT vert (VS_INPUT input)
			{
				GS_INPUT o;
				o.position.xyz = triangles[input.vertexid / 3].vertex[input.vertexid % 3];
				o.position.w = 1.0;
				o.vertexid = input.vertexid;
				return o;
			}

			[maxvertexcount(64)]
			void geom(triangle GS_INPUT p[3], inout TriangleStream<PS_INPUT> triStream)
			{
				PS_INPUT pIn = (PS_INPUT)0;

				pIn.position = mul(UNITY_MATRIX_MVP, p[0].position);
				pIn.color = float3(1.0f, 0.0f, 0.0f);
				triStream.Append(pIn);

				pIn.position = mul(UNITY_MATRIX_MVP, p[1].position);
				pIn.color = float3(0.0f, 1.0f, 0.0f);
				triStream.Append(pIn);

				pIn.position = mul(UNITY_MATRIX_MVP, p[2].position);
				pIn.color = float3(0.0f, 0.0f, 1.0f);
				triStream.Append(pIn);

				triStream.RestartStrip();
			}

			fixed4 frag (PS_INPUT input) : SV_Target
			{
				return float4(input.color, 1);
			}
			ENDCG
		}
	}
}
