Shader "Unlit/MarchingCubesVisualization"
{
	Properties
	{
		_horizonColor("Horizon color", COLOR) = (.172 , .463 , .435 , 0)
		_WaveScale("Wave scale", Range(0.02,0.15)) = .07
		[NoScaleOffset] _ColorControl("Reflective color (RGB) fresnel (A) ", 2D) = "" { }
		[NoScaleOffset] _BumpMap("Waves Normalmap ", 2D) = "" { }
		WaveSpeed("Wave speed (map1 x,y; map2 x,y)", Vector) = (19,9,-16,-7)
	}

	SubShader
	{
		Tags{ "LightMode" = "ForwardBase" "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 100

		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#include "UnityLightingCommon.cginc"
			#pragma target 5.0
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			#define NUMBER_OF_ELEMENTS 3
			#define ADDRESS(x,y,z,size) x + y * size + z * size * size
			struct Cell
			{
				float content[NUMBER_OF_ELEMENTS];
				float volume;
				float temperature;
			};

			//Thats the ouput type of the Marching Cubes-algorithm Compute Shader
			struct Triangle
			{
				float3 vertex[3];
				uint3 cell[3];
			};

			static float4 Colors[4] =
			{
				float4(1,0,0,1),
				float4(0,1,1,1),
				float4(1,1,0,1),
				float4(0.2,0.5,1,1)
			};

			//Thats the ouput of the Marching Cubes-algorithm Compute Shader
			StructuredBuffer<Triangle> triangles;

			StructuredBuffer<Cell> currentGeneration;

			#include "UnityCG.cginc"

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
				uint3 cells[3] : BLENDINDICES;
			};

			struct PS_INPUT
			{
				float4 position : SV_POSITION;
				float4 color : COLOR;
				float2 bumpuv[2] : TEXCOORD0;
				float3 viewDir : TEXCOORD2;
			};

			sampler2D _BumpMap;
			sampler2D _ColorControl;

			float scale;
			int size;

			GS_INPUT vert(VS_INPUT input)
			{
				GS_INPUT o;
				o.positions[0] = float4(triangles[input.vertexid].vertex[0], 1);
				o.positions[1] = float4(triangles[input.vertexid].vertex[1], 1);
				o.positions[2] = float4(triangles[input.vertexid].vertex[2], 1);

				o.cells[0] = triangles[input.vertexid].cell[0];
				o.cells[1] = triangles[input.vertexid].cell[1];
				o.cells[2] = triangles[input.vertexid].cell[2];

				return o;
			}

			uint GetCellsElement(uint3 id)
			{
				Cell cell = currentGeneration[ADDRESS(id.x, id.y, id.z, size)];

				uint pos = 0;

				[unroll(NUMBER_OF_ELEMENTS - 1)]
				for (uint i = 1; i < NUMBER_OF_ELEMENTS; ++i)
				{
					if (cell.content[i] > cell.content[pos])
					{
						pos = i;
					}
				}

				return (pos + 1) * (cell.volume > 0.01);
			}

			[maxvertexcount(3)]
			void geom(point GS_INPUT p[1], inout TriangleStream<PS_INPUT> triStream)
			{
				PS_INPUT pIn = (PS_INPUT)0;

				float3 normal = cross(p[0].positions[2] - p[0].positions[1], p[0].positions[0] - p[0].positions[1]);
				normal = UnityObjectToWorldNormal(-normal);
				float NdotL = max(0, dot(normal, _WorldSpaceLightPos0.xyz));
				float light = _LightColor0 * NdotL;

				float4 temp, wpos;

				//First point
				pIn.position = UnityObjectToClipPos(p[0].positions[2]);
				pIn.color = float4(Colors[GetCellsElement(p[0].cells[2])].xyz * light, Colors[GetCellsElement(p[0].cells[2])].a);

				wpos = mul(unity_ObjectToWorld, p[0].positions[2]);
				temp.xyzw = wpos.xzxz * _WaveScale + _WaveOffset;
				pIn.bumpuv[0] = temp.xy * float2(.4, .45);
				pIn.bumpuv[1] = temp.wz;
				pIn.viewDir.xzy = normalize(WorldSpaceViewDir(p[0].positions[2]));

				triStream.Append(pIn);

				//Second point
				pIn.position = UnityObjectToClipPos(p[0].positions[1]);
				pIn.color = float4(Colors[GetCellsElement(p[0].cells[1])].xyz * light, Colors[GetCellsElement(p[0].cells[1])].a);

				wpos = mul(unity_ObjectToWorld, p[0].positions[1]);
				temp.xyzw = wpos.xzxz * _WaveScale + _WaveOffset;
				pIn.bumpuv[0] = temp.xy * float2(.4, .45);
				pIn.bumpuv[1] = temp.wz;
				pIn.viewDir.xzy = normalize(WorldSpaceViewDir(p[0].positions[1]));

				triStream.Append(pIn);

				//Third point
				pIn.position = UnityObjectToClipPos(p[0].positions[0]);
				pIn.color = float4(Colors[GetCellsElement(p[0].cells[0])].xyz * light, Colors[GetCellsElement(p[0].cells[0])].a);

				wpos = mul(unity_ObjectToWorld, p[0].positions[0]);
				temp.xyzw = wpos.xzxz * _WaveScale + _WaveOffset;
				pIn.bumpuv[0] = temp.xy * float2(.4, .45);
				pIn.bumpuv[1] = temp.wz;
				pIn.viewDir.xzy = normalize(WorldSpaceViewDir(p[0].positions[0]));

				triStream.Append(pIn);

				triStream.RestartStrip();
			}

			fixed4 frag(PS_INPUT input) : SV_Target
			{
				half3 bump1 = UnpackNormal(tex2D(_BumpMap, input.bumpuv[0])).rgb;
				half3 bump2 = UnpackNormal(tex2D(_BumpMap, input.bumpuv[1])).rgb;
				half3 bump = (bump1 + bump2) * 0.5;

				half fresnel = dot(input.viewDir, bump);
				half4 water = tex2D(_ColorControl, float2(fresnel,fresnel));

				half4 col;
				col.rgb = lerp(water.rgb, _horizonColor.rgb, water.a);
				col.a = _horizonColor.a;

				return input.color;
			}
			ENDCG
		}
	}
}
