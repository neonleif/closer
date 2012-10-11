Shader "Hidden/SunShaftsComposite" {
	Properties {
		_MainTex ("Base", 2D) = "" {}
		_ColorBuffer ("Color", 2D) = "" {}
	}
	
	CGINCLUDE
				
	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};
		
	sampler2D _MainTex;
	sampler2D _ColorBuffer;
		
	half4 _SunColor;
		
	v2f vert( appdata_img v ) {
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.texcoord.xy;
		return o;
	}
		
	half4 frag(v2f i) : COLOR { 
		half4 depthMask = tex2D (_MainTex, i.uv.xy);
		half4 originalColor = tex2D (_ColorBuffer, i.uv.xy);
		depthMask = saturate (depthMask * _SunColor);	
		return 1.0f - (1.0f-originalColor) * (1.0f-depthMask);	
	}

	ENDCG
	
Subshader {
  
 Pass {
	  ZTest Always Cull Off ZWrite Off
	  Fog { Mode off }      

      CGPROGRAM
      #pragma fragmentoption ARB_precision_hint_fastest 
      #pragma vertex vert
      #pragma fragment frag
      ENDCG
  }
}

Fallback off
	
} // shader