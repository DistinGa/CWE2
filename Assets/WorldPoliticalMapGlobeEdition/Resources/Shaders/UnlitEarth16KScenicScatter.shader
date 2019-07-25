Shader "World Political Map/Unlit Earth 16K Scenic Scatter" 
{
	Properties {
		_TexTL ("Tex TL", 2D) = "white" {}
		_TexTR ("Tex TR", 2D) = "white" {}
		_TexBL ("Tex BL", 2D) = "white" {}
		_TexBR ("Tex BR", 2D) = "white" {}
		[Normal] _NormalMap ("Normal Map", 2D) = "bump" {}
		_BumpAmount ("Bump Amount", Range(0, 1)) = 0.5
		_CloudMap ("Cloud Map", 2D) = "black" {}
		_CloudSpeed ("Cloud Speed", Range(-1, 1)) = -0.04
		_CloudAlpha ("Cloud Alpha", Range(0, 1)) = 1
		_CloudShadowStrength ("Cloud Shadow Strength", Range(0, 1)) = 0.2
		_CloudElevation ("Cloud Elevation", Range(0.001, 0.1)) = 0.003
		_Brightness("Brightness", Range(1,3)) = 1.25
		_Contrast("Contrast", Range(0,2)) = 1.1
		_AmbientLight("Ambient Light", Range(0,1)) = 0.1
		_SunLightDirection("Sun Light Direction", Vector) = (0,0,1)
		[HideInInspector] _fOuterRadius ("Outer Radius", Float) = 1.25
		[HideInInspector] fOuterRadius2 ("Outer Radius Squared", Float) = 1.5625
		[HideInInspector] fInnerRadius ("Inner Radius", Float) = 1
		[HideInInspector] fInnerRadius2 ("Inner Radius Squared", Float) = 1
		[HideInInspector] fKrESun ("", Float) = 1.0
		[HideInInspector] fKmESun ("", Float) = 1.0
		[HideInInspector] fKr4PI ("", Float) = 1.0
		[HideInInspector] fKm4PI ("", Float) = 1.0
		[HideInInspector] fScale ("", Float) = 1.0
		[HideInInspector] fScaleDepth ("", Float) = 1.0
		[HideInInspector] fScaleOverScaleDepth ("", Float) = 1.0
		[HideInInspector] fHdrExposure ("", Float) = 1.0

	}
	SubShader 
	{
		Tags { "Queue"="Geometry-20" "RenderType"="Opaque" }
		ZWrite Off
    	Pass 
    	{
    		
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ WPM_BUMPMAP_ENABLED
			#pragma multi_compile __ WPM_CLOUDSHADOWS_ENABLED

			sampler2D _TexTL;
			sampler2D _TexTR;
			sampler2D _TexBL;
			sampler2D _TexBR;
			sampler2D _NormalMap;
			sampler2D _CloudMap;
		
			uniform float3 _SunLightDirection;		// The direction vector to the light source
			uniform float3 v3InvWavelength; // 1 / pow(wavelength, 4) for the red, green, and blue channels
			uniform float fOuterRadius;		// The outer (atmosphere) radius
			uniform float fOuterRadius2;	// fOuterRadius^2
			uniform float fInnerRadius;		// The inner (planetary) radius
			uniform float fInnerRadius2;	// fInnerRadius^2
			uniform float fKrESun;			// Kr * ESun
			uniform float fKmESun;			// Km * ESun
			uniform float fKr4PI;			// Kr * 4 * PI
			uniform float fKm4PI;			// Km * 4 * PI
			uniform float fScale;			// 1 / (fOuterRadius - fInnerRadius)
			uniform float fScaleDepth;		// The scale depth (i.e. the altitude at which the atmosphere's average density is found)
			uniform float fScaleOverScaleDepth;	// fScale / fScaleDepth
			uniform float fHdrExposure;		// HDR exposure
			uniform float _BumpAmount;		// Normal/Bump effect amount (0..1)
			uniform	float _CloudSpeed;
			uniform float _CloudAlpha;
			uniform float _CloudShadowStrength;
			uniform float _CloudElevation;
			uniform float _Brightness;
			uniform float _Contrast;
			uniform float _AmbientLight;

			struct v2f 
			{
    			float4 pos : SV_POSITION;
    			float2 uv : TEXCOORD0;
    			float3 c0 : COLOR0;
    			float3 c1 : COLOR1;
    			float3 viewDir: TEXCOORD1;
    			float3 normal: NORMAL;
    			#if WPM_BUMPMAP_ENABLED
				float3 tspace0 : TEXCOORD2; // tangent.x, bitangent.x, normal.x
               	float3 tspace1 : TEXCOORD3; // tangent.y, bitangent.y, normal.y
               	float3 tspace2 : TEXCOORD4; // tangent.z, bitangent.z, normal.z
               	#endif
			};
			
			float scale(float fCos)
			{
				float x = 1.0 - fCos;
				return fScaleDepth * exp(-0.00287 + x*(0.459 + x*(3.83 + x*(-6.80 + x*5.25))));
			}

			v2f vert (float4 vertex : POSITION, float3 normal : NORMAL, float4 tangent : TANGENT, float2 uv : TEXCOORD0)
			{
				float3 center = float3(unity_ObjectToWorld[0][3], unity_ObjectToWorld[1][3], unity_ObjectToWorld[2][3]);
			    float3 v3CameraPos = _WorldSpaceCameraPos - center;	// The camera's current position
				float fCameraHeight = length(v3CameraPos);					// The camera's current height
				float fCameraHeight2 = fCameraHeight*fCameraHeight;			// fCameraHeight^2
				
				// Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
				float3 v3Pos = mul(unity_ObjectToWorld, vertex).xyz - center;
				float3 v3Ray = v3Pos - v3CameraPos;
				float fFar = length(v3Ray);
				v3Ray /= fFar;
				
				// Calculate the closest intersection of the ray with the outer atmosphere (which is the near point of the ray passing through the atmosphere)
				float B = 2.0 * dot(v3CameraPos, v3Ray);
				float C = fCameraHeight2 - fOuterRadius2;
				float fDet = max(0.0, B*B - 4.0 * C);
				float fNear = 0.5 * (-B - sqrt(fDet));
				
				// Calculate the ray's starting position, then calculate its scattering offset
				float3 v3Start = v3CameraPos + v3Ray * fNear;
				fFar -= fNear;
				float fDepth = exp((fInnerRadius - fOuterRadius) / fScaleDepth);
				float fCameraAngle = dot(-v3Ray, v3Pos) / length(v3Pos);
				float v3PosLength = length(v3Pos);
				float fLightAngle = dot(_SunLightDirection, v3Pos) / v3PosLength;
				float fCameraScale = scale(fCameraAngle);
				float fLightScale = scale(fLightAngle);
				float fCameraOffset = fDepth*fCameraScale;
				float fTemp = (fLightScale + fCameraScale);
				
				const float fSamples = 2.0;
				
				// Initialize the scattering loop variables
				float fSampleLength = fFar / fSamples;
				float fScaledLength = fSampleLength * fScale;
				float3 v3SampleRay = v3Ray * fSampleLength;
				float3 v3SamplePoint = v3Start + v3SampleRay * 0.5;
				
				// Now loop through the sample rays
				float3 v3FrontColor = float3(0.0, 0.0, 0.0);
				float3 v3Attenuate;
				for(int i=0; i<int(fSamples); i++)
				{
					float fHeight = length(v3SamplePoint);
					float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fHeight));
					float fScatter = fDepth*fTemp - fCameraOffset;
					v3Attenuate = exp(-fScatter * (v3InvWavelength * fKr4PI + fKm4PI));
					v3FrontColor += v3Attenuate * (fDepth * fScaledLength);
					v3SamplePoint += v3SampleRay;
				}
				
    			v2f o;
    			o.pos = UnityObjectToClipPos(vertex);
    			o.uv = uv;
    			o.c0 = v3FrontColor * (v3InvWavelength * fKrESun + fKmESun);
    			o.c1 = v3Attenuate;
				o.viewDir = normalize(WorldSpaceViewDir(vertex));

				// normal stuff
				float3 wNormal		= v3Pos / v3PosLength;
				o.normal = wNormal;
				#if WPM_BUMPMAP_ENABLED
                float3 wTangent = UnityObjectToWorldDir(tangent.xyz);
       	        float tangentSign = tangent.w * unity_WorldTransformParams.w;
           	    float3 wBitangent = cross(wNormal, wTangent) * tangentSign;
               	// output the tangent space matrix
                o.tspace0 = float3(wTangent.x, wBitangent.x, wNormal.x);
   	            o.tspace1 = float3(wTangent.y, wBitangent.y, wNormal.y);
       	        o.tspace2 = float3(wTangent.z, wBitangent.z, wNormal.z);
       	        #endif

    			return o;
			}
			
			float3 projectOnPlane(float3 v, float3 n) {
			 	return v - dot(v, n) * n;
			}

			float4 frag(v2f i) : SV_Target
			{
				// get Earth texture texel
				float4 color;
				// compute Earth pixel color
				if (i.uv.x<0.5) {
					if (i.uv.y>0.5) {
						color = tex2D(_TexTL, float2(i.uv.x * 2.0, (i.uv.y - 0.5) * 2.0));
					} else {
						color = tex2D(_TexBL, float2(i.uv.x * 2.0, i.uv.y * 2.0));
					}
				} else {
					if (i.uv.y>0.5) {
						color = tex2D(_TexTR, float2((i.uv.x - 0.5) * 2.0f, (i.uv.y - 0.5) * 2.0));
					} else {
						color = tex2D(_TexBR, float2((i.uv.x - 0.5) * 2.0f, i.uv.y * 2.0));
					}
				}

				// surface normal
				float3 snormal = normalize(i.normal);

				// apply bump mapping
				#if WPM_BUMPMAP_ENABLED
				float3 tnormal = UnpackNormal(tex2D(_NormalMap, i.uv)); 
               	float3 worldNormal;
               	worldNormal.x = dot(i.tspace0, tnormal);
               	worldNormal.y = dot(i.tspace1, tnormal);
               	worldNormal.z = dot(i.tspace2, tnormal);
               	float3 normal = normalize(lerp(snormal, worldNormal, _BumpAmount));
               	#else
               	float3 normal = snormal;
               	#endif

              	// Clouds
                float  LdotS = saturate(dot(_SunLightDirection, snormal));
				float2 t = float2(_Time[0] * _CloudSpeed, 0);
				float2 disp = -i.viewDir * _CloudElevation;
				float4 cloud = tex2D (_CloudMap, i.uv + t - disp);
				cloud.rgb *= (LdotS + _AmbientLight);

				// Cloud shadows
				#if WPM_CLOUDSHADOWS_ENABLED
				const float2 c = float2(0.998,0);
				float3 proj  = projectOnPlane(_SunLightDirection, snormal);
				float3 up    = projectOnPlane(float3(0,1,0), snormal);
				float3 right = projectOnPlane(float3(1,0,0), snormal);
				float  x     = dot(proj, right);
				float  y     = dot(proj, up);
				float2 persp = float2(x,y) * 0.01;
				float4 shadows = tex2D (_CloudMap, i.uv + c + t + persp) * (LdotS * _CloudShadowStrength);
				#endif

                // Earth component
				float LdotN = saturate(dot(_SunLightDirection, normal));
				float lighting = LdotN + _AmbientLight;
				float4 earth = color * lighting;
				#if WPM_CLOUDSHADOWS_ENABLED
				earth *= 1.0 - shadows;
				#endif

				// apply atmosphere scattering
				float3 col = i.c0 + 0.25 * i.c1;
				
				// adjust color from HDR
				col = 1.0 - exp(col * fHdrExposure);
				earth.rgb *= max(col.b, _AmbientLight);
				
				color.rgb = earth.rgb + col * 0.65;

				// Compose clouds
				color = color * (1.0 - (_CloudAlpha * cloud.a)) + cloud * _CloudAlpha;
//				earth = lerp(earth, cloud, _CloudAlpha * cloud.a);
				

				// apply final contrast & brightness
		  		color.rgb = (color.rgb - 0.5.xxx) * _Contrast + 0.5.xxx;
				color.rgb *= _Brightness;
				return color;
			}
			
			ENDCG

    	}
	}
}


