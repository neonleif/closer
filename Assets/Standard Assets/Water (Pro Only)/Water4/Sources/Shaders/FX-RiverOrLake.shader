Shader "FX/RiverOrLake" { 
	Properties {
		_ReflectionTex ("Internal reflection", 2D) = "black" {}
		_MainTex ("Fallback texture", 2D) = "black" {}	
		_CubeTex ("Fallback cube reflection", CUBE) = "white" {}	
		_ShoreTex ("Shore & Foam texture ", 2D) = "black" {}	
		_BumpMap ("Normals ", 2D) = "bump" {}
				
		_DisplacementHeightMap ("1st displacement", 2D) = "grey" {}
		_SecondDisplacementHeightMap ("2nd displacement", 2D) = "grey" {} 
		_ThirdDisplacementHeightMap ("3rd displacement", 2D) = "grey" {} 	
					
		_DistortParams ("Distortions (Bump waves, Reflection, Fresnel power, Fresnel bias)", Vector) = (1.0 ,1.0, 2.0, 1.15)
		_InvFadeParemeter ("Auto blend parameter (Edge, Shore)", Vector) = (0.15 ,0.15, 0.15, 0.15)
		_FresnelScale ("FresnelScale", Range (0.15, 4.0)) = 0.75	
		
		_AnimationTiling ("Animation Tiling (Displacement)", Vector) = (2.2 ,2.2, -1.1, -1.1)
		_AnimationDirection ("Animation Direction & Speed (displacement)", Vector) = (1.0 ,1.0, 1.0, 1.0)
	
		_BumpTiling ("Bump Tiling", Vector) = (0.1 ,0.1, -0.2, -0.2)
		_BumpDirection ("Bump Direction & Speed", Vector) = (1.0 ,1.0, 1.0, 1.0)
	
		_BaseColor ("Base color", COLOR)  = ( .54, .95, .99, 0.5)	
		_ReflectionColor ("Reflection color", COLOR)  = ( .54, .95, .99, 0.5)	
		
		_Shininess ("Shininess", Range (2.0, 500.0)) = 200.0	
	
		_HeightDisplacement ("HeightDisplacement", Range (0.0, 17.5)) = 2.5	
		_NormalsDisplacement ("NormalsDisplacement", Range (0.0, 250.0)) = 1.0	
	}
	
	// ..............................................................................................................................
	//
	// HQ VERSION
	// ..............................................................................................................................
	
	SubShader 
	{
		Tags {"RenderType"="Transparent" "Queue"="Transparent"}
	
		Lod 500
		Blend SrcAlpha OneMinusSrcAlpha
		ZTest LEqual
		ZWrite Off
		Cull Off
						
		CGPROGRAM
		
		#pragma target 3.0
		#pragma surface surfHq SimpleWater vertex:vert noambient
		#pragma multi_compile WATER_EDGEBLEND_ON WATER_EDGEBLEND_OFF
		#pragma multi_compile WATER_VERTEX_DISPLACEMENT_ON WATER_VERTEX_DISPLACEMENT_OFF
				
		#include "WaterInclude.cginc"

		// hack for enabling texture fetch via tex2Dlod () for both d3d and glsl
		#ifndef SHADER_API_D3D9
			#pragma glsl
		#endif

		sampler2D _MainTex;
		sampler2D _BumpMap;
		
		sampler2D _DisplacementHeightMap;
		sampler2D _SecondDisplacementHeightMap;
		sampler2D _ThirdDisplacementHeightMap;
		sampler2D _ShoreTex;
		//sampler2D _Fresnel;

		sampler2D _ReflectionTex;
		
		sampler2D _GrabTexture;
		sampler2D _CameraDepthTexture;
		
		uniform float4 _DistortParams;
		uniform float4 _BaseColor;
		uniform float4 _ReflectionColor;
			
		uniform float _HeightDisplacement;
		uniform float _NormalsDisplacement;
		
		uniform float _Shininess;

		uniform float _FresnelScale;
		uniform float4 _InvFadeParemeter;

		uniform float4 _AnimationTiling;
		uniform float4 _AnimationDirection;
		uniform float4 _BumpTiling;
		uniform float4 _BumpDirection;
		
		uniform half4 _GrabPassFix;
		
		#define OVERALL_BUMP_STRENGTH _DistortParams.x
		#define REALTIME_TEXTURE_BUMP_STRENGTH _DistortParams.y
		#define FRESNEL_POWER _DistortParams.z
		#define FRESNEL_BIAS _DistortParams.w
		
		struct Input 
		{
			float2 uv_MainTex;
			float3 viewDir;
			float4 screenPos;
			float2 vtxNormalDisplace;
			INTERNAL_DATA
		};		
		
      	half4 LightingSimpleWater (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
          half3 h = normalize (lightDir + viewDir);
          half diff = max (0, dot (s.Normal, lightDir));

          float nh = max (0, dot (s.Normal, h));
          float spec = pow (nh, _Shininess);

          half4 c;
          c.rgb = /* s.Albedo.rgb * (_LightColor0.rgb*max(0.1,saturate(dot(normalize(s.Normal),normalize(lightDir))))) + */ (_LightColor0.rgb * spec) * (atten * 2);
          c.a = s.Alpha;
          
          return c;
      	}		
		
      	void vert (inout appdata_full v, out Input o) 
      	{          	
			half2 tileableUv = GetTileableUv(v.vertex);
			half4 displacementUv = tileableUv.xyxy * _AnimationTiling.xyzw + _Time.xxxx * _AnimationDirection.xyzw;
          	
          	v.texcoord.xy = tileableUv.xy;       	
          	
          	#ifdef WATER_VERTEX_DISPLACEMENT_ON
          		half4 vertexDisplacement;
          		half2 normalsDisplacement;
          		VertexDisplacementHQ (_DisplacementHeightMap,_SecondDisplacementHeightMap,_ThirdDisplacementHeightMap,
          			displacementUv,_HeightDisplacement,_NormalsDisplacement*0.1, vertexDisplacement, normalsDisplacement);
          		o.vtxNormalDisplace = normalsDisplacement;
          		v.vertex += vertexDisplacement;	
			#else
				o.vtxNormalDisplace = half2(0,0);			
			#endif
      	}		
      	
		void surfHq (Input IN, inout SurfaceOutput o) 
		{
			half4 tcs = IN.uv_MainTex.xyxy * _BumpTiling.xyzw + _Time.xxxx * _BumpDirection.xyzw;
			
			half4 c = _BaseColor;
						
			o.Normal = PerPixelNormalUnpacked(_BumpMap, tcs, OVERALL_BUMP_STRENGTH, IN.vtxNormalDisplace.xy);
			
			// edge blending
			half4 fadeOut = half4(1.0,1.0,1.0,1.0);
			half4 edgeBlendFactors = half4(0.0, 0.0, 0.0, 0.0);					
			
			#ifdef WATER_EDGEBLEND_ON
				half4 depth = tex2D(_CameraDepthTexture, IN.screenPos.xy/IN.screenPos.w);
				depth.r = 1.0 / (_ZBufferParams.z * depth.r + _ZBufferParams.w);
				edgeBlendFactors = saturate(_InvFadeParemeter * (depth.r-IN.screenPos.z));
				fadeOut.a = edgeBlendFactors.x;
				edgeBlendFactors.y = 1.0-edgeBlendFactors.y;
			#endif			
			
			// reflections & refractions
			IN.screenPos.xy += o.Normal.xy * REALTIME_TEXTURE_BUMP_STRENGTH * IN.screenPos.w;
			half4 rtReflections = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(IN.screenPos));		
		
			#if SHADER_API_D3D9			
				half2 grabLookup = (IN.screenPos.xy / IN.screenPos.w) * _GrabPassFix.xy + _GrabPassFix.zw;
				half4 rtRefractions = tex2D(_GrabTexture, grabLookup);
			#else
				half4 rtRefractions = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(IN.screenPos));
			#endif
			
			#ifdef WATER_EDGEBLEND_ON
				half4 foam = Foam(_ShoreTex, tcs);				
			#else
				half4 foam = 0;
			#endif	
			
			o.Normal.xy *= _FresnelScale;
			half refl2Refr = Fresnel(normalize(-IN.viewDir), normalize(o.Normal), FRESNEL_BIAS, FRESNEL_POWER);
			//half refl2Refr = FresnelViaTexture(normalize(-IN.viewDir), normalize(o.Normal), _Fresnel);

			c.rgb = lerp(lerp(rtRefractions,_BaseColor,_BaseColor.a), lerp(rtReflections,_ReflectionColor,_ReflectionColor.a), refl2Refr).rgb;
			
			o.Albedo = lerp(c.rgb, foam.rgb, edgeBlendFactors.y);
			o.Emission = o.Albedo;					
			o.Alpha = fadeOut.a;
		}		

		ENDCG		
	} 

	// ..............................................................................................................................
	//
	// MQ VERSION
	// ..............................................................................................................................

	SubShader 
	{
		Tags {"RenderType"="Transparent" "Queue"="Transparent"}
	
		Lod 300
		Blend SrcAlpha OneMinusSrcAlpha
		ZTest LEqual
		ZWrite Off
		Cull Off
						
		CGPROGRAM

		#pragma surface surfMq SimpleWater vertex:vert noambient
		#pragma target 3.0
		
		#include "WaterInclude.cginc"

		// hack for enabling texture fetch via tex2Dlod () for both d3d and glsl
		#ifndef SHADER_API_D3D9
			#pragma glsl
		#endif

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _ReflectionTex;
		sampler2D _GrabTexture;
		
		uniform float4 _DistortParams;
		uniform float4 _BaseColor;
		uniform float4 _ReflectionColor;
		
		uniform float _Shininess;

		uniform float4 _BumpTiling;
		uniform float4 _BumpDirection;

		uniform half4 _GrabPassFix;
		
		#define OVERALL_BUMP_STRENGTH _DistortParams.x
		#define REALTIME_TEXTURE_BUMP_STRENGTH _DistortParams.y
		#define FRESNEL_POWER _DistortParams.z
		#define FRESNEL_BIAS _DistortParams.w
		
      	half4 LightingSimpleWater (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
          half3 h = normalize (lightDir + viewDir);
          half diff = max (0, dot (s.Normal, lightDir));

          float nh = max (0, dot (s.Normal, h));
          float spec = pow (nh, _Shininess);

          half4 c;
          c.rgb = /* s.Albedo.rgb * (_LightColor0.rgb*max(0.1,saturate(dot(normalize(s.Normal),normalize(lightDir))))) + */ (_LightColor0.rgb * spec) * (atten * 2);
          c.a = s.Alpha;
          
          return c;
      	}		
		
		struct Input 
		{
			float2 uv_MainTex;
			float3 viewDir;
			float4 screenPos;
			INTERNAL_DATA
		};
		
      	void vert (inout appdata_full v, out Input o) 
      	{          	
			half2 tileableUv = GetTileableUv(v.vertex);          	
          	v.texcoord.xy = tileableUv.xy;       	
      	}		
      	
		void surfMq (Input IN, inout SurfaceOutput o) 
		{
			half4 tcs = IN.uv_MainTex.xyxy * _BumpTiling.xyzw + _Time.xxxx * _BumpDirection.xyzw;
			
			half4 c = _BaseColor;
						
			o.Normal = PerPixelNormalUnpacked(_BumpMap, tcs, OVERALL_BUMP_STRENGTH);		
			
			// reflections & refractions
			IN.screenPos.xy += o.Normal.xy * REALTIME_TEXTURE_BUMP_STRENGTH * IN.screenPos.w;
			#if SHADER_API_D3D9			
				half2 grabLookup = (IN.screenPos.xy / IN.screenPos.w);
				half4 rtRefractions = tex2D(_GrabTexture, grabLookup * _GrabPassFix.xy + _GrabPassFix.zw);
				half4 rtReflections = tex2D(_ReflectionTex, grabLookup);					
			#else
				half4 rtRefractions = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(IN.screenPos));
				half4 rtReflections = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(IN.screenPos));									
			#endif
			half refl2Refr = Fresnel(normalize(-IN.viewDir), normalize(o.Normal), FRESNEL_BIAS, FRESNEL_POWER);
			c.rgb = lerp(lerp(rtRefractions,_BaseColor,_BaseColor.a), lerp(rtReflections,_ReflectionColor,_ReflectionColor.a), refl2Refr).rgb;
			
			o.Albedo = c.rgb;
			o.Emission = o.Albedo;
			o.Alpha = 1.0;
		}		
		ENDCG		
	} 
	
	// ..............................................................................................................................
	//
	// LQ VERSION
	// ..............................................................................................................................
	
	SubShader 
	{
		Tags {"RenderType"="Transparent" "Queue"="Transparent"}
	
		Lod 200
		Blend SrcAlpha OneMinusSrcAlpha
		ZTest LEqual
		ZWrite Off
		Cull Off
						
		CGPROGRAM

		#pragma surface surfLq SimpleWater vertex:vert noambient
		#include "WaterInclude.cginc"

		#ifndef SHADER_API_D3D9
			#pragma glsl
		#endif

		sampler2D _BumpMap;
		samplerCUBE _CubeTex;
		
		uniform float4 _DistortParams;
		uniform float4 _BaseColor;
		uniform float4 _ReflectionColor;
		
		uniform float _Shininess;

		uniform float4 _BumpTiling;
		uniform float4 _BumpDirection;
		
		#define OVERALL_BUMP_STRENGTH _DistortParams.x
		#define FRESNEL_POWER _DistortParams.z	
		#define FRESNEL_BIAS _DistortParams.w
		
      	half4 LightingSimpleWater (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
          half3 h = normalize (lightDir + viewDir);
          half diff = max (0, dot (s.Normal, lightDir));

          float nh = max (0, dot (s.Normal, h));
          float spec = pow (nh, _Shininess);

          half4 c;
          c.rgb = /* s.Albedo.rgb * (_LightColor0.rgb*max(0.1,saturate(dot(normalize(s.Normal),normalize(lightDir))))) + */ (_LightColor0.rgb * spec) * (atten * 2);
          c.a = saturate(s.Alpha*2.0);
          
          return c;
      	}		
		
		struct Input {
			float2 uv_MainTex;
			float3 viewDir;
			INTERNAL_DATA
		};
		
      	void vert (inout appdata_full v, out Input o) {          	
			half2 tileableUv = GetTileableUv(v.vertex);
          	v.texcoord.xy = tileableUv.xy;       	
      	}		
      	
		void surfLq (Input IN, inout SurfaceOutput o) {
			half4 tcs = IN.uv_MainTex.xyxy * _BumpTiling.xyzw + _Time.xxxx * _BumpDirection.xyzw;
			
			half4 c = _BaseColor;
			o.Normal = PerPixelNormalUnpacked(_BumpMap, tcs, OVERALL_BUMP_STRENGTH);		
			
			// reflections & refractions
			half refl2Refr = Fresnel(normalize(-IN.viewDir), normalize(o.Normal), FRESNEL_BIAS, FRESNEL_POWER);
			c.rgb = lerp(_BaseColor, lerp (texCUBE(_CubeTex,IN.viewDir),_ReflectionColor, _ReflectionColor.a), refl2Refr).rgb;
			
			o.Albedo = c.rgb;
			o.Emission = o.Albedo;
			o.Alpha = refl2Refr;
		}		
		ENDCG		
	} 	
	
	FallBack "Transparent/Diffuse"
}
