Shader "MarchingCubes/Multiple/Flat"
{
	Properties
	{
		_MainTex("Texture", 3D) = "white" {}
	}

	SubShader
	{
		Tags{ "LightMode" = "ForwardBase" "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#include "UnityLightingCommon.cginc"
			#include "UnityCG.cginc"
			#pragma target 5.0
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			static half4 Colors[3] =
			{
				half4(0,0,0,0),
				half4(1,1,0,0.5),
				half4(0.2,0.5,1,0.5)
			};

			//Thats the ouput type of the Marching Cubes-algorithm Compute Shader
			struct Triangle
			{
				half3 vertex[3];
				int element;
			};

			//Thats the ouput of the Marching Cubes-algorithm Compute Shader
			StructuredBuffer<Triangle> mesh;

			uniform float4 _horizonColor;

			uniform float4 WaveSpeed;
			uniform float _WaveScale;
			uniform float4 _WaveOffset;

			struct VS_INPUT
			{
				uint vertexid : SV_VertexID;
			};

			struct GS_INPUT
			{
				float4 positions[3] : POSITION;
				float4 color : COLOR;
			};

			struct PS_INPUT
			{
				float4 position : SV_POSITION;
				float4 color : COLOR;
			};


			sampler3D _MainTex;

			float4 offset;
			float4 scale;

			GS_INPUT vert(VS_INPUT input)
			{
				GS_INPUT o;
				o.positions[0] = float4(mesh[input.vertexid].vertex[0], 1);
				o.positions[1] = float4(mesh[input.vertexid].vertex[1], 1);
				o.positions[2] = float4(mesh[input.vertexid].vertex[2], 1);

				o.color = Colors[mesh[input.vertexid].element];
				return o;
			}

			[maxvertexcount(3)]
			void geom(point GS_INPUT p[1], inout TriangleStream<PS_INPUT> triStream)
			{
				PS_INPUT pIn = (PS_INPUT)0;

				fixed3 normal = cross(p[0].positions[2] - p[0].positions[1], p[0].positions[1] - p[0].positions[0]);
				normal = UnityObjectToWorldNormal(normal);
				fixed NdotL = max(0, dot(normal, _WorldSpaceLightPos0.xyz));
				fixed4 light = fixed4(_LightColor0.xyz * NdotL, 1);

				[unroll(3)]
				for (int i = 2; i >= 0; --i)
				{
					pIn.position = UnityObjectToClipPos(p[0].positions[i] * scale + offset);

					pIn.color = p[0].color * light;

					triStream.Append(pIn);
				}

				triStream.RestartStrip();
			}

			fixed4 frag(PS_INPUT input) : SV_Target
			{
				return input.color;
			}
			ENDCG
		}
	}
}
