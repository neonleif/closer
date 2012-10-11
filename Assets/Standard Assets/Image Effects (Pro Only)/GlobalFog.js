
#pragma strict

@script ExecuteInEditMode
@script RequireComponent (Camera)
@script AddComponentMenu ("Image Effects/Global Fog")

class GlobalFog extends PostEffectsBase {
	public var globalDensity : float = 0.5f;
	public var heightFalloff : float = 0.5f;
	public var globalFogColor : Color = Color.grey;
	
	public var fogShader : Shader;
	private var fogMaterial : Material = null;	
	

	function CreateMaterials () {
		fogMaterial = CheckShaderAndCreateMaterial (fogShader, fogMaterial);
	}
	
	function Start () {
		CreateMaterials ();
		CheckSupport (true);
	}
	
	function OnEnable() {
		camera.depthTextureMode |= DepthTextureMode.Depth;	
	}

	function OnRenderImage (source : RenderTexture, destination : RenderTexture) {	
		CreateMaterials ();
		
		var frustumCorners : Matrix4x4;
		var ray : Ray;
		var corner : Vector3;
		
		ray = camera.ViewportPointToRay (Vector3 (0.0f,1.0f, 0.0f)); // TL
		corner =  (ray.direction * camera.farClipPlane);				
		frustumCorners.SetRow (0, corner); 
		
		ray = camera.ViewportPointToRay (Vector3 (1.0f,1.0f, 0.0f)); // TR
		corner =  (ray.direction * camera.farClipPlane);				
		frustumCorners.SetRow (1, corner);		
		
		ray = camera.ViewportPointToRay (Vector3 (1.0f,0.0f, 0.0f)); // BR
		corner =  (ray.direction * camera.farClipPlane);				
		frustumCorners.SetRow (2, corner);
		
		ray = camera.ViewportPointToRay (Vector3 (0.0f,0.0f, 0.0f)); // BL
		corner =  (ray.direction * camera.farClipPlane);				
		frustumCorners.SetRow (3, corner);
								
		fogMaterial.SetMatrix ("_FrustumCornersWS", frustumCorners);
		fogMaterial.SetVector ("_CameraWS", camera.transform.position);
		
		fogMaterial.SetFloat ("_GlobalDensity", globalDensity * 0.01f);
		fogMaterial.SetFloat ("_HeightFalloff", heightFalloff);
		fogMaterial.SetColor ("_FogColor", globalFogColor);
		
		CustomGraphicsBlit (source, destination, fogMaterial, 0);
	}
	
static function CustomGraphicsBlit (source : RenderTexture, dest : RenderTexture, fxMaterial : Material, passNr : int) {
	RenderTexture.active = dest;
	       
	fxMaterial.SetTexture ("_MainTex", source);	        
        
	// var invertY : boolean = source.texelSize.y < 0.0f;
        
	GL.PushMatrix ();
	GL.LoadOrtho ();	
    	
	fxMaterial.SetPass (passNr);	
	
    GL.Begin (GL.QUADS);
						
	GL.MultiTexCoord2 (0, 0.0f, 0.0f); 
	GL.Vertex3 (0.0f, 0.0f, 3.0f); // BL
	
	GL.MultiTexCoord2 (0, 1.0f, 0.0f); 
	GL.Vertex3 (1.0f, 0.0f, 2.0f); // BR
	
	GL.MultiTexCoord2 (0, 1.0f, 1.0f); 
	GL.Vertex3 (1.0f, 1.0f, 1.0f); // TR
	
	GL.MultiTexCoord2 (0, 0.0f, 1.0f); 
	GL.Vertex3 (0.0f, 1.0f, 0.0); // TL
	
	GL.End ();
    GL.PopMatrix ();
}		
}
