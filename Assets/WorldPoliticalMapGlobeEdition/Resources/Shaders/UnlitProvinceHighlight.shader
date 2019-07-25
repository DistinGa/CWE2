Shader "World Political Map/Unlit Province Highlight" {
Properties {
    _Color ("Tint Color", Color) = (1,1,1,0.5)
    _Intensity ("Intensity", Range(0.0, 2.0)) = 1.0
    _MainTex ("Texture", 2D) = "white" {}
}
SubShader {
    Tags {
        "Queue"="Geometry-11"
        "IgnoreProjector"="True"
        "RenderType"="Transparent"
    }
			Cull Off
			ZWrite Off
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha 
		Pass {
			CGPROGRAM	
			#pragma fragment frag
			#pragma vertex vert	
			#include "UnityCG.cginc"

			fixed4 _Color;	
			fixed _Intensity;
			sampler2D _MainTex;

			struct AppData {
				float4 vertex : POSITION;
				float2 texcoord: TEXCOORD0;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv: TEXCOORD0;
			};

			v2f vert(AppData v) {
				v2f o;						
				o.pos = UnityObjectToClipPos(v.vertex);	
				o.uv = v.texcoord;
				return o;								
			}

			fixed4 frag(v2f i) : SV_Target {
				fixed4 pixel = tex2D(_MainTex, i.uv);
				return pixel * _Color * _Intensity;			
			}

			ENDCG

		}
	}	
}
