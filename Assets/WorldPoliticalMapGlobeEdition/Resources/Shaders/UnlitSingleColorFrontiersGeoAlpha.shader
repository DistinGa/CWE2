Shader "World Political Map/Unlit Single Color Frontiers Alpha Geo" {
 
Properties {
    _Color ("Color", Color) = (1,1,1,1)
    _Thickness ("Thickness", Float) = 0.01
}

SubShader {
    Tags {
      "Queue"="Geometry-8"
      "RenderType"="Opaque"
    }
    ZWrite [_ZWrite]
    Cull Off

    GrabPass { "_BackgroundTexture" }

      Pass {

	   	CGPROGRAM
		#pragma vertex vert	
		#pragma geometry geom
		#pragma fragment frag				
		#pragma target 4.0
		#include "UnityCG.cginc"
		
		fixed4 _Color;
		float _Thickness;
		sampler2D _BackgroundTexture;

		struct v2g {
			float4 vertex : POSITION;
		};

		struct g2f {
			float4 pos    : SV_POSITION;
			float4 grabPos : TEXCOORD0;
		};

		
		void vert(inout v2g v) {
			v.vertex = UnityObjectToClipPos(v.vertex);
			#if UNITY_REVERSED_Z
				v.vertex.z -= 0.001;
			#else
				v.vertex.z += 0.001;
			#endif
		}


		[maxvertexcount(6)]
        void geom(line v2g p[2], inout TriangleStream<g2f> outputStream) {
           float4 p0 = p[0].vertex;
           float4 p1 = p[1].vertex;

           float4 ab = p1 - p0;
           float4 normal = float4(-ab.y, ab.x, 0, 0);
           normal.xy = normalize(normal.xy) * _Thickness;
           float4 tl = p0 - normal;
           float4 bl = p0 + normal;
           float4 tr = p1 - normal;
           float4 br = p1 + normal;
  		   float4 dd = float4(normalize(p1.xy-p0.xy), 0, 0) * _Thickness;

           g2f pIn;
           pIn.pos = p0 - dd;
   		   pIn.grabPos = ComputeGrabScreenPos(pIn.pos);
           outputStream.Append(pIn);
           pIn.pos = bl;
		   pIn.grabPos = ComputeGrabScreenPos(pIn.pos);
           outputStream.Append(pIn);
           pIn.pos = tl;
           pIn.grabPos = ComputeGrabScreenPos(pIn.pos);
           outputStream.Append(pIn);
           pIn.pos = br;
           pIn.grabPos = ComputeGrabScreenPos(pIn.pos);
           outputStream.Append(pIn);
           pIn.pos = tr;
           pIn.grabPos = ComputeGrabScreenPos(pIn.pos);
           outputStream.Append(pIn);
           pIn.pos = p1 + dd;
           pIn.grabPos = ComputeGrabScreenPos(pIn.pos);
           outputStream.Append(pIn);
 		}
		
		fixed4 frag(g2f i) : SV_Target {
			fixed4 bgcolor = tex2Dproj(_BackgroundTexture, i.grabPos);
			return fixed4(_Color.rgb * _Color.a + bgcolor.rgb * (1.0 - _Color.a), _Color.a);
		}
		ENDCG
    }
}

SubShader {
    Color [_Color]
    ZWrite Off
    Tags {
		"Queue"="Geometry-8"
        "RenderType"="Opaque"
    }
    Blend SrcAlpha OneMinusSrcAlpha
    Pass {
    }
   
}
 
}
