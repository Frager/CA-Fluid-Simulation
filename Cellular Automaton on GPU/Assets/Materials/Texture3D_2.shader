Shader "Unlit/Texture3D_2"
{
	Properties
	{
		_MainTex("Texture", 3D) = "white" {}
		_UVCoord("UV Coordinate", Vector) = (0,0,0,0)
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
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
		};

		struct v2f
		{
			float4 vertex : SV_POSITION;
		};

		sampler3D _MainTex;
		float4 _UVCoord;

		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);

			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			fixed4 col = tex3D(_MainTex, _UVCoord);

			return col;
		}

		ENDCG
		}
	}
}
