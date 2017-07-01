// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "MarchingCubes/Gouraud"
{
	Properties
	{
		_MainTex("Texture", 3D) = "white" {}
		_Shininess("Shininess", Float) = 50
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
				half3 normal[3];
			};

			//Thats the ouput of the Marching Cubes-algorithm Compute Shader
			StructuredBuffer<Triangle> mesh;

			uniform float4 _horizonColor;

			uniform float4 WaveSpeed;
			uniform float _WaveScale;
			uniform float4 _WaveOffset;

			sampler3D _MainTex;

			float _Shininess;

			float4 offset;
			float4 scale;
			float2 dimensions;

			struct VS_INPUT
			{
				uint vertexid : SV_VertexID;
			};

			struct GS_INPUT
			{
				float4 positions[3] : POSITION;
				float3 normals[3] : NORMAL;
			};

			struct PS_INPUT
			{
				float4 position : SV_POSITION;
				float3 uv : TEXCOORDS;
				float4 light : COLOR;
				half3 worldRefl : TEXCOORD0;
				half3 worldRefr : TEXCOORD1;
			};


			GS_INPUT vert(VS_INPUT input)
			{
				GS_INPUT o;
				o.positions[0] = float4(mesh[input.vertexid].vertex[0], 1);
				o.positions[1] = float4(mesh[input.vertexid].vertex[1], 1);
				o.positions[2] = float4(mesh[input.vertexid].vertex[2], 1);

				o.normals[0] = UnityObjectToWorldNormal(mesh[input.vertexid].normal[0]);
				o.normals[1] = UnityObjectToWorldNormal(mesh[input.vertexid].normal[1]);
				o.normals[2] = UnityObjectToWorldNormal(mesh[input.vertexid].normal[2]);
				return o;
			}

			fixed4 BlinnPhong(float4 position, float3 normal)
			{
				fixed3 normalDirection = normalize(normal);

				fixed3 viewDirection = normalize(UnityWorldSpaceViewDir(position));
				fixed3 lightDirection;
				fixed attenuation;

				if (0.0 == _WorldSpaceLightPos0.w)
				{
					attenuation = 1.0;
					lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				}
				else
				{
					fixed3 vertexToLightSource =_WorldSpaceLightPos0.xyz - position.xyz;
					fixed distance = length(vertexToLightSource);
					attenuation = 1.0 / distance;
					lightDirection = normalize(vertexToLightSource);
				}

				fixed3 ambientLighting = UNITY_LIGHTMODEL_AMBIENT.rgb;

				fixed3 diffuseReflection = attenuation * _LightColor0.rgb * max(0.0, dot(normalDirection, lightDirection));

				fixed3 specularReflection;
				if (dot(normalDirection, lightDirection) < 0.0)
				{
					specularReflection = fixed3(0.0, 0.0, 0.0);
				}
				else
				{
					specularReflection = attenuation * _LightColor0.rgb * pow(max(0.0, dot(
						reflect(-lightDirection, normalDirection),
						viewDirection)), _Shininess);
				}

				return fixed4(ambientLighting + diffuseReflection + specularReflection, 1.0);
			}

			[maxvertexcount(3)]
			void geom(point GS_INPUT p[1], inout TriangleStream<PS_INPUT> triStream)
			{
				PS_INPUT pIn = (PS_INPUT)0;

				float3 worldViewDir, worldNormal;
				[unroll(3)]
				for (int i = 2; i >= 0; --i)
				{
					pIn.position = UnityObjectToClipPos(p[0].positions[i] * scale + offset);
					pIn.uv = p[0].positions[i] + half3(dimensions.x, 0, dimensions.z);
					pIn.light = BlinnPhong(mul(p[0].positions[i] * scale, unity_ObjectToWorld), p[0].normals[i]);

					triStream.Append(pIn);
				}

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
