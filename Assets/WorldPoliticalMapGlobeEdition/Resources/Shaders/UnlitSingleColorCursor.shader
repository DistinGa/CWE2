Shader "World Political Map/Unlit Single Color Cursor" {
 
Properties {
    _Color ("Color", Color) = (1,1,1)
}
 
SubShader {
    Tags {
        "Queue"="Transparent"
        "RenderType"="Transparent"
    }
    ZWrite [_ZWrite]
 	Pass {
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag				
		
		#include "UnityCG.cginc"
		
		fixed4 _Color;

		struct AppData {
			float4 vertex : POSITION;
			float4 scrPos : TEXCOORD0;
			float hidden  : TEXCOORD1;
		};
		
		void vert(inout AppData v) {
			float3 viewDir = ObjSpaceViewDir(v.vertex);
			v.hidden = dot(v.vertex.xyz, viewDir);
		
			v.vertex = UnityObjectToClipPos(v.vertex);
			v.scrPos = ComputeScreenPos(v.vertex);
		}
		
		fixed4 frag(AppData i) : SV_Target {
			// Compute view from camera
			clip(i.hidden);
			float2 wcoord = (i.scrPos.xy/i.scrPos.w);
			float grad = abs(ddy(wcoord.y) / ddx(wcoord.x));
			float xm = lerp (wcoord.x, wcoord.y, grad > 0.5);
			xm = fmod( floor(xm * 1000.0) / 4,2);
			clip( xm - 0.5 );
			return _Color;					
		}
			
		ENDCG
    }

}
}
 
