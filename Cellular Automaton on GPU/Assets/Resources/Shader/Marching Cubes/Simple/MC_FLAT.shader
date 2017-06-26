Shader "MarchingCubes/Flat"
{
	Properties
	{
		//Part of the wtare-shader from the standard assets
		_horizonColor("Horizon color", COLOR) = (.172 , .463 , .435 , 0)
		_WaveScale("Wave scale", Range(0.02,0.15)) = .07
		[NoScaleOffset] _ColorControl("Reflective color (RGB) fresnel (A) ", 2D) = "" { }
		[NoScaleOffset] _BumpMap("Waves Normalmap ", 2D) = "" { }
		WaveSpeed("Wave speed (map1 x,y; map2 x,y)", Vector) = (19,9,-16,-7)
		///////
		_MainTex("Texture", 3D) = "white" {}
	}

	SubShader
	{
		Tags{ "LightMode" = "ForwardBase" "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 100

		ZWrite Off
		Blend SrcAlpha  OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#include "UnityLightingCommon.cginc"
			#include "UnityCG.cginc"
			#pragma target 5.0
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag


			//Thats the ouput type of the Marching Cubes-algorithm Compute Shader
			struct Triangle
			{
				half3 vertex[3];
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
			};

			struct PS_INPUT
			{
				float4 position : SV_POSITION;
				float3 uv : TEXCOORDS;
				float4 light : COLOR;
		#ifdef REALISTIC
				float2 bumpuv[2] : TEXCOORD0;
				float3 viewDir : TEXCOORD2;
		#endif
			};

			sampler2D _BumpMap;
			sampler2D _ColorControl;
			sampler3D _MainTex;

			float4 offset;
			float4 scale;

			GS_INPUT vert(VS_INPUT input)
			{
				GS_INPUT o;
				o.positions[0] = float4(mesh[input.vertexid].vertex[0], 1);
				o.positions[1] = float4(mesh[input.vertexid].vertex[1], 1);
				o.positions[2] = float4(mesh[input.vertexid].vertex[2], 1);
				return o;
			}

			[maxvertexcount(3)]
			void geom(point GS_INPUT p[1], inout TriangleStream<PS_INPUT> triStream)
			{
				PS_INPUT pIn = (PS_INPUT)0;

		#ifdef REALISTIC
				fixed4 temp, wpos;
		#else
				fixed3 normal = cross(p[0].positions[2] - p[0].positions[1], p[0].positions[1] - p[0].positions[0]);
				normal = UnityObjectToWorldNormal(normal);
				fixed NdotL = max(0, dot(normal, _WorldSpaceLightPos0.xyz));
				fixed4 light = fixed4(_LightColor0.xyz * NdotL, 1);
		#endif

				[unroll(3)]
				for (int i = 2; i >= 0; --i)
				{
					pIn.position = UnityObjectToClipPos(p[0].positions[i] * scale + offset);
					pIn.uv = p[0].positions[i] + half3(1 / 64.0, 0, 1 / 64.0);
		#ifdef REALISTIC
					wpos = mul(unity_ObjectToWorld, p[0].positions[i] * scale + offset);
					temp.xyzw = wpos.xzxz * _WaveScale + _WaveOffset;
					pIn.bumpuv[0] = temp.xy * fixed2(.4, .45);
					pIn.bumpuv[1] = temp.wz;
					pIn.viewDir.xzy = normalize(WorldSpaceViewDir(p[0].positions[i] * scale + offset));
		#else
					pIn.light = light;
		#endif
					triStream.Append(pIn);
				}

				triStream.RestartStrip();
			}

			fixed4 frag(PS_INPUT input) : SV_Target
			{
				fixed4 col;

		#ifdef REALISTIC
				fixed3 bump1 = UnpackNormal(tex2D(_BumpMap, input.bumpuv[0])).rgb;
				fixed3 bump2 = UnpackNormal(tex2D(_BumpMap, input.bumpuv[1])).rgb;
				fixed3 bump = (bump1 + bump2) * 0.5;

				fixed fresnel = dot(input.viewDir, bump);
				fixed4 water = tex2D(_ColorControl, fixed2(fresnel, fresnel));

				col.rgb = tex3D(_MainTex, input.uv).xyz * lerp(water.rgb, _horizonColor.rgb, water.a);
				col.a = _horizonColor.a;
		#else			
				col = tex3D(_MainTex, input.uv) * input.light;
		#endif
				return col;
			}
			ENDCG
		}
	}
}
