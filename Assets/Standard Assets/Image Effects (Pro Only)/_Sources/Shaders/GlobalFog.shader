

Shader "Hidden/GlobalFog" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "black" {}
}

SubShader {
	Pass {
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }

		CGPROGRAM

		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest 
		#include "UnityCG.cginc"

		uniform sampler2D _MainTex;
		uniform sampler2D _CameraDepthTexture;
		
		uniform float _GlobalDensity;
		uniform float _HeightFalloff;
		uniform float4 _FogColor;
		
		uniform float4 _MainTex_TexelSize;
		
		// for fast world space reconstruction
		uniform float4x4 _FrustumCornersWS;
		uniform float4 _CameraWS;
		 
		struct v2f {
			float4 pos : POSITION;
			float2 uv : TEXCOORD0;
			float4 interpolatedRay : TEXCOORD1;
		};
		
		v2f vert( appdata_img v )
		{
			v2f o;
			half index = v.vertex.z;
			v.vertex.z = 0.1;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv = v.texcoord.xy;
			
			#if SHADER_API_D3D9
			if (_MainTex_TexelSize.y < 0)
				o.uv.y = 1-o.uv.y;
			#endif				
			
			o.interpolatedRay = _FrustumCornersWS[(int)index];
			o.interpolatedRay.w = index;
			return o;
		}
		
		float ComputeFog (in float3 camDir) 
		{
			float fogInt = length( camDir );	
			
			float slopeThreshhold = 0.01;
			if( abs( camDir.y ) > slopeThreshhold ) 
			{
				float t = _HeightFalloff * camDir.y;
				fogInt *= ( 1.0 - exp( -t ) ) / t;				
			}
			
			return exp( -_GlobalDensity * fogInt );
		}
		
		half4 frag (v2f i) : COLOR
		{
			float dpth = Linear01Depth (tex2D (_CameraDepthTexture, i.uv).x);

			float4 wsDir = (/* _CameraWS + */ dpth * i.interpolatedRay);
			return lerp( _FogColor, tex2D( _MainTex, i.uv ), ComputeFog( wsDir.xyz ) );
		}
		
		ENDCG
	}
}

Fallback off

}