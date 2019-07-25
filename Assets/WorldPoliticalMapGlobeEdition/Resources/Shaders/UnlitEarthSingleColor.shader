Shader "World Political Map/Unlit Earth Single Color" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white"
    }
    Category {
       Lighting On
	   ZWrite Off
       Tags {
       	"Queue"="Geometry-20" "RenderType"="Opaque" 
       }
       SubShader {
            Material {
               Emission [_Color]
            }
            Pass {
               SetTexture [_MainTex] {
                      Combine Texture * Primary, Texture * Primary
                }
            }
        } 
    }
}