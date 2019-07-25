Shader "World Political Map/Unlit Single Color Order 2" {
 
Properties {
    _Color ("Color", Color) = (1,1,1)
}
 
SubShader {
    ZWrite [_ZWrite]
    Tags {
        "Queue"="Geometry-8"
        "RenderType"="Opaque"
    }
    Pass {
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
			#if UNITY_REVERSED_Z
			v.vertex.z -= 0.001;
			#else
			v.vertex.z += 0.001;
			#endif	
		}
		
		fixed4 frag(AppData i) : SV_Target {
			return _Color;					
		}
			
		ENDCG
    }
   
}
 
}
