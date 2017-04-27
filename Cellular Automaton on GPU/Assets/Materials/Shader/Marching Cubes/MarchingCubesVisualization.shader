Shader "Unlit/MarchingCubesVisualization"
{
	Properties
	{
		_horizonColor("Horizon color", COLOR) = (.172 , .463 , .435 , 0)
		_WaveScale("Wave scale", Range(0.02,0.15)) = .07
		[NoScaleOffset] _ColorControl("Reflective color (RGB) fresnel (A) ", 2D) = "" { }
		[NoScaleOffset] _BumpMap("Waves Normalmap ", 2D) = "" { }
		WaveSpeed("Wave speed (map1 x,y; map2 x,y)", Vector) = (19,9,-16,-7)
		_MainTex("Texture", 3D) = "white" {}
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

			struct Triangle
			{
				float3 vertex[3];
			};

			StructuredBuffer<Triangle> triangles;

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
			};

			struct PS_INPUT
			{
				float4 position : SV_POSITION;
				float light : BLENDINDICES;
				float3 uv : TEXCOORDS;
				float2 bumpuv[2] : TEXCOORD0;
				float3 viewDir : TEXCOORD2;
			};

			sampler2D _BumpMap;
			sampler2D _ColorControl;
			sampler3D _MainTex;
			float4 _MainTex_ST;

			float scale;
			int size;

			GS_INPUT vert(VS_INPUT input)
			{
				GS_INPUT o;
				o.positions[0] = float4(triangles[input.vertexid].vertex[0], 1);
				o.positions[1] = float4(triangles[input.vertexid].vertex[1], 1);
				o.positions[2] = float4(triangles[input.vertexid].vertex[2], 1);
				return o;
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

				pIn.position = UnityObjectToClipPos(p[0].positions[2]);
				pIn.light = light;
				pIn.uv = p[0].positions[2] / (size * scale);

				wpos = mul(unity_ObjectToWorld, p[0].positions[2]);
				temp.xyzw = wpos.xzxz * _WaveScale + _WaveOffset;
				pIn.bumpuv[0] = temp.xy * float2(.4, .45);
				pIn.bumpuv[1] = temp.wz;
				pIn.viewDir.xzy = normalize(WorldSpaceViewDir(p[0].positions[2]));

				triStream.Append(pIn);

				pIn.position = UnityObjectToClipPos(p[0].positions[1]);
				pIn.light = light;
				pIn.uv = p[0].positions[1] / (size * scale);

				wpos = mul(unity_ObjectToWorld, p[0].positions[1]);
				temp.xyzw = wpos.xzxz * _WaveScale + _WaveOffset;
				pIn.bumpuv[0] = temp.xy * float2(.4, .45);
				pIn.bumpuv[1] = temp.wz;
				pIn.viewDir.xzy = normalize(WorldSpaceViewDir(p[0].positions[1]));

				triStream.Append(pIn);

				pIn.position = UnityObjectToClipPos(p[0].positions[0]);
				pIn.light = light;
				pIn.uv = p[0].positions[0] / (size * scale);

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

				return col * tex3D(_MainTex, input.uv);
			}
			ENDCG
		}
	}
}
