Shader "MarchingCubes/Multiple/Phong"
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
				half3 normal[3];
				int element;
			};

			//Thats the ouput of the Marching Cubes-algorithm Compute Shader
			StructuredBuffer<Triangle> mesh;

			uniform float4 _horizonColor;

			uniform float4 WaveSpeed;
			uniform float _WaveScale;
			uniform float4 _WaveOffset;

			sampler2D _BumpMap;
			sampler2D _ColorControl;
			sampler3D _MainTex;

			float _Shininess;

			float4 offset;
			float4 scale;

			struct VS_INPUT
			{
				uint vertexid : SV_VertexID;
			};

			struct GS_INPUT
			{
				float4 positions[3] : POSITION;
				float3 normals[3] : NORMAL;
				float4 color : COLOR;
			};

			struct PS_INPUT
			{
				float4 position : SV_POSITION;
				float4 wpos : POSITION2;
				float4 color : COLOR;
				float3 normal : NORMAL;
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

				o.color = Colors[mesh[input.vertexid].element];
				return o;
			}

			[maxvertexcount(3)]
			void geom(point GS_INPUT p[1], inout TriangleStream<PS_INPUT> triStream)
			{
				PS_INPUT pIn = (PS_INPUT)0;

				[unroll(3)]
				for (int i = 2; i >= 0; --i)
				{
					pIn.position = UnityObjectToClipPos(p[0].positions[i] * scale + offset);
					pIn.color = p[0].color;
					pIn.wpos = mul(unity_ObjectToWorld, p[0].positions[i] * scale + offset);
					pIn.normal = p[0].normals[i];
					triStream.Append(pIn);
				}

				triStream.RestartStrip();
			}

			fixed4 BlinnPhong(float4 position, float3 normal)
			{
				fixed3 normalDirection = normalize(normal);

				fixed3 viewDirection = normalize(
					_WorldSpaceCameraPos - position.xyz);
				fixed3 lightDirection;
				fixed attenuation;

				if (0.0 == _WorldSpaceLightPos0.w)
				{
					attenuation = 1.0;
					lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				}
				else
				{
					fixed3 vertexToLightSource =
						_WorldSpaceLightPos0.xyz - position.xyz;
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

			fixed4 frag(PS_INPUT input) : SV_Target
			{
				return input.color * BlinnPhong(input.wpos, input.normal);
			}
			ENDCG
		}
	}
}
