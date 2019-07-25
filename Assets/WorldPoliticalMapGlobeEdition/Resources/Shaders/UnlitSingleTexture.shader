Shader "World Political Map/Unlit Single Texture" {

Properties {
	_MainTex ("Texture", 2D) = "white"  {}
}
SubShader {
	Tags { "Queue"="Geometry-20" "RenderType"="Opaque" }
	ZWrite Off
	Pass {
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag
        #pragma fragmentoption ARB_precision_hint_fastest
        #include "UnityCG.cginc"

		sampler2D _MainTex;
		
		struct appdata {
			float4 vertex : POSITION;
			float2 texcoord: TEXCOORD0;
		};

		struct v2f {
			float4 pos : SV_POSITION;
			float2 uv: TEXCOORD0;
		};
		
		v2f vert(appdata v) {
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = v.texcoord;
			return o;
		}
		
		fixed4 frag(v2f i) : SV_Target {
			return tex2D(_MainTex, i.uv);
		}
			
		ENDCG
    }
  }  

	Fallback "Diffuse"
}
