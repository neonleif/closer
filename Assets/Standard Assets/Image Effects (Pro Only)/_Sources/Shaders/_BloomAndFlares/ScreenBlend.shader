Shader "Hidden/ScreenBlend" {
	Properties {
		_MainTex ("Screen Blended", 2D) = "" {}
		_ColorBuffer ("Color", 2D) = "" {}
	}
	
	CGINCLUDE

	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : POSITION;
		float2 uv[2] : TEXCOORD0;
	};
		
	sampler2D _ColorBuffer;
	sampler2D _MainTex;
	half _Intensity;
	half4 _ColorBuffer_TexelSize;
		
	v2f vert( appdata_img v ) {
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv[0] =  v.texcoord.xy;
		
		o.uv[1] =  v.texcoord.xy;
		#if SHADER_API_D3D9
		if (_ColorBuffer_TexelSize.y < 0)
			o.uv[1].y = 1-o.uv[1].y;
		#endif	
		
		return o;
	}
	
	half4 fragScreen (v2f i) : COLOR {
		half4 toBlend = saturate (tex2D(_MainTex, i.uv[0]) * _Intensity);
		return 1-(1-toBlend)*(1-tex2D(_ColorBuffer, i.uv[1]));
	}

	half4 fragAdd (v2f i) : COLOR {
		return tex2D(_MainTex, i.uv[0].xy) * _Intensity;
	}

	ENDCG
	
Subshader {
	  ZTest Always Cull Off ZWrite Off
	  Fog { Mode off }  
	  	
 Pass {    

      CGPROGRAM
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma vertex vert
      #pragma fragment fragScreen
      ENDCG
  }
/*
 Pass {    

      CGPROGRAM
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma vertex vert
      #pragma fragment fragAdd
      ENDCG
  }
 */
}

Fallback off
	
} // shader