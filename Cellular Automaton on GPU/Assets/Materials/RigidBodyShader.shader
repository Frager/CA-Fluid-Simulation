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
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			struct RigidBody
			{
				float2 size;
				float3 position;
				float3x3 rotation;
			};

			StructuredBuffer<RigidBody> newPositions2;

			StructuredBuffer<float3> newPositions;
			
			v2f vert (appdata v)
			{
				v2f o;
				float4x4 transform = {	1,0,0,newPositions2[0].position.x,
										0,1,0,newPositions2[0].position.y,
										0,0,1,newPositions2[0].position.z,
										0,0,0,1 };

				float4 pos = v.vertex;
				pos.xz *= 10;
	
				pos.xyz = mul(newPositions2[0].rotation, pos.xyz);

				// Transform the position from object space to homogeneous projection space
				pos = mul(transform, pos);
				pos = mul(UNITY_MATRIX_V, pos);
				pos = mul(UNITY_MATRIX_P, pos);

				o.vertex = pos;
				//o.vertex = UnityObjectToClipPos(v.vertex + newPositions[0]);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = fixed4(1,0,0,1);
				return col;
			}
			ENDCG
		}
	}
}
