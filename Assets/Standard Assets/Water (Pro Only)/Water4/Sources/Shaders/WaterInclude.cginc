
#ifndef WATER_CG_INCLUDED
#define WATER_CG_INCLUDED

#include "UnityCG.cginc"

inline half3 PerPixelNormal(sampler2D bumpMap, half4 coords, half3 vertexNormal, half bumpStrength) 
{
	half4 bump = tex2D(bumpMap, coords.xy) + tex2D(bumpMap, coords.zw);
	bump.xy = bump.wy - half2(1.0, 1.0);
	half3 worldNormal = vertexNormal + bump.xxy * bumpStrength * half3(1,0,1);
	return normalize(worldNormal);
} 

inline half3 PerPixelNormalUnpacked(sampler2D bumpMap, half4 coords, half bumpStrength) 
{
	half4 bump = tex2D(bumpMap, coords.xy) + tex2D(bumpMap, coords.zw);
	bump = bump * 0.5;
	half3 normal = UnpackNormal(bump);
	normal.xy *= bumpStrength;
	return normalize(normal);
} 

inline half3 PerPixelNormalUnpacked(sampler2D bumpMap, half4 coords, half bumpStrength, half2 perVertxOffset)
{
	half4 bump = tex2D(bumpMap, coords.xy) + tex2D(bumpMap, coords.zw);
	bump = bump * 0.5;
	half3 normal = UnpackNormal(bump);
	normal.xy *= bumpStrength;
	normal.xy += perVertxOffset;
	return normalize(normal);	
}

inline half3 PerPixelNormalLite(sampler2D bumpMap, half4 coords, half3 vertexNormal, half bumpStrength) 
{
	half4 bump = tex2D(bumpMap, coords.xy);
	bump.xy = bump.wy - half2(0.5, 0.5);
	half3 worldNormal = vertexNormal + bump.xxy * bumpStrength * half3(1,0,1);
	return normalize(worldNormal);
} 

inline half4 Foam(sampler2D shoreTex, half4 coords, half amount) 
{
	half4 foam = tex2D(shoreTex, coords.xy) * tex2D(shoreTex,coords.zw);
	return foam * amount;
}

inline half4 Foam(sampler2D shoreTex, half4 coords) 
{
	half4 foam = tex2D(shoreTex, coords.xy) * tex2D(shoreTex,coords.zw);
	return foam;
}

inline half Fresnel(half3 viewVector, half3 worldNormal, half bias, half power)
{
	half facing =  clamp(1.0-max(dot(-viewVector, worldNormal), 0.0), 0.0,1.0);	
	half refl2Refr = saturate(bias+(1.0-bias) * pow(facing,power));	
	return refl2Refr;	
}

inline half FresnelViaTexture(half3 viewVector, half3 worldNormal, sampler2D fresnel)
{
	half facing =  saturate(dot(-viewVector, worldNormal));	
	half fresn = tex2D(fresnel, half2(facing, 0.5f)).b;	
	return fresn;
}

inline half2 GetTileableUv(half4 vertex) 
{
	// @NOTE: use worldSpaceVertex.xz instead of ws to make it rotation independent
	half2 ws = half2(_Object2World[0][3],_Object2World[2][3]);
	half2 tileableUv = (ws + vertex.xz/unity_Scale.w);	
	return tileableUv;
}

inline void VertexDisplacementHQ(	sampler2D mapA, sampler2D mapB,
									sampler2D mapC, half4 uv,
									half vertexStrength, half normalsStrength,
									out half4 vertexOffset, out half2 normalOffset) 
{
	// @NOTE: for best performance, this should really be properly packed!
	
	half4 tf = tex2Dlod(mapA, half4(uv.xy, 0.0,0.0));
	tf += tex2Dlod(mapB, half4(uv.zw, 0.0,0.0));
	tf += tex2Dlod(mapC, half4(uv.xw, 0.0,0.0));
	tf /= 3.0; 
	
	tf.rga = tf.rga-half3(0.5,0.5,0.5);
				
	// height displacement in alpha channel, normals info in rgb
	
	vertexOffset = tf.a * half4(0,unity_Scale.w,0,0) * vertexStrength;							
	normalOffset = tf.rg * normalsStrength;
}

inline void VertexDisplacementLQ(	sampler2D mapA, sampler2D mapB,
									sampler2D mapC, half4 uv,
									half vertexStrength, half normalsStrength,
									out half4 vertexOffset, out half2 normalOffset) 
{
	// @NOTE: for best performance, this should really be properly packed!
	
	half4 tf = tex2Dlod(mapA, half4(uv.xy, 0.0,0.0));
	tf += tex2Dlod(mapB, half4(uv.zw, 0.0,0.0));
	tf *= 0.5; 
	
	tf.rga = tf.rga-half3(0.5,0.5,0.5);
				
	// height displacement in alpha channel, normals info in rgb
	
	vertexOffset = tf.a * half4(0,unity_Scale.w,0,0) * vertexStrength;							
	normalOffset = tf.rg * normalsStrength;
}


#endif
