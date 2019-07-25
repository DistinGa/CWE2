Shader "World Political Map/Unlit Marker Line" {
 
Properties {
    _Color ("Color", Color) = (1,1,1,0.5)
    _MainTex("Texture (RGBA)", 2D) = "white" {}
}
 
SubShader {
         Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

	     Color [_Color]
	     Cull Off
         ZWrite Off
         Blend SrcAlpha OneMinusSrcAlpha 
         ColorMask RGB
             
	Pass {
 		SetTexture [_MainTex] {
           	Combine Texture * Primary, Texture * Primary
        }
    }
 }

}
 
