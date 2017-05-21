Shader "Unlit/RigidBodyShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc" 
			#include "UnityLightingCommon.cginc" 

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				fixed4 diff : COLOR0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			struct RigidBody
			{
				float3 scale;
				float3 position;
				//float2 rotation;
			};

			StructuredBuffer<RigidBody> newPositions2;

			StructuredBuffer<float3> newPositions;
			
			v2f vert (appdata_base v)
			{
				v2f o;
				float4x4 transform = { newPositions2[0].scale.x,0,0,newPositions2[0].position.x,
										0,newPositions2[0].scale.y,0,newPositions2[0].position.y,
										0,0,newPositions2[0].scale.z,newPositions2[0].position.z,
										0,0,0,1 };

				float4 pos = v.vertex;

				//float2 rot = float2(radians(newPositions2[0].rotation.x), radians(newPositions2[0].rotation.y));

				//float3x3 transformY = { cos(rot.x), sin(rot.x), 0,
				//						-sin(rot.x), cos(rot.x), 0,
				//						0, 0, 1 };

				//float3x3 transformZ = { cos(rot.y), 0, -sin(rot.y),
				//						0, 1, 0,
				//						sin(rot.y), 0, cos(rot.y) };

				//pos.xyz = mul(transformY, pos.xyz);
				//pos.xyz = mul(transformZ, pos.xyz);
	
				// Transform the position from object space to homogeneous projection space
				pos = mul(transform, pos);
				pos = mul(UNITY_MATRIX_V, pos);
				pos = mul(UNITY_MATRIX_P, pos);

				o.vertex = pos;
				o.uv = v.texcoord;


				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				// dot product between normal and light direction for
				// standard diffuse (Lambert) lighting
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				// factor in the light color
				o.diff = nl * _LightColor0;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				col *= i.diff;
				return col;
			}
			ENDCG
		}
	}
}
