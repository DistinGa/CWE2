Shader "World Political Map/UnlitGlobeClip"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color (RGB)", Color) = (1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

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
				float3 worldPos: TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _WPM_GlobePos;
			fixed4 _Color;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float dist = dot(i.worldPos - _WPM_GlobePos.xyz, i.worldPos - _WPM_GlobePos.xyz);
				clip(dist - _WPM_GlobePos.w);

				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * _Color;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}

  		Pass
         {
             Name "ShadowCaster"
             Tags { "LightMode" = "ShadowCaster" }
           
             Fog { Mode Off }

             CGPROGRAM
 
             #pragma vertex vert
             #pragma fragment frag
             #pragma multi_compile_shadowcaster
             #pragma fragmentoption ARB_precision_hint_fastest
             
             #include "UnityCG.cginc"
 
             struct v2f
             { 
                 V2F_SHADOW_CASTER;
             };
           
             v2f vert(appdata_base v)
             {
                 v2f o;
                 TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                 return o;
             }
           
             float4 frag(v2f i) : COLOR
             {
                 SHADOW_CASTER_FRAGMENT(i)
             }
 
             ENDCG
          }

	}
}
