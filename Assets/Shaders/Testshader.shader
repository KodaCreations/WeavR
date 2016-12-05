Shader "Custom/Testshader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ShipPos("Ship Pos", Vector) = (1, 1, 1, 1)
		_DistanceBeforeVisable("Distance Before visable", Float) = 20
	}
	SubShader
	{
		Tags { "Queue"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		LOD 1000

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float4 worldPos : POSITION1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _ShipPos;
			float _DistanceBeforeVisable;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.worldPos = mul(UNITY_MATRIX_MV, v.vertex);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				//float4 test = _ShipPos;
				float distanceToWall = distance(i.worldPos, _ShipPos);
				col.a = 1- distanceToWall / _DistanceBeforeVisable;
				// apply fog

				//UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
