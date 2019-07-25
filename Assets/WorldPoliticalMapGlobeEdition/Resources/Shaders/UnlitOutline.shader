Shader "World Political Map/Unlit Outline" {
 
Properties {
    _Color ("Color", Color) = (1,1,1,1)
    _EdgeColor ("Relief Edge Color", Color) = (1,1,1,1)
}
 
SubShader {
    Tags {
       "Queue"="Geometry-6"
       "RenderType"="Opaque"
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
    
   // SECOND STROKE (RIGHT) ***********
 
    Pass {
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag				
		#include "UnityCG.cginc"

		fixed4 _Color, _EdgeColor;

		struct AppData {
			float4 vertex : POSITION;
		};

		void vert(inout AppData v) {
			v.vertex = UnityObjectToClipPos(v.vertex);
			v.vertex.x += 2 * (v.vertex.w/_ScreenParams.x);
			#if UNITY_REVERSED_Z
			v.vertex.z -= 0.001;
			#else
			v.vertex.z += 0.001;
			#endif		
		}
		
		fixed4 frag(AppData i) : SV_Target {
			return _EdgeColor;	// adds "relief touch"
		}
			
		ENDCG
    }
    
      // THIRD STROKE (UP) ***********
 
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
			v.vertex.y += 2 * (v.vertex.w/_ScreenParams.y);
#if UNITY_REVERSED_Z
			v.vertex.z -= 0.001;
#else
			v.vertex.z += 0.001;
#endif		
		}
		
		fixed4 frag(AppData i) : SV_Target {
			return _Color;						//Output RGBA color
		}
			
		ENDCG
    }
    
       
      // FOURTH STROKE (LEFT) ***********
 
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
			v.vertex.x -= 2 * (v.vertex.w/_ScreenParams.x);			
#if UNITY_REVERSED_Z
			v.vertex.z -= 0.001;
#else
			v.vertex.z += 0.001;
#endif		
		}
		
		fixed4 frag(AppData i) : SV_Target {
			return _Color;						//Output RGBA color
		}
			
		ENDCG
    }
    
    // FIFTH STROKE (DOWN) ***********
 
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
			v.vertex.y -= 2 * (v.vertex.w/_ScreenParams.y);		
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
