Shader "World Political Map/HexaGridNoExtrusion" {
    Properties {
        _Color ("Color", Color) = (1,0.5,0.5,1)
    }
    SubShader {
	Tags { "RenderType"="Opaque" "Queue"="Geometry-11" }
	Pass {
		ZWrite [_ZWrite]
	
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag				
		
		#include "UnityCG.cginc"
		
		fixed4 _Color;

		struct AppData {
			float4 vertex : POSITION;
		};
		
		void vert(inout AppData v) {
			v.vertex = UnityObjectToClipPos(v.vertex);
		}
		
		fixed4 frag(AppData i) : SV_Target {
			return _Color;					
		}
			
		ENDCG
    }
    }
}