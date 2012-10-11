
Shader "Hidden/MLAA" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

CGINCLUDE
	
	#include "UnityCG.cginc"

	uniform sampler2D _MainTex;
	uniform sampler2D edgesMapL;
	uniform sampler2D edgesMap;
	uniform sampler2D colorMap;
	uniform sampler2D areaMap;
	uniform sampler2D blendMap;
	uniform float4 _MainTex_TexelSize;

	uniform float threshold;

	#define MAX_SEARCH_STEPS 8    
	#define MAX_DISTANCE 32
	
	float4 tex2Dlevel0(sampler2D map, float2 texcoord) {
	    return tex2Dlod(map, float4(texcoord, 0.0, 0.0));
	}	
	
	float4 tex2Doffset(sampler2D map, float2 texcoord, float2 offset) {
	    return tex2Dlevel0(map, texcoord + _MainTex_TexelSize.xy * offset);
	}	

	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};

	/**
	 * Typical Multiply-Add operation to ease translation to assembly code.
	 */
	
	float4 mad(float4 m, float4 a, float4 b) {
	    /* #if defined(XBOX)
	    float4 result;
	    asm {
	        mad result, m, a, b
	    };
	    return result;
	    #else */
	    return m * a + b;
	    // #endif
	}
	
	// hacked in:
	
	float2 round( float2 vals ) {
		half2 fracs = step(0.5, frac(vals));
		return floor(vals) + fracs;
	}
		
		
	/** 
	 * Ok, we have the distance and both crossing edges, can you please return 
	 * the float2 blending weights?
	 */
	
	float2 Area(float2 distance, float e1, float e2) {
	     // * By dividing by areaSize - 1.0 below we are implicitely offsetting to
	     //   always fall inside of a pixel
	     // * Rounding prevents bilinear access precision problems
	    float areaSize = MAX_DISTANCE * 5.0;
	    float2 pixcoord = MAX_DISTANCE * round(4.0 * float2(e1, e2)) + distance;
	    float2 texcoord = pixcoord / (areaSize - 1.0);
	    float4 res = tex2Dlevel0(areaMap, texcoord);
	    return res.ra;
	}	

	//
	//
	// 1ST PASS
	//
	//
	
	
	/**
	 * Same as above, this eases translation to assembly code;
	 */
		
		
	float4 ColorEdgeDetectionPS(float2 texcoord) {
	    float3 weights = float3(0.2126,0.7152, 0.0722); // These ones are from the CIE XYZ standard.
	
	    float L = dot(tex2Dlevel0(_MainTex, texcoord).rgb, weights);
	    float Lleft = dot(tex2Doffset(_MainTex, texcoord, -float2(1.0, 0.0)).rgb, weights);
	    float Ltop = dot(tex2Doffset(_MainTex, texcoord, -float2(0.0, 1.0)).rgb, weights);  
	    float Lright = dot(tex2Doffset(_MainTex, texcoord, float2(1.0, 0.0)).rgb, weights);
	    float Lbottom = dot(tex2Doffset(_MainTex, texcoord, float2(0.0, 1.0)).rgb, weights);
	
	    /**
	     * We detect edges in gamma 1.0/2.0 space. Gamma space boosts the contrast
	     * of the blacks, where the human vision system is more sensitive to small
	     * gradations of intensity.
	     */
	     
	    float4 delta = abs(sqrt(L).xxxx - sqrt(float4(Lleft, Ltop, Lright, Lbottom)));
	    float4 edges = step(threshold.xxxx, delta);
	
	    //if (dot(edges, 1.0) == 0.0)
	    //   discard;
	
		clip(-dot(edges, 1.0));
	
	    return edges;
	}
	
	//
	//
	// 2ND PASS
	//
	//
	
	/**
	 * Search functions for the 2nd pass.
	 */
	
	float SearchXLeft(float2 texcoord) {
	    // We compare with 0.9 to prevent bilinear access precision problems.
	    float i;
	    float e = 0.0;
	    for (i = -1.5; i > -2.0 * MAX_SEARCH_STEPS; i -= 2.0) {
	        e = tex2Doffset(edgesMapL, texcoord, float2(i, 0.0)).g;
	        if (e < 0.9) break;
	    }
	    return max(i + 1.5 - 2.0 * e, -2.0 * MAX_SEARCH_STEPS);
	}
	
	float SearchXRight(float2 texcoord) {
	    float i;
	    float e = 0.0;
	    for (i = 1.5; i < 2.0 * MAX_SEARCH_STEPS; i += 2.0) {
	        e = tex2Doffset(edgesMapL, texcoord, float2(i, 0.0)).g;
	        if (e < 0.9) break;
	    }
	    return min(i - 1.5 + 2.0 * e, 2.0 * MAX_SEARCH_STEPS);
	}
	
	float SearchYUp(float2 texcoord) {
	    float i;
	    float e = 0.0;
	    for (i = -1.5; i > -2.0 * MAX_SEARCH_STEPS; i -= 2.0) {
	        e = tex2Doffset(edgesMapL, texcoord, float2(i, 0.0).yx).r;
	        if (e < 0.9) break;
	    }
	    return max(i + 1.5 - 2.0 * e, -2.0 * MAX_SEARCH_STEPS);
	}
	
	float SearchYDown(float2 texcoord) {
	    float i;
	    float e = 0.0;
	    for (i = 1.5; i < 2.0 * MAX_SEARCH_STEPS; i += 2.0) {
	        e = tex2Doffset(edgesMapL, texcoord, float2(i, 0.0).yx).r;
	        if (e < 0.9) break;
	    }
	    return min(i - 1.5 + 2.0 * e, 2.0 * MAX_SEARCH_STEPS);
	}
	
	
	/**
	 * Checks if the crossing edges e1 and e2 correspond to a _U_ shape.
	 */
	
	bool IsUShape(float e1, float e2) {
	    float t = e1 + e2;
	    return abs(t - 1.5) < 0.1 || abs(t - 0.5) < 0.1;
	}
	
	/**
	 *  S E C O N D   P A S S
	 */
	
	float4 BlendWeightCalculationPS(float2 texcoord) {
	    float4 areas = 0.0;
	
	    float4 e = tex2Dlevel0(edgesMap, texcoord);
	
	    // [branch]
	    if (e.g) { // Edge at north
	
	        // Search distances to the left and to the right:
	        float2 d = float2(SearchXLeft(texcoord), SearchXRight(texcoord));
	
	        // Now fetch the crossing edges. Instead of sampling between edgels, we
	        // sample at -0.25, to be able to discern what value has each edgel:
	        float4 coords = mad(float4(d.x, -0.25, d.y + 1.0, -0.25), _MainTex_TexelSize.xyxy, texcoord.xyxy);
	        
	        float4 tmp;
	        tmp = tex2Dlevel0(edgesMapL, coords.xy);
	        float e1 = tmp.r;
	        tmp = tex2Dlevel0(edgesMapL, coords.zw);
	        float e2 = tmp.r;
	
	        if (-d.r + d.g + 1 > 1 || IsUShape(e1, e2)) {
	            // Ok, we know how this pattern looks like, now it is time for getting
	            // the actual area:
	            areas.rg = Area(abs(d), e1, e2);
	        }
	    }
	
	    // [branch]
	    if (e.r) { // Edge at west
	
	        // Search distances to the top and to the bottom:
	        float2 d = float2(SearchYUp(texcoord), SearchYDown(texcoord));
	
	        // Now fetch the crossing edges (yet again):
	        float4 coords = mad(float4(-0.25, d.x, -0.25, d.y + 1.0),
	                            _MainTex_TexelSize.xyxy, texcoord.xyxy);
	        float e1 = tex2Dlevel0(edgesMapL, coords.xy).g;
	        float e2 = tex2Dlevel0(edgesMapL, coords.zw).g;
	        
	        if (-d.r + d.g + 1 > 1 || IsUShape(e1, e2)) {
	            // Get the area for this direction:
	            areas.ba = Area(abs(d), e1, e2);
	        }
	    }
	
	    return areas;
	}	
	
	
