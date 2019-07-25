Shader "World Political Map/Unlit Country Surface Single Color" {
 
Properties {
    _Color ("Color", Color) = (1,1,1)
}
 
SubShader {
    Color [_Color]
        Tags {
        "Queue"="Geometry-17"
        "RenderType"="Opaque"
    	}
   	ZWrite [_ZWrite]
    Pass {
    }
}
 
}
