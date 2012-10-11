Shader "FX/Ocean" { 
Properties {
	_ReflectionTex ("Internal reflection", 2D) = "black" {}
	_RefractionTex ("Internal refraction", 2D) = "black" {}
	_MainTex ("Fallback texture", 2D) = "black" {}	
	_ShoreTex ("Shore & Foam texture ", 2D) = "black" {}	
	
	_BumpMap ("Normals ", 2D) = "bump" {}
	
	_DisplacementHeightMap ("1st displacement", 2D) = "grey" {}
	_SecondDisplacementHeightMap ("2nd displacement", 2D) = "grey" {} 
	_ThirdDisplacementHeightMap ("3rd displacement", 2D) = "grey" {} 	
	
	_WavesTex ("Internal waves map", 2D) = "black" {}
		
	_DistortParams ("Distortions (Bump waves, Reflection, Fresnel power, Fresnel bias)", Vector) = (1.0 ,1.0, 2.0, 1.15)
	_InvFadeParemeter ("Auto blend parameter (Edge, Shore)", Vector) = (0.15 ,0.15, 0.15, 0.15)
	
	_AnimationTiling ("Animation Tiling (Displacement)", Vector) = (2.2 ,2.2, -1.1, -1.1)
	_AnimationDirection ("Animation Direction & Speed (displacement)", Vector) = (1.0 ,1.0, 1.0, 1.0)

	_BumpTiling ("Bump Tiling", Vector) = (0.1 ,0.1, -0.2, -0.2)
	_BumpDirection ("Bump Direction & Speed", Vector) = (1.0 ,1.0, 1.0, 1.0)

	_Foam ("Foam & white caps (Intensity, Cutoff)", Vector) = (0.5 ,0.75, 0.0, 0.0)
	
	_FresnelScale ("FresnelScale", Range (0.15, 4.0)) = 0.75	

	_BaseColor ("Base color", COLOR)  = ( .54, .95, .99, 0.5)	
	_DepthColor ("Depth color", COLOR)  = ( .34, .85, .92, 0.5)	
	_ReflectionColor ("Reflection color", COLOR)  = ( .54, .95, .99, 0.5)	
	_SpecularColor ("Specular color", COLOR)  = ( .72, .72, .72, 1)
	
	_WorldLightDir ("Specular light direction", Vector) = (0.0, 0.1, -0.5, 0.0)
	_Shininess ("Shininess", Range (2.0, 500.0)) = 200.0	

	_HeightDisplacement("HeightDisplacement", Range (0.0, 7.5)) = 2.5	
	_NormalsDisplacement("NormalsDisplacement", Range (0.0, 100.0)) = 2.0		
}

CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "WaterInclude.cginc"

	struct appdata 
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct v2f 
	{
		float4 pos : SV_POSITION;
		float4 normalInterpolator : TEXCOORD0;
		float4 viewInterpolator : TEXCOORD1; 	
		float4 bumpCoords : TEXCOORD2;
		float4 screenPos : TEXCOORD3;	
	};

	sampler2D _BumpMap;
	sampler2D _ReflectionTex;
	sampler2D _RefractionTex;
	sampler2D _ShoreTex;
	sampler2D _CameraDepthTexture;
	sampler2D _DisplacementHeightMap;
	sampler2D _SecondDisplacementHeightMap;
	sampler2D _ThirdDisplacementHeightMap;

	uniform float4 _DistortParams;
	uniform float4 _RefrColorDepth;
	uniform float4 _SpecularColor;
	uniform float4 _BaseColor;
	uniform float4 _ReflectionColor;
	uniform float4 _DepthColor;

	uniform float4 _InvFadeParemeter;
	uniform float4 _Foam;
	uniform float _Shininess;
	uniform float4 _WorldLightDir;
	uniform float4 _ShoreTiling;

	uniform float _NormalsDisplacement;
	uniform float _HeightDisplacement;

	uniform float4 _BumpTiling;
	uniform float4 _BumpDirection;
	uniform float4 _AnimationTiling;
	uniform float4 _AnimationDirection;
	
	uniform float _FresnelScale;

	#define OVERALL_BUMP_STRENGTH _DistortParams.x
	#define REALTIME_TEXTURE_BUMP_STRENGTH _DistortParams.y
	#define FRESNEL_POWER _DistortParams.z
	#define DEPTH_COLOR_REFRACTION _DistortParams.w

	#define BASIC_FOAM_AMOUNT i.normalInterpolator.w
	#define VERTEX_WORLD_NORMAL i.normalInterpolator.xyz
	#define SCREENSPACE_COORDINATES i.screenPos
	
	#define FRESNEL_BIAS _DistortParams.w
	
	// ..............................................................................................................................
	//
	// HQ VERSION
	// ..............................................................................................................................	

	v2f vert(appdata_full v)
	{
		v2f o;
		
		half3 worldSpaceVertex = mul(_Object2World,(v.vertex)).xyz;
		half2 tileableUv = GetTileableUv(v.vertex);

		// uv's for small waves (bump map lookup only)
		o.bumpCoords.xyzw = tileableUv.xyxy * _BumpTiling.xyzw + _Time.xxxx * _BumpDirection.xyzw;	

		half4 displacementUv = tileableUv.xyxy * _AnimationTiling.xyzw + _Time.xxxx * _AnimationDirection.xyzw;
		#ifdef WATER_VERTEX_DISPLACEMENT_ON
			half4 tf = tex2Dlod(_DisplacementHeightMap, half4(displacementUv.xy, 0.0,0.0));
			tf += tex2Dlod(_SecondDisplacementHeightMap, half4(displacementUv.zw, 0.0,0.0));
			tf += tex2Dlod(_ThirdDisplacementHeightMap, half4(displacementUv.wz, 0.0,0.0));
			tf /= 3.0; 
			o.viewInterpolator.w = saturate((tf.a-0.25)*3.5);
		#else
			half4 tf = half4(0.5,0.5,0.5,0.5);
			o.viewInterpolator.w = 0.5;
		#endif
		
		#ifdef WATER_VERTEX_DISPLACEMENT_ON
			half4 displ = (tf.a - 0.5) * half4(0,unity_Scale.w,0,0) * _HeightDisplacement;
			o.pos = mul(UNITY_MATRIX_MVP,  v.vertex + displ);
			o.viewInterpolator.xyz = (worldSpaceVertex + displ.rgb) - _WorldSpaceCameraPos;		
		#else
			o.pos = mul(UNITY_MATRIX_MVP,  v.vertex);
			o.viewInterpolator.xyz = worldSpaceVertex-_WorldSpaceCameraPos;
		#endif

		o.screenPos = ComputeScreenPos(o.pos); 
		
		#ifdef WATER_VERTEX_DISPLACEMENT_ON
			o.normalInterpolator.xyz = mul((float3x3)_Object2World, (tf.rbg-half3(0.5,0.5,0.5)));
			o.normalInterpolator.xz *= _NormalsDisplacement * 0.1;
			o.normalInterpolator.w = _Foam.x*saturate(tf.a-0.5-_Foam.y);		
		#else
			o.normalInterpolator.xyz = mul((float3x3)_Object2World,v.normal.xyz*unity_Scale.w);
			o.normalInterpolator.w = 0.0;
		#endif

		return o;

	}

	half4 frag( v2f i ) : COLOR
	{		
		half3 worldNormal = PerPixelNormal(_BumpMap, i.bumpCoords, VERTEX_WORLD_NORMAL, OVERALL_BUMP_STRENGTH);
		half3 viewVector = normalize(i.viewInterpolator.xyz);

		half4 rtReflUv = i.screenPos;
		rtReflUv.xy += worldNormal.xz * REALTIME_TEXTURE_BUMP_STRENGTH;
		half4 rtReflections = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(rtReflUv));	
		half4 rtRefractions = tex2Dproj(_RefractionTex, UNITY_PROJ_COORD(rtReflUv));

		half3 reflectVector = normalize(reflect(viewVector, worldNormal));
		//float spec = max(pow(dot(normalize(_WorldLightDir.xyz), reflectVector.xyz), _Shininess), 0.0);
          
          half3 h = normalize (normalize(_WorldLightDir.xyz) + viewVector.xyz);
          float nh = max (0, dot (worldNormal, -h));
          float spec = max(0.0,pow (nh, _Shininess));		
		
		half4 fadeOut = half4(1.0,1.0,1.0,1.0);
		half4 edgeBlendFactors = half4(0.0, 0.0, 0.0, 0.0);
		#ifdef WATER_EDGEBLEND_ON
			half4 depth = tex2D(_CameraDepthTexture, SCREENSPACE_COORDINATES.xy/SCREENSPACE_COORDINATES.w);
			depth.r = 1.0 / (_ZBufferParams.z * depth.r + _ZBufferParams.w);
			edgeBlendFactors = saturate(_InvFadeParemeter * (depth.r-SCREENSPACE_COORDINATES.z));
			fadeOut.a = edgeBlendFactors.x;
			edgeBlendFactors.y =  1.0-edgeBlendFactors.y;
		#endif	
		
		half4 foam = Foam(_ShoreTex, i.bumpCoords, BASIC_FOAM_AMOUNT + edgeBlendFactors.y);		
		//half refl2Refr = FresnelViaTexture(viewVector,worldNormal, _Fresnel);
		worldNormal.xz *= _FresnelScale;
		half refl2Refr = Fresnel(viewVector, worldNormal, FRESNEL_BIAS, FRESNEL_POWER);
		
		half4 baseColor = lerp (_DepthColor, _BaseColor, i.viewInterpolator.w);
		baseColor = lerp (lerp (rtRefractions, baseColor, _BaseColor.a), lerp (rtReflections,_ReflectionColor,_ReflectionColor.a), refl2Refr);
		baseColor = half4 ((foam + baseColor + spec*_SpecularColor).rgb, fadeOut.a );
		
		return baseColor;
	}
	
	// ..............................................................................................................................
	//
	// MQ VERSION
	// ..............................................................................................................................	
	
	v2f vert300(appdata_full v)
	{
		v2f o;
		
		half3 worldSpaceVertex = mul(_Object2World,(v.vertex)).xyz;
		half2 tileableUv = GetTileableUv(v.vertex);

		// uv's for small waves (bump map lookup only)
		o.bumpCoords.xyzw = tileableUv.xyxy * _BumpTiling.xyzw + _Time.xxxx * _BumpDirection.xyzw;	

		half4 displacementUv = tileableUv.xyxy * _AnimationTiling.xyzw + _Time.xxxx * _AnimationDirection.xyzw;
		#ifdef WATER_VERTEX_DISPLACEMENT_ON
			half4 tf = tex2Dlod(_DisplacementHeightMap, half4(displacementUv.xy, 0.0,0.0));
			tf += tex2Dlod(_SecondDisplacementHeightMap, half4(displacementUv.zw, 0.0,0.0));
			tf *= 0.5; 
			o.viewInterpolator.w = saturate((tf.a-0.25)*3.5);
		#else
			half4 tf = half4(0.5,0.5,0.5,0.5);
			o.viewInterpolator.w = 0.5;
		#endif
		
		#ifdef WATER_VERTEX_DISPLACEMENT_ON
			half4 displ = (tf.a - 0.5) * half4(0,unity_Scale.w,0,0) * _HeightDisplacement;
			o.pos = mul(UNITY_MATRIX_MVP,  v.vertex + displ);
			o.viewInterpolator.xyz = (worldSpaceVertex + displ.rgb) - _WorldSpaceCameraPos;		
		#else
			o.pos = mul(UNITY_MATRIX_MVP,  v.vertex);
			o.viewInterpolator.xyz = worldSpaceVertex-_WorldSpaceCameraPos;
		#endif

		o.screenPos = ComputeScreenPos(o.pos); 
		
		#ifdef WATER_VERTEX_DISPLACEMENT_ON
			o.normalInterpolator.xyz = mul((float3x3)_Object2World, (tf.rbg-half3(0.5,0.5,0.5)));
			o.normalInterpolator.xz *= _NormalsDisplacement * 0.1;
			o.normalInterpolator.w = _Foam.x*saturate(tf.a-0.5-_Foam.y);		
		#else
			o.normalInterpolator.xyz = mul((float3x3)_Object2World,v.normal.xyz*unity_Scale.w);
			o.normalInterpolator.w = 0.0;
		#endif

		return o;
	}

	half4 frag300( v2f i ) : COLOR
	{		
		half3 worldNormal = PerPixelNormal(_BumpMap, i.bumpCoords, VERTEX_WORLD_NORMAL, OVERALL_BUMP_STRENGTH);
		half3 viewVector = normalize(i.viewInterpolator.xyz);

		half4 rtReflUv = i.screenPos;
		rtReflUv.xy += worldNormal.xz * REALTIME_TEXTURE_BUMP_STRENGTH;
		half4 rtReflections = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(rtReflUv));	

		half3 reflectVector = normalize( reflect(viewVector, worldNormal) );
		float spec = max(pow(dot(normalize(_WorldLightDir.xyz), reflectVector.xyz), _Shininess), 0.0);
		
		half4 fadeOut = half4(1.0,1.0,1.0,1.0);
		half4 edgeBlendFactors = half4(0.0,0.0,0.0,0.0);
		#ifdef WATER_EDGEBLEND_ON
			half4 depth = tex2D(_CameraDepthTexture, SCREENSPACE_COORDINATES.xy/SCREENSPACE_COORDINATES.w);
			depth.r = 1.0 / (_ZBufferParams.z * depth.r + _ZBufferParams.w);
			edgeBlendFactors = saturate(_InvFadeParemeter * (depth.r-SCREENSPACE_COORDINATES.z));
			fadeOut.a = edgeBlendFactors.x;
		#endif	
		
		worldNormal.xz *= _FresnelScale;		
		half refl2Refr = Fresnel(viewVector, worldNormal, FRESNEL_BIAS, FRESNEL_POWER);
		
		half4 baseColor = lerp (_DepthColor, _BaseColor, i.viewInterpolator.w);
		baseColor = lerp (_BaseColor, lerp (rtReflections,_ReflectionColor,_ReflectionColor.a), refl2Refr);
		baseColor = half4 ((baseColor + spec * _SpecularColor).rgb, fadeOut.a * saturate( 0.5 + refl2Refr*1.0));
		
		return baseColor;
	}	
	
	// ..............................................................................................................................
	//
	// LQ VERSION
	// ..............................................................................................................................	

	v2f vert200(appdata_full v)
	{
		v2f o;
		
		half3 worldSpaceVertex = mul(_Object2World,(v.vertex)).xyz;
		half2 tileableUv = GetTileableUv(v.vertex);

		o.bumpCoords.xyzw = tileableUv.xyxy * _BumpTiling.xyzw + _Time.xxxx * _BumpDirection.xyzw;	

		o.viewInterpolator.xyz = worldSpaceVertex-_WorldSpaceCameraPos;
		o.viewInterpolator.w = 0.5;
		
		o.pos = mul(UNITY_MATRIX_MVP,  v.vertex);
		o.screenPos = ComputeScreenPos(o.pos); 
		
		o.normalInterpolator.xyz = mul((float3x3)_Object2World,v.normal.xyz*unity_Scale.w);
		o.normalInterpolator.w = 0.0;

		return o;

	}

	half4 frag200( v2f i ) : COLOR
	{		
		half3 worldNormal = PerPixelNormal(_BumpMap, i.bumpCoords, VERTEX_WORLD_NORMAL, OVERALL_BUMP_STRENGTH);
		half3 viewVector = normalize(i.viewInterpolator.xyz);

		half3 reflectVector = normalize( reflect(viewVector, worldNormal) );
		float spec = max(pow(dot(normalize(_WorldLightDir.xyz), reflectVector.xyz), _Shininess), 0.0);

		half refl2Refr = Fresnel(viewVector, worldNormal, FRESNEL_BIAS, FRESNEL_POWER);	

		// final color
		
		half4 baseColor = _BaseColor;
		baseColor = lerp(baseColor, _ReflectionColor, refl2Refr);
		baseColor.a = 2.0*refl2Refr;
		
		return baseColor + spec * _SpecularColor;
	}
			
