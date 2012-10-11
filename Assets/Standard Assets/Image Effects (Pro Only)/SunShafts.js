

@script ExecuteInEditMode
@script RequireComponent (Camera)
@script AddComponentMenu ("Image Effects/Sun Shafts")

enum SunShaftsResolution {
    Low = 0,
    Normal = 1,
	High = 2,
}
		
class SunShafts extends PostEffectsBase 
{	
	public var resolution : SunShaftsResolution = SunShaftsResolution.Normal;
	
	public var sunTransform : Transform;
	public var radialBlurIterations : int = 2;
	public var sunColor : Color = Color.white;
	public var sunShaftBlurRadius : float = 2.5f;
	public var sunShaftIntensity : float = 1.15;
	public var useSkyBoxAlpha : float = 0.75f;
	
	public var maxRadius : float = 0.75f;
	
	public var useDepthTexture : boolean = true;
	
	public var prepareBlurShader : Shader;
	private var prepareBlurMaterial : Material;
		
	public var radialBlurShader : Shader;
	private var radialBlurMaterial : Material;
	
	public var sunShaftsShader : Shader;
	private var sunShaftsMaterial : Material;	
	
	public var simpleClearShader : Shader;
	private var simpleClearMaterial : Material;
	
	
	function CreateMaterials () {			
		prepareBlurMaterial = CheckShaderAndCreateMaterial (prepareBlurShader, prepareBlurMaterial);
		sunShaftsMaterial = CheckShaderAndCreateMaterial (sunShaftsShader, sunShaftsMaterial);
		radialBlurMaterial = CheckShaderAndCreateMaterial (radialBlurShader, radialBlurMaterial);
		simpleClearMaterial = CheckShaderAndCreateMaterial (simpleClearShader, simpleClearMaterial);
	}
	
	function Start () {		
		CreateMaterials ();	
		CheckSupport (useDepthTexture);
		
		if(useDepthTexture) { 
			camera.depthTextureMode |= DepthTextureMode.Depth;	
		}
	}
	
	function OnRenderImage (source : RenderTexture, destination : RenderTexture) {	
		CreateMaterials ();	
		
        var divider : float = 4.0;
        if (resolution == SunShaftsResolution.Normal)
            divider = 2.0;
        else if (resolution == SunShaftsResolution.High)
            divider = 1.0;
            
		var v : Vector3 = Vector3.one * 0.5;
		if (sunTransform)
			v = camera.WorldToViewportPoint (sunTransform.position);
		else {
			v = Vector3(0.5, 0.5, 0.0);
		}            
			
		var secondQuarterRezColor : RenderTexture = RenderTexture.GetTemporary (source.width / divider, source.height / divider, 0);	
        var lrDepthBuffer : RenderTexture = RenderTexture.GetTemporary (source.width / divider, source.height / divider, 0);
		
		// mask out everything except the skybox
		// we have 2 methods, one of which requires depth buffer support, the other one is just comparing images
		
		prepareBlurMaterial.SetVector ("_BlurRadius4", Vector4 (1.0, 1.0, 0.0, 0.0) * sunShaftBlurRadius );
		prepareBlurMaterial.SetVector ("_SunPosition", Vector4 (v.x, v.y, v.z, maxRadius));		
		prepareBlurMaterial.SetFloat ("_NoSkyBoxMask", 1.0f - useSkyBoxAlpha);	
		
		if (!useDepthTexture) {		
			var tmpBuffer : RenderTexture = RenderTexture.GetTemporary(source.width, source.height, 0);					
			RenderTexture.active = tmpBuffer;
			GL.ClearWithSkybox (false, camera);
			
			prepareBlurMaterial.SetTexture("_Skybox", tmpBuffer);
			Graphics.Blit (source, lrDepthBuffer, prepareBlurMaterial, 1);		
			RenderTexture.ReleaseTemporary(tmpBuffer);				
		}
		else {		
			Graphics.Blit (source, lrDepthBuffer, prepareBlurMaterial, 0);
		}
		
        // paint a small black small border to get rid of clamping problems
		DrawBorder (lrDepthBuffer, simpleClearMaterial);
		        			
		// radial blur:
						
		radialBlurIterations = ClampBlurIterationsToSomethingThatMakesSense (radialBlurIterations);	
				
		var ofs : float = sunShaftBlurRadius * (1.0f / 768.0f);
		
		radialBlurMaterial.SetVector ("_BlurRadius4", Vector4 (ofs, ofs, 0.0f, 0.0f));			
		radialBlurMaterial.SetVector ("_SunPosition", Vector4 (v.x, v.y, v.z, maxRadius));				
				
		for (var it2 : int = 0; it2 < radialBlurIterations; it2++ ) {
			// each iteration takes 2 * 6 samples, we need to update _BlurRadius to get a very smooth look
						
			Graphics.Blit (lrDepthBuffer, secondQuarterRezColor, radialBlurMaterial);
			ofs = sunShaftBlurRadius * (((it2 * 2.0f + 1.0f) * 6.0f)) / 768.0f;
			radialBlurMaterial.SetVector ("_BlurRadius4", Vector4 (ofs, ofs, 0.0f, 0.0f) );			
			
			Graphics.Blit (secondQuarterRezColor, lrDepthBuffer, radialBlurMaterial);		
			ofs = sunShaftBlurRadius * (((it2 * 2.0f + 2.0f) * 6.0f)) / 768.0f;			
			radialBlurMaterial.SetVector ("_BlurRadius4", Vector4 (ofs, ofs, 0.0f, 0.0f) );
		}
		
		// put together:
		
		if (v.z >= 0.0)
			sunShaftsMaterial.SetVector ("_SunColor", Vector4 (sunColor.r, sunColor.g, sunColor.b, sunColor.a) * sunShaftIntensity);
		else
			sunShaftsMaterial.SetVector ("_SunColor", Vector4.zero); // no backprojection !
		sunShaftsMaterial.SetTexture ("_ColorBuffer", source);
		Graphics.Blit (lrDepthBuffer, destination, sunShaftsMaterial); 	
		
		RenderTexture.ReleaseTemporary (lrDepthBuffer);	
		RenderTexture.ReleaseTemporary (secondQuarterRezColor);	
	}
		
	// helper functions

	private function ClampBlurIterationsToSomethingThatMakesSense (its : int) : int {
		if (its < 1)
			return 1;
		else if (its > 4)
			return 4;		
		else
			return its;	
	}

}