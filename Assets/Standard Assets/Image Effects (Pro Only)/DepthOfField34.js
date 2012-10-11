
#pragma strict

@script ExecuteInEditMode
@script RequireComponent (Camera)
@script AddComponentMenu ("Image Effects/Depth of Field (3.4)") 

enum Dof34QualitySetting {
	OnlyBackground = 1,
	BackgroundAndForeground = 2,
}

enum DofResolution{
	High = 2,
	Medium = 3,
	Low = 4,	
}

class DepthOfField34 extends PostEffectsBase {
	
	public var quality : Dof34QualitySetting = Dof34QualitySetting.OnlyBackground;
	public var resolution : DofResolution  = DofResolution.Medium;
	public var simpleTweakMode : boolean = true;
	
	// simple tweak mode
	public var focalPoint : float = 0.2f;
	public var smoothness : float = 2.0f;
	
	// complex tweak mode
	public var focalZDistance : float = 0.0;
	public var focalZStartCurve : float = 1.0f;
	public var focalZEndCurve : float = 1.0f;
	
	private var focalStartCurve : float = 1.2;
	private var focalEndCurve : float = 1.2;
	
	private var focalDistance01 : float = 0.1;
		
	public var objectFocus : Transform = null;
	public var focalSize : float = 0.0025;
	
	public var blurIterations : int = 1;
	public var maxBlurSpread : float = 1.375;
	
	public var foregroundBlurIterations : int = 1;
	public var foregroundMaxBlurSpread : float = 1.375;
	public var foregroundBlurExtrude : float = 1.65;
			
	public var dofBlurShader : Shader;
	private var dofBlurMaterial : Material = null;	
	
	public var dofShader : Shader;
	private var dofMaterial : Material = null;
    
    public var visualize : boolean = false;
    
    private var widthOverHeight : float = 1.25f;
    private var oneOverBaseSize : float = 1.0f / 512.0f;	
        
    public var bokeh : boolean = false;
    public var bokehSupport : boolean = true;
    public var bokehShader : Shader;
    public var bokehTexture : Texture2D;
    private var bokehMaterial : Material;
    public var bokehScale : float = 6.0;
    public var bokehIntensity : float = 5.1251265;
    public var bokehThreshhold : float = 0.65f;
    public var bokehBlendStrength : float = 0.875f;
    public var bokehDownsample : int = 1;
	
	function CreateMaterials () {		
		dofBlurMaterial = CheckShaderAndCreateMaterial (dofBlurShader, dofBlurMaterial);
		dofMaterial = CheckShaderAndCreateMaterial (dofShader,dofMaterial);  
		bokehSupport = bokehShader.isSupported;     

		if(bokeh && bokehSupport && bokehShader) 
			bokehMaterial = CheckShaderAndCreateMaterial (bokehShader, bokehMaterial);
	}
	
	function Start () {
		CreateMaterials ();
		CheckSupport (true);
	}

	function OnDisable() {
		Triangles.Cleanup ();	
	}

	function OnEnable() {
		camera.depthTextureMode |= DepthTextureMode.Depth;		
	}
	
	function FocalDistance01(worldDist : float) : float {
		return camera.WorldToViewportPoint((worldDist-camera.nearClipPlane) * camera.transform.forward + camera.transform.position).z / (camera.farClipPlane-camera.nearClipPlane);	
	}
	
	function GetDividerBasedOnQuality () {
		var divider : int = 1;
		if (resolution == DofResolution.Medium)
			divider = 2;
		else if (resolution == DofResolution.Low)
			divider = 2;	
		return divider;	
	}
	
	function GetLowResolutionDividerBasedOnQuality (baseDivider : int) {
        var lowTexDivider : int = baseDivider;
        if (resolution == DofResolution.High)	
        	lowTexDivider *= 2;   
        if (resolution == DofResolution.Low)	
        	lowTexDivider *= 2; 	
        return lowTexDivider;	
	}
	
