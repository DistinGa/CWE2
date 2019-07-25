Shader "World Political Map/BackFacesForZBuffer" {

Properties {
	_Color ("Default Color", Color) = (1,1,1,1)
	_Inverted ("Inverted Mode", Float) = 0
}
SubShader {
	Tags { "Queue"="Geometry-21" "RenderType"="Opaque" }
	Offset -1, -1
	Cull Front
	ZWrite On
	Pass {
	
	    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag				
		
		#include "UnityCG.cginc"
		
		fixed4 _Color;
		fixed _Inverted;

		struct AppData {
			float4 vertex : POSITION;
			float3 viewDir : TEXCOORD0;
			float3 orgVert : TEXCOORD1;
		};
		
		void vert(inout AppData v) {
			v.orgVert = v.vertex.xyz;
			v.viewDir = ObjSpaceViewDir(v.vertex);
			v.vertex = UnityObjectToClipPos(v.vertex);
			if (_Inverted) {
#if UNITY_REVERSED_Z
				v.vertex.z -= 0.001;
#else
				v.vertex.z += 0.001;
#endif	
			}
		}
		
		fixed4 frag(AppData i) : SV_Target {
			clip(-dot(i.orgVert, i.viewDir));
			return _Color;
		}
			
		ENDCG
    }
    
}
}
