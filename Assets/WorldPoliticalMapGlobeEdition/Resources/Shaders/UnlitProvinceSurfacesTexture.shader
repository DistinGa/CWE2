Shader "World Political Map/Unlit Province Surface Texture" {
 
Properties {
    _Color ("Color", Color) = (1,1,1)
    _MainTex ("Texture", 2D) = "white"
}
 
SubShader {
    Tags {
        "Queue"="Geometry-15"
        "RenderType"="Opaque"
    }
    	
  	Blend SrcAlpha OneMinusSrcAlpha
	ZWrite Off
	Pass {

    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag				
		
		#include "UnityCG.cginc"
		
		fixed4 _Color;
		sampler2D _MainTex;

		struct AppData {
			float4 vertex : POSITION;
			float4 uv : TEXCOORD0;
		};
		
		void vert(inout AppData v) {
			v.vertex = UnityObjectToClipPos(v.vertex);
		}
		
		fixed4 frag(AppData i) : SV_Target {
			return tex2D(_MainTex, i.uv) * _Color;
		}
			
		ENDCG
    }

}
 
}
