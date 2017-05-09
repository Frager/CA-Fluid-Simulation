Shader "Unlit/MyMarchingCubesVisualization"
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

			struct Quad
			{
				float3 vertex[4];
			};

			StructuredBuffer<Quad> quads;

			#include "UnityCG.cginc"

			//http://stackoverflow.com/questions/20728757/unity-compute-shader-array-indexing-through-sv-dispatchthreadid

			struct VS_INPUT
			{
				uint vertexid : SV_VertexID;
			};

			struct GS_INPUT
			{
				float4 positions[4] : POSITION;
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
			uint3 size;

			GS_INPUT vert(VS_INPUT input)
			{
				GS_INPUT o;
				o.positions[0] = float4(quads[input.vertexid].vertex[0], 1);
				o.positions[1] = float4(quads[input.vertexid].vertex[1], 1);
				o.positions[2] = float4(quads[input.vertexid].vertex[2], 1);
				o.positions[3] = float4(quads[input.vertexid].vertex[3], 1);
				return o;
			}

			[maxvertexcount(8)]
			void geom(point GS_INPUT p[1], inout TriangleStream<PS_INPUT> triStream)
			{
				PS_INPUT pIn = (PS_INPUT)0;

				float3 normal = cross(p[0].positions[2] - p[0].positions[1], p[0].positions[0] - p[0].positions[1]);
				normal = UnityObjectToWorldNormal(-normal);
				float NdotL = max(0, dot(normal, _WorldSpaceLightPos0.xyz));
				float light = _LightColor0 * NdotL;

				pIn.position = UnityObjectToClipPos(p[0].positions[0] * scale);
				pIn.light = light;
				pIn.uv = p[0].positions[0];
				triStream.Append(pIn);

				pIn.position = UnityObjectToClipPos(p[0].positions[3] * scale);
				pIn.light = light;
				pIn.uv = p[0].positions[3];
				triStream.Append(pIn);

				pIn.position = UnityObjectToClipPos(p[0].positions[1] * scale);
				pIn.light = light;
				pIn.uv = p[0].positions[1];
				triStream.Append(pIn);

				triStream.RestartStrip();

				pIn.position = UnityObjectToClipPos(p[0].positions[3] * scale);
				pIn.light = light;
				pIn.uv = p[0].positions[3];
				triStream.Append(pIn);

				pIn.position = UnityObjectToClipPos(p[0].positions[2] * scale);
				pIn.light = light;
				pIn.uv = p[0].positions[2];
				triStream.Append(pIn);

				pIn.position = UnityObjectToClipPos(p[0].positions[1] * scale);
				pIn.light = light;
				pIn.uv = p[0].positions[1];
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