ENDCG

Subshader 
{ 
	Tags {"RenderType"="Transparent" "Queue"="Transparent"}
	
	Lod 500
	ColorMask RGB
	
	Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest LEqual
			ZWrite Off
			Cull Off
			
			CGPROGRAM
			
			// needed for vertex texture fetch via tex2Dlod ()
			#pragma target 3.0 
			
			#pragma vertex vert
			#pragma fragment frag
			
			// hack for enabling texture fetch via tex2Dlod () for both d3d and glsl
			#ifndef SHADER_API_D3D9
				#pragma glsl
			#endif
			
			#pragma fragmentoption ARB_precision_hint_fastest
						
			#pragma multi_compile WATER_VERTEX_DISPLACEMENT_ON WATER_VERTEX_DISPLACEMENT_OFF
			#pragma multi_compile WATER_EDGEBLEND_ON WATER_EDGEBLEND_OFF						
						  			
			ENDCG
	}
}

Subshader 
{ 	
	Tags {"RenderType"="Transparent" "Queue"="Transparent"}
	
	Lod 300
	ColorMask RGB
	
	Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest LEqual
			ZWrite Off
			Cull Off
			
			CGPROGRAM
			
			// needed for vertex texture fetch via tex2Dlod ()
			#pragma target 3.0 
			
			#pragma vertex vert300
			#pragma fragment frag300
			
			// hack for enabling texture fetch via tex2Dlod () for both d3d and glsl
			#ifndef SHADER_API_D3D9
				#pragma glsl
			#endif
			
			#pragma fragmentoption ARB_precision_hint_fastest
						
			#pragma multi_compile WATER_VERTEX_DISPLACEMENT_ON WATER_VERTEX_DISPLACEMENT_OFF
			#pragma multi_compile WATER_EDGEBLEND_ON WATER_EDGEBLEND_OFF						
						  			
			ENDCG
	}	
}

Subshader 
{ 	
	Tags {"RenderType"="Transparent" "Queue"="Transparent"}
	
	Lod 200
	ColorMask RGB
	
	Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest LEqual
			ZWrite Off
			Cull Off
			
			CGPROGRAM
			
			#pragma vertex vert200
			#pragma fragment frag200
			
			#pragma fragmentoption ARB_precision_hint_fastest						
						  			
			ENDCG
	}	
}

// simple transparent fallback
Fallback "Transparent/Diffuse"
}
