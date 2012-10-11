
#pragma strict

@script ExecuteInEditMode
@script RequireComponent (Camera)
@script AddComponentMenu ("Image Effects/Fisheye")

class Fisheye extends PostEffectsBase {
	public var strengthX : float = 4.0f;
	public var strengthY : float = 4.0f;

	public var fishEyeShader : Shader = null;
	private var fisheyeMaterial : Material = null;	
	
	function CreateMaterials () {
		fisheyeMaterial = CheckShaderAndCreateMaterial(fishEyeShader,fisheyeMaterial);
	}
	
	function Start () {
		CreateMaterials ();
		CheckSupport (false);
	}
	
	function OnRenderImage (source : RenderTexture, destination : RenderTexture) {		
		CreateMaterials ();
		
		var oneOverBaseSize : float = 1.0f / 512.0f;		
		
		var ar : float = (source.width * 1.0) / (source.height * 1.0);
		
		fisheyeMaterial.SetVector ("intensity", Vector4 (strengthX * ar * oneOverBaseSize, strengthY * oneOverBaseSize, strengthX * ar * oneOverBaseSize, strengthY * oneOverBaseSize));
		Graphics.Blit (source, destination, fisheyeMaterial); 	
	}
}