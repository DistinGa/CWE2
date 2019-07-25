Shader "World Political Map/Unlit Province Surface Single Color" {
 
Properties {
    _Color ("Color", Color) = (1,1,1)
}
 
SubShader {
    Color [_Color]
        Tags {
        "Queue"="Geometry-15"
        "RenderType"="Opaque"
    	}
    Pass {
    	ZWrite [_ZWrite]
    }
}
 
}
