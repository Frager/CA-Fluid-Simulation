// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/MarchingCubesVisualization"
{
	Properties
	{
		_MainTex("Texture", 3D) = "white" {}
	}

	SubShader
	{
		Tags{ "LightMode" = "ForwardBase" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#include "UnityLightingCommon.cginc"
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
				float light : BLENDINDICES;
				float3 uv : TEXCOORDS;
			};

			sampler3D _MainTex;
			float4 _MainTex_ST;

			float scale;
			int size;

			GS_INPUT vert(VS_INPUT input)
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

				float3 normal = cross(p[2].position - p[1].position, p[0].position - p[1].position);
				normal = UnityObjectToWorldNormal(normal);
				float NdotL = max(0, dot(normal, _WorldSpaceLightPos0.xyz));
				float light = _LightColor0 * NdotL;

				pIn.position = UnityObjectToClipPos(p[0].position);
				pIn.light = light;
				pIn.uv = p[0].position / (size * scale);
				triStream.Append(pIn);

				pIn.position = UnityObjectToClipPos(p[1].position);
				pIn.light = light;
				pIn.uv = p[1].position / (size * scale);
				triStream.Append(pIn);

				pIn.position = UnityObjectToClipPos(p[2].position);
				pIn.light = light;
				pIn.uv = p[2].position / (size * scale);
				triStream.Append(pIn);

				triStream.RestartStrip();
			}

			fixed4 frag(PS_INPUT input) : SV_Target
			{
				return tex3D(_MainTex, input.uv) * input.light;
			}
			ENDCG
		}
	}
}
