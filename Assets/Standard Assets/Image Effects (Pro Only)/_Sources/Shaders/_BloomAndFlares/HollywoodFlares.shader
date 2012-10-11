Shader "Hidden/HollywoodFlares" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
		_NonBlurredTex ("Base (RGB)", 2D) = "" {}
	}
	
	CGINCLUDE

	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};
	
	float4 offsets;
	float4 tintColor;
	float stretchWidth;
	
	sampler2D _MainTex;
	sampler2D _NonBlurredTex;
		
	v2f vert (appdata_img v) {
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv =  v.texcoord.xy;
		return o;
	}
		
	half4 fragPrepare (v2f i) : COLOR {
		half4 color = tex2D (_MainTex, i.uv);
		half4 colorNb = tex2D (_NonBlurredTex, i.uv);
				
		return color * tintColor * 0.5 + colorNb * normalize (tintColor) * 0.5; // - saturate(colorNb - color); 
	}
	
	half4 fragStretch (v2f i) : COLOR {
		float4 color = tex2D (_MainTex, i.uv);

		float b = stretchWidth;

		color = max (color,tex2D (_MainTex, i.uv + b * 2.0 * offsets.xy));
		color = max (color,tex2D (_MainTex, i.uv - b * 2.0 * offsets.xy));		
		color = max (color,tex2D (_MainTex, i.uv + b * 4.0 * offsets.xy));
		color = max (color,tex2D (_MainTex, i.uv - b * 4.0 * offsets.xy));
		color = max (color,tex2D (_MainTex, i.uv + b * 8.0 * offsets.xy));
		color = max (color,tex2D (_MainTex, i.uv - b * 8.0 * offsets.xy));
		color = max (color,tex2D (_MainTex, i.uv + b * 14.0 * offsets.xy));
		color = max (color,tex2D (_MainTex, i.uv - b * 14.0 * offsets.xy));
		color = max (color,tex2D (_MainTex, i.uv + b * 20.0 * offsets.xy));
		color = max (color,tex2D (_MainTex, i.uv - b * 20.0 * offsets.xy));

								
		return color;
	}	

	ENDCG
	
Subshader {
	  ZTest Always Cull Off ZWrite Off
	  Fog { Mode off } 
 Pass {     

      CGPROGRAM
      
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma vertex vert
      #pragma fragment fragPrepare
      
      ENDCG
  }

 Pass {     

      CGPROGRAM
      
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma vertex vert
      #pragma fragment fragStretch
      
      ENDCG
  }
}
	
Fallback off
	
} // shader