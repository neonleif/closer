
Shader "Hidden/DLAA" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

CGINCLUDE
	
	#include "UnityCG.cginc"

	uniform sampler2D _MainTex;
	uniform float4 _MainTex_TexelSize;

	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};
		
	float4 sampleOffseted(  sampler2D tex, float2 texCoord, float2 pixelOffset )
	{
	   return tex2D(tex, texCoord + pixelOffset * _MainTex_TexelSize.xy);
	}
	
	float avg( float3 value )
	{
	   float oneThird = 1.0 / 3.0;
	   return dot(value.xyz, float3(oneThird, oneThird, oneThird) );
	}
	
	float4 edgeDetect( float2 texCoord )
	{
	   float4 sCenter       = sampleOffseted(_MainTex, texCoord, float2( 0.0,  0.0) );
	   float4 sUpLeft       = sampleOffseted(_MainTex, texCoord, float2(-0.5, -0.5) );
	   float4 sUpRight   = sampleOffseted(_MainTex, texCoord, float2( 0.5, -0.5) );
	   float4 sDownLeft  = sampleOffseted(_MainTex, texCoord, float2(-0.5,  0.5) );
	   float4 sDownRight = sampleOffseted(_MainTex, texCoord, float2( 0.5,  0.5) );
	 
	   float4 diff          = abs( ((sUpLeft + sUpRight + sDownLeft + sDownRight) * 4.0) - (sCenter * 16.0) );
	   float edgeMask       = avg(diff.xyz);
	
	   return float4(sCenter.rgb, edgeMask);
	}
	
	float4 edgeDetectAndBlur(  float2 texCoord )
	{
        // short edges
        float4 sampleCenter     = sampleOffseted(_MainTex, texCoord.xy, float2( 0.0,  0.0) );   
        float4 sampleHorizNeg0   = sampleOffseted(_MainTex, texCoord.xy, float2(-1.5,  0.0) );
        float4 sampleHorizPos0   = sampleOffseted(_MainTex, texCoord.xy, float2( 1.5,  0.0) ); 
        float4 sampleVertNeg0   = sampleOffseted(_MainTex, texCoord.xy, float2( 0.0, -1.5) ); 
        float4 sampleVertPos0   = sampleOffseted(_MainTex, texCoord.xy, float2( 0.0,  1.5) );

        float4 sumHoriz         = sampleHorizNeg0 + sampleHorizPos0;
        float4 sumVert                  = sampleVertNeg0  + sampleVertPos0;

        float4 diffToCenterHoriz = abs( sumHoriz - (2.0 * sampleCenter) ) / 2.0;  
        float4 diffToCenterVert  = abs( sumHoriz - (2.0 * sampleCenter) ) / 2.0;

        float valueEdgeHoriz    = avg( diffToCenterHoriz.xyz );
        float valueEdgeVert     = avg( diffToCenterVert.xyz );
        
        float edgeDetectHoriz   = saturate( (3.0 * valueEdgeHoriz) - 0.1);
        float edgeDetectVert    = saturate( (3.0 * valueEdgeVert)  - 0.1);

        float4 avgHoriz         = ( sumHoriz + sampleCenter) / 3.0;
        float4 avgVert                  = ( sumVert  + sampleCenter) / 3.0;

        float valueHoriz                = avg( avgHoriz.xyz );
        float valueVert         = avg( avgVert.xyz );

        float blurAmountHoriz   = saturate( edgeDetectHoriz / valueHoriz );
        float blurAmountVert    = saturate( edgeDetectVert  / valueVert );

        float4 aaResult         = lerp( sampleCenter,  avgHoriz, blurAmountHoriz );
        aaResult                        = lerp( aaResult,       avgVert,  blurAmountVert );
  
        // long edges
        float4 sampleVertNeg1   = sampleOffseted(_MainTex, texCoord.xy, float2(0.0, -3.5) ); 
        float4 sampleVertNeg2   = sampleOffseted(_MainTex, texCoord.xy, float2(0.0, -7.5) );
        float4 sampleVertPos1   = sampleOffseted(_MainTex, texCoord.xy, float2(0.0,  3.5) ); 
        float4 sampleVertPos2   = sampleOffseted(_MainTex, texCoord.xy, float2(0.0,  7.5) ); 

        float4 sampleHorizNeg1   = sampleOffseted(_MainTex, texCoord.xy, float2(-3.5, 0.0) ); 
        float4 sampleHorizNeg2   = sampleOffseted(_MainTex, texCoord.xy, float2(-7.5, 0.0) );
        float4 sampleHorizPos1   = sampleOffseted(_MainTex, texCoord.xy, float2( 3.5, 0.0) ); 
        float4 sampleHorizPos2   = sampleOffseted(_MainTex, texCoord.xy, float2( 7.5, 0.0) ); 

        float pass1EdgeAvgHoriz  = ( sampleHorizNeg2.a + sampleHorizNeg1.a + sampleCenter.a + sampleHorizPos1.a + sampleHorizPos2.a ) / 5.0;
        float pass1EdgeAvgVert   = ( sampleVertNeg2.a  + sampleVertNeg1.a  + sampleCenter.a + sampleVertPos1.a  + sampleVertPos2.a  ) / 5.0;
        pass1EdgeAvgHoriz       = saturate( pass1EdgeAvgHoriz * 2.0f - 1.0f );
        pass1EdgeAvgVert                = saturate( pass1EdgeAvgVert  * 2.0f - 1.0f );
        float longEdge                  = max( pass1EdgeAvgHoriz, pass1EdgeAvgVert);

		float4 sampleLeft       = sampleOffseted(_MainTex, texCoord.xy, float2(-1.0,  0.0) );
		float4 sampleRight   = sampleOffseted(_MainTex, texCoord.xy, float2( 1.0,  0.0) );
		float4 sampleUp         = sampleOffseted(_MainTex, texCoord.xy, float2( 0.0, -1.0) );
		float4 sampleDown       = sampleOffseted(_MainTex, texCoord.xy, float2( 0.0,  1.0) );

		if ( longEdge > 0 )
		{
	        float4 avgHorizLong  = ( sampleHorizNeg2 + sampleHorizNeg1 + sampleCenter + sampleHorizPos1 + sampleHorizPos2 ) / 5.0;
	        float4 avgVertLong   = ( sampleVertNeg2  + sampleVertNeg1  + sampleCenter + sampleVertPos1  + sampleVertPos2  ) / 5.0;
	        float valueHorizLong   = avg(avgHorizLong.xyz);
	        float valueVertLong     = avg(avgVertLong.xyz);

	        float valueCenter       = avg(sampleCenter.xyz);
	        float valueLeft         = avg(sampleLeft.xyz);
	        float valueRight        = avg(sampleRight.xyz);
	        float valueTop          = avg(sampleUp.xyz);
	        float valueBottom       = avg(sampleDown.xyz);
	
	        float4 diffToCenter  = valueCenter - float4(valueLeft, valueTop, valueRight, valueBottom);      
	        float blurAmountLeft = saturate( 0.0 + ( valueVertLong  - valueLeft   ) / diffToCenter.x );
	        float blurAmountUp   = saturate( 0.0 + ( valueHorizLong - valueTop      ) / diffToCenter.y );
	        float blurAmountRight= saturate( 1.0 + ( valueVertLong  - valueCenter ) / diffToCenter.z );
	        float blurAmountDown = saturate( 1.0 + ( valueHorizLong - valueCenter ) / diffToCenter.w );     
	
	        float4 blurAmounts   = float4( blurAmountLeft, blurAmountRight, blurAmountUp, blurAmountDown );
	        blurAmounts             = (blurAmounts == float4(0.0, 0.0, 0.0, 0.0)) ? float4(1.0, 1.0, 1.0, 1.0) : blurAmounts;
	
	        float4 longBlurHoriz = lerp( sampleLeft,  sampleCenter,  blurAmounts.x );
	        longBlurHoriz           = lerp( sampleRight, longBlurHoriz, blurAmounts.y );
	        float4 longBlurVert  = lerp( sampleUp,  sampleCenter,  blurAmounts.z );
	        longBlurVert            = lerp( sampleDown,  longBlurVert,  blurAmounts.w );
	
	        aaResult                = lerp( aaResult,       longBlurHoriz, pass1EdgeAvgVert);
	        aaResult                = lerp( aaResult,       longBlurVert,  pass1EdgeAvgHoriz);
		}
	
	   return float4(aaResult.rgb, 1.0f);
	}	

	v2f vert( appdata_img v ) {
		v2f o;
		o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
		
		float2 uv = v.texcoord.xy;
		o.uv.xy = uv;
		
		return o;
	}

	half4 fragFirst (v2f i) : COLOR {		 	 	    
	    return edgeDetect( i.uv );
	}
	
	half4 fragSecond (v2f i) : COLOR {		 	 	    
	    return edgeDetectAndBlur( i.uv );
	}
		
ENDCG	

SubShader {
	Pass {
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }
	
		CGPROGRAM
	
		#pragma vertex vert
		#pragma fragment fragFirst
		#pragma fragmentoption ARB_precision_hint_fastest 
		
		ENDCG
	}
	Pass {
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }
	
		CGPROGRAM
	
		#pragma vertex vert
		#pragma fragment fragSecond
		#pragma fragmentoption ARB_precision_hint_fastest 
		#pragma target 3.0
		
		ENDCG
	}
}

Fallback off

}