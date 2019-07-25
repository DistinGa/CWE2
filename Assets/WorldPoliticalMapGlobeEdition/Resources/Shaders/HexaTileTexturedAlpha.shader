Shader "World Political Map/HexaTileTexturedAlpha" {
    Properties {
        _Color ("Main Color", Color) = (1,0.5,0.5,1)
        _MainTex ("Texture", 2D) = "white"
    }
    SubShader {
    	Tags { "Queue" = "Geometry-14" }
        Pass {
        	ZWrite Off
	    	Blend SrcAlpha OneMinusSrcAlpha
        	Color [_Color]
            Lighting Off
            Fog { Mode Off }
			SetTexture [_MainTex] {
            	Combine Texture * Primary, Texture * Primary
        	}
        }
    }
}