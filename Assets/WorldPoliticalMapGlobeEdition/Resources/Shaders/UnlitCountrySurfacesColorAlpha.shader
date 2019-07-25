﻿Shader "World Political Map/Unlit Country Surface Single Color Alpha" {
 
Properties {
    _Color ("Color", Color) = (1,1,1)
}
 
SubShader {
    Tags {
        "Queue"="Geometry-17"
        "RenderType"="Transparent"
    	}
    Color [_Color]
    Blend SrcAlpha OneMinusSrcAlpha
    Pass {
    ZWrite Off
    }
}
 
}
