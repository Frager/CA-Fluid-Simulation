Shader "MarchingCubes/Tesselation"
{
	Properties
	{
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
				return o;
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
						viewDirection)), 48);
				}

				return fixed4(ambientLighting + diffuseReflection + specularReflection, 1.0);
			}

			[maxvertexcount(9)]
			void geom(point GS_INPUT p[1], inout TriangleStream<PS_INPUT> triStream)
			{
				PS_INPUT pIn = (PS_INPUT)0;

				fixed3 normal = normalize(cross(p[0].positions[2] - p[0].positions[1], p[0].positions[1] - p[0].positions[0]));
				fixed4 mean = (p[0].positions[2] + p[0].positions[1] + p[0].positions[0]) / 3.0;

				fixed4 pointZero = mean + fixed4(0.003 * normal, 1);
				fixed4 light;

				normal = normalize(cross(p[0].positions[2] - p[0].positions[1], p[0].positions[1] - pointZero));
				light = BlinnPhong((p[0].positions[2] + p[0].positions[1] + pointZero) / 3.0, normal);
	
				pIn.position = UnityObjectToClipPos(p[0].positions[2] * scale + offset);
				pIn.uv = p[0].positions[2];
				pIn.light = light;
				triStream.Append(pIn);

				pIn.position = UnityObjectToClipPos(p[0].positions[1] * scale + offset);
				pIn.uv = p[0].positions[1];
				pIn.light = light;
				triStream.Append(pIn);

				pIn.position = UnityObjectToClipPos(pointZero * scale + offset);
				pIn.uv = pointZero;
				pIn.light = light;
				triStream.Append(pIn);

				triStream.RestartStrip();



				normal = normalize(cross(p[0].positions[1] - p[0].positions[0], p[0].positions[1] - pointZero));
				light = BlinnPhong((p[0].positions[0] + p[0].positions[1] + pointZero) / 3.0, normal);

				pIn.position = UnityObjectToClipPos(p[0].positions[1] * scale + offset);
				pIn.uv = p[0].positions[1];
				pIn.light = light;
				triStream.Append(pIn);

				pIn.position = UnityObjectToClipPos(p[0].positions[0] * scale + offset);
				pIn.uv = p[0].positions[0];
				pIn.light = light;
				triStream.Append(pIn);

				pIn.position = UnityObjectToClipPos(pointZero * scale + offset);
				pIn.uv = pointZero;
				pIn.light = light;
				triStream.Append(pIn);

				triStream.RestartStrip();



				normal = normalize(cross(p[0].positions[0] - p[0].positions[2], p[0].positions[2] - pointZero));
				light = BlinnPhong((p[0].positions[0] + p[0].positions[2] + pointZero) / 3.0, normal);

				pIn.position = UnityObjectToClipPos(pointZero * scale + offset);
				pIn.uv = pointZero;
				pIn.light = light;
				triStream.Append(pIn);

				pIn.position = UnityObjectToClipPos(p[0].positions[0] * scale + offset);
				pIn.uv = p[0].positions[0];
				pIn.light = light;
				triStream.Append(pIn);

				pIn.position = UnityObjectToClipPos(p[0].positions[2] * scale + offset);
				pIn.uv = p[0].positions[2];
				pIn.light = light;
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
