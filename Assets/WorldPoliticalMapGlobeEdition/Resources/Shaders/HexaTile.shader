Shader "World Political Map/HexaTile" {
    Properties {
        _Color ("Main Color", Color) = (1,0.5,0.5,1)
    }
    SubShader {
    	Tags { "Queue" = "Geometry-14" }
//    	Zwrite Off
		ZWrite [_ZWrite]
       	Color [_Color]
        Lighting Off
        Fog { Mode Off }
        Pass {
        }
    }
}
