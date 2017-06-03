Shader "ImageEffects/Overlay"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Overlay("Overlay", 2D) = "white" {}
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			v2f_img vert(appdata_img v)
			{
				return vert_img(v);
			}
		
			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;
			sampler2D _Overlay;

			float3 uvToEye(float2 TexCoord, float depth)
			{
				float4 clipSpacePosition = float4(TexCoord, depth, 1.0);
				float4 viewSpacePosition = mul(unity_CameraInvProjection, clipSpacePosition);

				viewSpacePosition /= viewSpacePosition.w;

				float4 worldSpacePosition = mul(unity_MatrixInvV, viewSpacePosition);

				return worldSpacePosition.xyz;
			}

			fixed4 Shading(float depth, float2 texCoord)
			{
				// read eye-space depth from texture
				if (depth < 0.0001)
				{
					return 0;
				}

				// calculate eye-space position from depth
				float3 posEye = uvToEye(texCoord, depth);

				// calculate differences
				float3 ddx = uvToEye(texCoord + float2(1.0 / 512.0, 0), tex2D(_Overlay, texCoord + float2(1.0 / 512.0, 0)).y) - posEye;
				float3 ddx2 = posEye - uvToEye(texCoord - float2(1.0 / 512.0, 0), tex2D(_Overlay, texCoord - float2(1.0 / 512.0, 0)).y);

				if (abs(ddx.z) > abs(ddx2.z))
				{
					ddx = ddx2;
				}

				float3 ddy = uvToEye(texCoord + float2(0, 1.0 / 512.0), tex2D(_Overlay, texCoord + float2(0, 1.0 / 512.0)).y) - posEye;
				float3 ddy2 = posEye - uvToEye(texCoord - float2(0, 1.0 / 512.0), tex2D(_Overlay, texCoord - float2(0, 1.0 / 512.0)).y);

				if (abs(ddy2.z) < abs(ddy.z))
				{
					ddy = ddy2;
				}

				// calculate normal
				float3 n = cross(ddx, ddy);
				n = (normalize(n) + 1) / 2.0;

				if (length(n) > 0.1)
				{
					float diff = max(0.0, dot(n, _WorldSpaceLightPos0.xyz));
					return fixed4(diff, diff, diff, 1);
				}
				else
				{
					return 0;
				}
			}

			fixed4 frag (v2f_img i) : SV_Target
			{
				float cameraDepth = Linear01Depth(tex2D(_CameraDepthTexture, i.uv));
				float fluidDepth = tex2D(_Overlay, i.uv).x;

				fixed4 col = (tex2D(_MainTex, i.uv));

				if (cameraDepth > fluidDepth)
				{
					col = Shading(tex2D(_Overlay, i.uv).y, i.uv);
				}
				return col;
			}
			ENDCG
		}
	}
}
