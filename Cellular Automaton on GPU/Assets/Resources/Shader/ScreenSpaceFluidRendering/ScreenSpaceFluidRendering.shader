Shader "Fluid/ScreenSpaceFluidRendering"
{
	Properties
	{

	}

	SubShader
	{
		Tags{ "LightMode" = "ForwardBase" }
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

				float4 worldPos = UnityObjectToClipPos(p[0].position.xyz * scale.xyz);
				float particle_size = p[0].position.w * 3;

				float ratio = _ScreenParams.x / _ScreenParams.y;

				float3 viewPos = UnityObjectToViewPos(p[0].position.xyz * scale.xyz);

				pIn.position = worldPos + float4(2, 0, 0, 0);
				pIn.viewPos = viewPos;
				pIn.uv = half2(1, 0);
				triStream.Append(pIn);

				pIn.position = worldPos;
				pIn.viewPos = viewPos;
				pIn.uv = half2(0, 0);
				triStream.Append(pIn);

				pIn.position = worldPos + float4(2, -3, 0, 0);
				pIn.viewPos = viewPos;
				pIn.uv = half2(1, 1);
				triStream.Append(pIn);

				triStream.RestartStrip();

				pIn.position = worldPos;
				pIn.viewPos = viewPos;
				pIn.uv = half2(0, 0);
				triStream.Append(pIn);

				pIn.position = worldPos + float4(0, -3, 0, 0);
				pIn.viewPos = viewPos;
				pIn.uv = half2(0, 1);
				triStream.Append(pIn);

				pIn.position = worldPos + float4(2, -3, 0, 0);
				pIn.viewPos = viewPos;
				pIn.uv = half2(1, 1);
				triStream.Append(pIn);

				triStream.RestartStrip();
			}

			fixed frag(PS_INPUT input) : SV_Target
			{
				float3 N;
				N.xy = input.uv*2.0 - 1.0;
				float r2 = dot(N.xy, N.xy);

				if (r2 > 1.0) discard;   // kill pixels outside circle
				N.z = - sqrt(1.0 - r2);
				// calculate depth
				float4 pixelPos = float4(input.viewPos + N, 1.0);
				float4 clipSpacePos = mul(pixelPos, UNITY_MATRIX_P);
				float fragDepth = clipSpacePos.z / clipSpacePos.w;

				float diffuse = max(0.0, dot(N, _WorldSpaceLightPos0.xyz));

				return fragDepth;
			}

			ENDCG
		}
	}
}

