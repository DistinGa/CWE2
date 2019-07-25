Shader "World Political Map/Unlit Marker" {
 
Properties {
    _Color ("Color", Color) = (1,1,1,0.5)
}
 
SubShader {
	Tags {
        "Queue"="Geometry-7"
        "RenderType"="Transparent"
    }
    Color [_Color]
   	Blend SrcAlpha OneMinusSrcAlpha
   	ZWrite Off
    Pass {
    }
  }
}
 