	function OnRenderImage (source : RenderTexture, destination : RenderTexture) {	
		CreateMaterials ();		
		
		// update needed focal & rt size parameter
		
		bokeh = bokeh && bokehSupport;

		var blurForeground : boolean = quality > Dof34QualitySetting.OnlyBackground;	
		var focal01Size : float = focalSize;

		if (simpleTweakMode) {		
			if (objectFocus)
				focalDistance01 = (camera.WorldToViewportPoint (objectFocus.position)).z / (camera.farClipPlane);
			else
				focalDistance01 = FocalDistance01 (focalPoint);
			
			var curve : float = focalDistance01 * smoothness;			
			focalStartCurve = curve;
			focalEndCurve = curve;
			focal01Size = 0.005f;
			blurForeground = blurForeground && (focalPoint > (camera.nearClipPlane + Mathf.Epsilon));
		} 
		else {
			if(objectFocus) {
				var vpPoint = camera.WorldToViewportPoint (objectFocus.position);
				vpPoint.z = (vpPoint.z) / (camera.farClipPlane);
				focalDistance01 = vpPoint.z;			
			} else {
				focalDistance01 = FocalDistance01 (focalZDistance);	
			}
			focalStartCurve = focalZStartCurve;
			focalEndCurve = focalZEndCurve;
			blurForeground = blurForeground && (focalPoint > (camera.nearClipPlane + Mathf.Epsilon));				
		}
		
		widthOverHeight = (1.0f * source.width) / (1.0f * source.height);
		oneOverBaseSize = 1.0f / 512.0f;		
								
        //  we use the alpha channel for storing the COC which also means that
        //  unfortunately, alpha based image effects such as sun shafts, bloom or glow
        //  might not work as expected if stacked after this image effect 
        		
		dofMaterial.SetFloat ("_ForegroundBlurExtrude", foregroundBlurExtrude);
		dofMaterial.SetVector ("_CurveParams", Vector4 (simpleTweakMode ? 1.0f / focalStartCurve : focalStartCurve, simpleTweakMode ? 1.0f / focalEndCurve : focalEndCurve, focal01Size * 0.5, focalDistance01));
		dofMaterial.SetVector ("_InvRenderTargetSize", Vector4 (1.0 / (1.0 * source.width), 1.0 / (1.0 * source.height),0.0,0.0));
		
		// needed render textures
		
		var divider : int =  GetDividerBasedOnQuality ();
        var lowTexDivider : int = GetLowResolutionDividerBasedOnQuality (divider);
		
        var foregroundTexture : RenderTexture = null;
        var foregroundDefocus : RenderTexture = null;
        if (blurForeground) {
        	foregroundTexture = RenderTexture.GetTemporary (source.width, source.height, 0); 
        	foregroundDefocus = RenderTexture.GetTemporary (source.width / divider, source.height / divider, 0);
        }    
		var mediumTexture : RenderTexture = RenderTexture.GetTemporary (source.width / divider, source.height / divider, 0);         
        var backgroundDefocus : RenderTexture = RenderTexture.GetTemporary (source.width / divider, source.height / divider, 0);    
        var lowTexture : RenderTexture  = RenderTexture.GetTemporary (source.width / lowTexDivider, source.height / lowTexDivider, 0);     
		var bokehSource : RenderTexture = null;
		var bokehSource2 : RenderTexture = null;
 		if (bokeh) {
        	bokehSource  = RenderTexture.GetTemporary (source.width / (lowTexDivider * bokehDownsample), source.height / (lowTexDivider * bokehDownsample), 0); 
        	bokehSource2  = RenderTexture.GetTemporary (source.width / (lowTexDivider * bokehDownsample), source.height / (lowTexDivider * bokehDownsample), 0); 
 		}    
        
        // just to make sure:
        
        source.filterMode = FilterMode.Bilinear;
        if (foregroundTexture) {
        	foregroundTexture.filterMode = FilterMode.Bilinear;   
       		foregroundDefocus.filterMode = FilterMode.Bilinear;
        }
        backgroundDefocus.filterMode = FilterMode.Bilinear;
        mediumTexture.filterMode = FilterMode.Bilinear;    
        lowTexture.filterMode = FilterMode.Bilinear;     
        if (bokeh) {
        	bokehSource.filterMode = FilterMode.Bilinear;
        	bokehSource2.filterMode = FilterMode.Bilinear;        	
        }
		
		// blur foreground if needed
				
		if (blurForeground) { 
			// foreground handling comes first (coc -> alpha channel)
			Graphics.Blit (source, foregroundTexture, dofMaterial, 5); 
			
			// better downsample and blur (shouldn't be weighted)
			Graphics.Blit (foregroundTexture, mediumTexture, dofMaterial, 6);					
			Blur (mediumTexture, mediumTexture, 1, 1, foregroundMaxBlurSpread);	
			if (bokehSource) {
				dofMaterial.SetVector ("_InvRenderTargetSize", Vector4 (1.0f / (1.0f * bokehSource.width), 1.0f / (1.0f * bokehSource.height), 0.0f, 0.0f));
				Graphics.Blit (mediumTexture, bokehSource, dofMaterial, 6);
			}
			Blur (mediumTexture, lowTexture, foregroundBlurIterations, 1, foregroundMaxBlurSpread);	
			
			// some final FG calculations can be performed in low resolution: 		
		
			dofBlurMaterial.SetTexture ("_TapLow", lowTexture);
			dofBlurMaterial.SetTexture ("_TapMedium", mediumTexture);							
			Graphics.Blit (null, foregroundDefocus, dofBlurMaterial, 3);			
			
	        // background (coc -> alpha channel)
			// @NOTE: this is safe, we are not sampling from "source"			
	       	Graphics.Blit (source, source, dofMaterial, 3);
	       		
	       	// better downsample (should actually be weighted for higher quality)
	       	Graphics.Blit (source, mediumTexture, dofMaterial, 6);	
		} 
		else {
			// @NOTE: this is safe, we are not sampling from "source"
			Graphics.Blit (source, source, dofMaterial, 3); 
				
	       	// better downsample (could actually be weighted for higher quality)
	       	Graphics.Blit (source, mediumTexture, dofMaterial, 6);				
		}
				
       	// blur background
       	            	            	     
		Blur (mediumTexture, mediumTexture, 1, 0, maxBlurSpread);	
		if (bokehSource) {
			if (!blurForeground) {
				dofMaterial.SetVector ("_InvRenderTargetSize", Vector4 (1.0f / (1.0f * bokehSource.width), 1.0f / (1.0f * bokehSource.height), 0.0f, 0.0f));
				Graphics.Blit (mediumTexture, bokehSource2, dofMaterial, 6);				
			} 
			else {
				dofMaterial.SetTexture ("_TapMedium", mediumTexture);			
				Graphics.Blit (bokehSource, bokehSource2, dofMaterial, 8);
			}
		}		
		Blur (mediumTexture, lowTexture, blurIterations, 0, maxBlurSpread);
       		
		dofBlurMaterial.SetTexture ("_TapLow", lowTexture);
		dofBlurMaterial.SetTexture ("_TapMedium", mediumTexture);							
		Graphics.Blit (null, backgroundDefocus, dofBlurMaterial, 3);	
				
		if (bokeh && bokehMaterial) {
			var meshes : Mesh[] = Triangles.GetMeshes (bokehSource.width, bokehSource.height);		
			
			GL.PushMatrix ();
			GL.LoadIdentity ();
			
			RenderTexture.active = bokehSource;
			GL.Clear (false, true, Color (0.0f,0.0f,0.0f,0.0f));
			
			bokehMaterial.SetTexture ("_Source", bokehSource2);//blurForeground ? mediumTexture : backgroundDefocus);
			bokehMaterial.SetVector ("_ArScale", Vector4(1.0f, (mediumTexture.width * 1.0f) / (mediumTexture.height * 1.0f), 1.0f, 1.0f));
			bokehMaterial.SetFloat ("_Scale", bokehScale * 0.01f);
			bokehMaterial.SetFloat ("_Intensity", bokehIntensity * 0.01f);
			bokehMaterial.SetFloat ("_Threshhold", bokehThreshhold);
			bokehMaterial.SetTexture ("_MainTex", bokehTexture);
			bokehMaterial.SetPass (0);	
			
			for (var m : Mesh in meshes)
				if (m) Graphics.DrawMeshNow (m, Matrix4x4.identity);	
	
			GL.PopMatrix ();
		
			// blend bokeh result into low resolution texture(s)
						
			dofMaterial.SetFloat ("_BlendStrength", bokehBlendStrength);
			Graphics.Blit (bokehSource, backgroundDefocus, dofMaterial, 9);    		
			if (blurForeground) 
				Graphics.Blit (bokehSource, foregroundDefocus, dofMaterial, 9);   
		}
		
		dofMaterial.SetTexture ("_TapLowForeground", blurForeground ? foregroundDefocus : null);
		dofMaterial.SetTexture ("_TapLowBackground", backgroundDefocus); 
		dofMaterial.SetTexture ("_TapMedium", mediumTexture); // needed for debugging/visualization
			
		// defocus for background
		Graphics.Blit (source, blurForeground ? foregroundTexture : destination, dofMaterial, visualize ? 2 : 0); 
		
		// defocus for foreground
		if (blurForeground) 			
			Graphics.Blit (foregroundTexture, destination, dofMaterial, visualize ? 1 : 4);
								
		if (foregroundTexture) RenderTexture.ReleaseTemporary (foregroundTexture);
		if (foregroundDefocus) RenderTexture.ReleaseTemporary (foregroundDefocus);
		RenderTexture.ReleaseTemporary (backgroundDefocus);
		RenderTexture.ReleaseTemporary (mediumTexture);
		RenderTexture.ReleaseTemporary (lowTexture);
		if (bokehSource) RenderTexture.ReleaseTemporary (bokehSource);
		if (bokehSource2) RenderTexture.ReleaseTemporary (bokehSource2);			
	}
	