/**
 *  T H I R D   P A S S
 */

float4 NeighborhoodBlendingPS(float2 texcoord )  {
    // Fetch the blending weights for current pixel:
    float4 topLeft = tex2Dlevel0(blendMap, texcoord);
    float bottom = tex2Doffset(blendMap, texcoord, float2(0.0, 1.0)).g;
    float right = tex2Doffset(blendMap, texcoord, float2(1.0, 0.0)).a;
    float4 a = float4(topLeft.r, bottom, topLeft.b, right);

    // There is some blending weight with a value greater than 0.0?
    float sum = dot(a, 1.0);
    // [branch]
    if (sum > 0.0) {;
        float4 color = 0.0;

        // Add the contributions of the possible 4 lines that can cross this pixel:
        #ifdef BILINEAR_FILTER_TRICK
            float4 o = a * PIXEL_SIZE.yyxx;
            color = mad(tex2Dlevel0(colorMapL, texcoord + float2( 0.0, -o.r)), a.r, color);
            color = mad(tex2Dlevel0(colorMapL, texcoord + float2( 0.0,  o.g)), a.g, color);
            color = mad(tex2Dlevel0(colorMapL, texcoord + float2(-o.b,  0.0)), a.b, color);
            color = mad(tex2Dlevel0(colorMapL, texcoord + float2( o.a,  0.0)), a.a, color);
        #else
            float4 C = tex2Dlevel0(colorMap, texcoord);
            float4 Cleft = tex2Doffset(colorMap, texcoord, -float2(1.0, 0.0));
            float4 Ctop = tex2Doffset(colorMap, texcoord, -float2(0.0, 1.0));
            float4 Cright = tex2Doffset(colorMap, texcoord, float2(1.0, 0.0));
            float4 Cbottom = tex2Doffset(colorMap, texcoord, float2(0.0, 1.0));
            color = mad(lerp(C, Ctop, a.r), a.r, color);
            color = mad(lerp(C, Cbottom, a.g), a.g, color);
            color = mad(lerp(C, Cleft, a.b), a.b, color);
            color = mad(lerp(C, Cright, a.a), a.a, color);
        #endif

        // Normalize the resulting color and we are finished!
        return color / sum; 
    } else {
        return tex2Dlevel0(colorMap, texcoord);
    }
}	

	v2f vert( appdata_img v ) {
		v2f o;
		o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
		
		float2 uv = v.texcoord.xy;
		o.uv.xy = uv;
		
		return o;
	}

	half4 fragFirstPass (v2f i) : COLOR {		 	 	    
	    return 0;
	}
	
	half4 fragEdgeDetectAndBlur (v2f i) : COLOR {		 	 	    
	    return 0;
	}
		
ENDCG	

SubShader {
	Pass {
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }
	
		CGPROGRAM
	
		#pragma vertex vert
		#pragma fragment fragFirstPass
		#pragma fragmentoption ARB_precision_hint_fastest 
		
		ENDCG
	}
	Pass {
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }
	
		CGPROGRAM
	
		#pragma vertex vert
		#pragma fragment fragEdgeDetectAndBlur
		#pragma fragmentoption ARB_precision_hint_fastest 
		#pragma target 3.0
		
		ENDCG
	}
}

Fallback off

}