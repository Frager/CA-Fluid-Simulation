// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Fluid/ScreenSpaceFluidRendering"
{
	Properties
	{
		_Offset("Offset", Vector) = (-0,0,-0,0)
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#include "UnityLightingCommon.cginc"
			#include "UnityCG.cginc"
			#pragma target 5.0
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag


			StructuredBuffer<half4> mesh;

			float4 _Offset;

			struct VS_INPUT
			{
				uint vertexid : SV_VertexID;
			};

			struct GS_INPUT
			{
				float4 position : SV_POSITION;
			};

			struct PS_INPUT
			{
				float4 position : SV_POSITION;
				float3 viewPos : POSITION1;
				float2 uv : TEXCOOR1;
			};

			struct PS_OUTPUT
			{
				float color : SV_TARGET;
				float depth : SV_DEPTH;
			};

			float4 scale;

			GS_INPUT vert(VS_INPUT input)
			{
				GS_INPUT o;
				o.position = mesh[input.vertexid];
				return o;
			}

			[maxvertexcount(6)]
			void geom(point GS_INPUT p[1], inout TriangleStream<PS_INPUT> triStream)
			{
				PS_INPUT pIn = (PS_INPUT)0;

				float4 worldPos = UnityObjectToClipPos(p[0].position.xyz * scale.xyz + _Offset);

				float particle_size = p[0].position.w * 3;

				float ratio = _ScreenParams.x / _ScreenParams.y;

				float3 viewPos = UnityObjectToViewPos(p[0].position.xyz * scale.xyz + _Offset);

				pIn.position = worldPos + float4(2, 0, 0, 0);
				pIn.viewPos = viewPos;
				pIn.uv = half2(1, 0);
				triStream.Append(pIn);

				pIn.position = worldPos;
				pIn.viewPos = viewPos;
				pIn.uv = half2(0, 0);
				triStream.Append(pIn);

				pIn.position = worldPos + float4(2, -2 * ratio * particle_size, 0, 0);
				pIn.viewPos = viewPos;
				pIn.uv = half2(1, 1);
				triStream.Append(pIn);

				triStream.RestartStrip();

				pIn.position = worldPos;
				pIn.viewPos = viewPos;
				pIn.uv = half2(0, 0);
				triStream.Append(pIn);

				pIn.position = worldPos + float4(0, -2 * ratio * particle_size, 0, 0);
				pIn.viewPos = viewPos;
				pIn.uv = half2(0, 1);
				triStream.Append(pIn);

				pIn.position = worldPos + float4(2, -2 * ratio * particle_size, 0, 0);
				pIn.viewPos = viewPos;
				pIn.uv = half2(1, 1);
				triStream.Append(pIn);

				triStream.RestartStrip();
			}

			PS_OUTPUT frag(PS_INPUT input)
			{
				PS_OUTPUT o;
				float3 N;
				N.xy = input.uv*2.0 - 1.0;
				float r2 = dot(N.xy, N.xy);

				if (r2 > 1.0)
				{
					o.color = 1;
					o.depth = 0;
					return o;
				}

				N.z = - sqrt(1.0 - r2);

				float3 viewPos = input.viewPos + N;

				float4 clipSpacePos = mul(float4(viewPos, 1.0), UNITY_MATRIX_P);

				o.color = float(-((viewPos.z) * _ProjectionParams.w));
				o.depth = input.position.z / input.position.w;
				return o;
			}

			ENDCG
		}
	}
}