	function Blur (from : RenderTexture, to : RenderTexture, iterations : int, blurPass: int, spread : float) 
	{
		var tmp : RenderTexture = RenderTexture.GetTemporary (to.width, to.height);	
		
		if (iterations>1)
			BlurHex (from, to, iterations/2, blurPass, spread, tmp);
													
		for (var it : int = 0; it < iterations % 2; it++) {
			dofBlurMaterial.SetVector ("offsets", Vector4 (0.0, spread * oneOverBaseSize, 0.0, 0.0));
			Graphics.Blit ((it == 0 && iterations <= 1) ? from : to, tmp, dofBlurMaterial, blurPass);
			dofBlurMaterial.SetVector ("offsets", Vector4 (spread / widthOverHeight * oneOverBaseSize,  0.0, 0.0, 0.0));		
			Graphics.Blit (tmp, to, dofBlurMaterial, blurPass);	 
		}	
			
		RenderTexture.ReleaseTemporary (tmp);
	}

	function BlurHex (from : RenderTexture, to : RenderTexture, iterations : int, blurPass: int, spread : float, tmp : RenderTexture) {		
		for (var it : int = 0; it < iterations; it++) {
			dofBlurMaterial.SetVector ("offsets", Vector4 (0.0, spread * oneOverBaseSize, 0.0, 0.0));
			Graphics.Blit ( ((it == 0)) ? from : to, tmp, dofBlurMaterial, blurPass);
			dofBlurMaterial.SetVector ("offsets", Vector4 (spread / widthOverHeight * oneOverBaseSize,  0.0, 0.0, 0.0));		
			Graphics.Blit (tmp, to, dofBlurMaterial, blurPass);	 
			dofBlurMaterial.SetVector ("offsets", Vector4 (spread / widthOverHeight * oneOverBaseSize,  spread * oneOverBaseSize, 0.0, 0.0));		
			Graphics.Blit (to, tmp, dofBlurMaterial, blurPass);	 
			dofBlurMaterial.SetVector ("offsets", Vector4 (spread / widthOverHeight * oneOverBaseSize,  -spread * oneOverBaseSize, 0.0, 0.0));		
			Graphics.Blit (tmp, to, dofBlurMaterial, blurPass);	 
		}				
	}
}
