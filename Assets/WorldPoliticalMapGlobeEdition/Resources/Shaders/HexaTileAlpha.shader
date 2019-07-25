Shader "World Political Map/HexaTileAlpha" {
    Properties {
        _Color ("Main Color", Color) = (1,0.5,0.5,1)
    }
    SubShader {
    	Tags { "Queue" = "Geometry-14" }
    	Blend SrcAlpha OneMinusSrcAlpha
    	Zwrite Off
       	Color [_Color]
        Lighting Off
        Fog { Mode Off }
        Pass {
        }
    }
}
