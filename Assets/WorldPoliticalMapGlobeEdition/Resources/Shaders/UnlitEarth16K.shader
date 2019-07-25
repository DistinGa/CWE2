Shader "World Political Map/Unlit Texture 16K" {
 
Properties {
    _Color ("Color", Color) = (1,1,1)
	_TexTL ("Tex TL", 2D) = "white" {}
	_TexTR ("Tex TR", 2D) = "white" {}
	_TexBL ("Tex BL", 2D) = "white" {}
	_TexBR ("Tex BR", 2D) = "white" {}
}
 
SubShader {
  	Tags { "Queue"="Geometry-20" "RenderType"="Opaque" }
		Lighting Off
		ZWrite Off
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				
				#include "UnityCG.cginc"
				
				sampler2D _TexTL;
				sampler2D _TexTR;
				sampler2D _TexBL;
				sampler2D _TexBR;

				struct v2f {
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
				};
        
				v2f vert (appdata_base v) {
					v2f o;
					o.pos 				= UnityObjectToClipPos (v.vertex);
					o.uv				= v.texcoord;
					return o;
				 }
				
				half4 frag (v2f i) : SV_Target {
					half4 color;
					// compute Earth pixel color
					if (i.uv.x<0.5) {
						if (i.uv.y>0.5) {
							color = tex2D(_TexTL, float2(i.uv.x * 2.0, (i.uv.y - 0.5) * 2.0));
						} else {
							color = tex2D(_TexBL, float2(i.uv.x * 2.0, i.uv.y * 2.0));
						}
					} else {
						if (i.uv.y>0.5) {
							color = tex2D(_TexTR, float2((i.uv.x - 0.5) * 2.0f, (i.uv.y - 0.5) * 2.0));
						} else {
							color = tex2D(_TexBR, float2((i.uv.x - 0.5) * 2.0f, i.uv.y * 2.0));
						}
					}

		  		   return color;
				}
			
			ENDCG
		}
	}
}