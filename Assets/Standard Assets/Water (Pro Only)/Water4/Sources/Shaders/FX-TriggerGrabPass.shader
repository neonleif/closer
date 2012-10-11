
// this is a little hack until we are able to properly control grab passes (3.5?)
// make sure to assign a mesh that approximates the water surface so that this
// dummy object doesn't get culled

Shader "FX/TriggerGrabPass" {
	Properties {
	}
	SubShader {
		Tags {"RenderType"="Transparent" "Queue"="Transparent-1"}
		LOD 200
		
		GrabPass { }			
	} 
	FallBack Off
}
