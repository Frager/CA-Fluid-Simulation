// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "ImageEffects/Overlay"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Overlay("Overlay", 2D) = "white" {}
		_Shininess("Shininess", Float) = 50
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"

			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;
			sampler2D _Overlay;

			float _Shininess;

			float4x4 clipToWorld;
			float4x4 viewProj;

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float3 worldDirection : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			v2f_img vert(appdata_img v)
			{
				return vert_img(v);
			}
	
			float3 uvToWorld(float2 texCoord)
			{
				float3 worldDirection = mul(clipToWorld, float4(texCoord, 0.0, 1.0)) - _WorldSpaceCameraPos;

				float origDepth = (((1 / tex2D(_Overlay, texCoord).x) - _ZBufferParams.y)) / _ZBufferParams.x;
				return worldDirection * LinearEyeDepth(origDepth) + _WorldSpaceCameraPos;
			}

			float3 Normal(float2 texCoord)
			{
				float3 posEye = uvToWorld(texCoord);

				float3 ddx = uvToWorld(texCoord + float2(1.0 / 512.0, 0)) - posEye;
				float3 ddx2 = posEye - uvToWorld(texCoord - float2(1.0 / 512.0, 0));

				if (abs(ddx.z) > abs(ddx2.z))
				{
					ddx = ddx2;
				}

				float3 ddy = uvToWorld(texCoord + float2(0, 1.0 / 512.0)) - posEye;
				float3 ddy2 = posEye - uvToWorld(texCoord - float2(0, 1.0 / 512.0));

				if (abs(ddy2.z) < abs(ddy.z))
				{
					ddy = ddy2;
				}

				float3 n = cross(ddx, ddy);
				n = normalize(n);

				//float3 ddxA = uvToWorld(texCoord + float2(1.0 / 512.0, 0));
				//float3 ddxB = uvToWorld(texCoord - float2(1.0 / 512.0, 0));
				//float3 ddx = ddxA - ddxB;
				//float3 ddyA = uvToWorld(texCoord + float2(0, 1.0 / 512.0));
				//float3 ddyB = uvToWorld(texCoord - float2(0, 1.0 / 512.0));
				//float3 ddy = ddyA - ddyB;
				//float3 n = cross(ddx, ddy);
				//n = normalize(n);

				return n;
			}

			fixed4 BlinnPhong(float3 position, float3 normal)
			{
				fixed3 viewDirection = normalize(_WorldSpaceCameraPos - position);
				fixed3 lightDirection;
				fixed attenuation;
				if (0.0 == _WorldSpaceLightPos0.w)
				{
					attenuation = 1.0;
					lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				}
				else
				{
					fixed3 vertexToLightSource = _WorldSpaceLightPos0.xyz - position;
					fixed distance = length(vertexToLightSource);
					attenuation = 1.0 / distance;
					lightDirection = normalize(vertexToLightSource);
				}
				fixed3 ambientLighting = UNITY_LIGHTMODEL_AMBIENT.rgb;
				fixed3 diffuseReflection = attenuation * _LightColor0.rgb * max(0.0, dot(normal, lightDirection));
				fixed3 specularReflection;
				if (dot(normal, lightDirection) < 0.0)
				{
					specularReflection = fixed3(0.0, 0.0, 0.0);
				}
				else
				{
					specularReflection = attenuation * _LightColor0.rgb * pow(max(0.0, dot(
						reflect(-lightDirection, normal),
						viewDirection)), _Shininess);
				}
				return fixed4(ambientLighting + diffuseReflection + specularReflection, 1.0);
			}

			fixed4 frag (v2f_img i) : SV_Target
			{
				float cameraDepth = Linear01Depth(tex2D(_CameraDepthTexture, i.uv));
				float fluidDepth = tex2D(_Overlay, i.uv).x;

				fixed4 col = tex2D(_MainTex, i.uv);

				if (cameraDepth > fluidDepth)
				{
					float3 worldPos = uvToWorld(i.uv);
					float3 normal = Normal(i.uv);
					float3 worldDirection = normalize(mul(clipToWorld, float4(i.uv, 0.0, 1.0)) - _WorldSpaceCameraPos);

					float angle = acos(dot(normal, worldDirection));

					col = 0.2 * tex2D(_MainTex, i.uv + float2(0.01, 0.01) * angle) + 0.8 * BlinnPhong(worldPos, normal);
				}
				return col;
			}
			ENDCG
		}
	}
}
