
#pragma strict
@script ExecuteInEditMode
@script AddComponentMenu ("Image Effects/Color Correction")

enum ColorCorrectionMode {
	Simple = 0,
	Advanced = 1	
}

class ColorCorrectionCurves extends PostEffectsBase 
{
	public var redChannel : AnimationCurve;// = new AnimationCurve(Keyframe(0, 0.0, 1.0, 1.0), Keyframe(1, 1.0, 1.0, 1.0));
	public var greenChannel : AnimationCurve;// = new AnimationCurve(Keyframe(0, 0.0, 1.0, 1.0), Keyframe(1, 1.0, 1.0, 1.0));
	public var blueChannel : AnimationCurve;// = new AnimationCurve(Keyframe(0, 0.0, 1.0, 1.0), Keyframe(1, 1.0, 1.0, 1.0));
	
	public var useDepthCorrection : boolean = false;
	
	public var zCurveChannel : AnimationCurve;// = new AnimationCurve(Keyframe(0, 0.0, 1.0, 1.0), Keyframe(1, 1.0, 1.0, 1.0));
	public var depthRedChannel : AnimationCurve;// = new AnimationCurve(Keyframe(0, 0.0, 1.0, 1.0), Keyframe(1, 1.0, 1.0, 1.0));
	public var depthGreenChannel : AnimationCurve;// = new AnimationCurve(Keyframe(0, 0.0, 1.0, 1.0), Keyframe(1, 1.0, 1.0, 1.0));
	public var depthBlueChannel : AnimationCurve;// = new AnimationCurve(Keyframe(0, 0.0, 1.0, 1.0), Keyframe(1, 1.0, 1.0, 1.0));
	
	private var ccMaterial : Material;
	private var ccDepthMaterial : Material;
	private var selectiveCcMaterial : Material;
	
	private var rgbChannelTex : Texture2D;
	private var rgbDepthChannelTex : Texture2D;
	
	private var zCurve : Texture2D;
	
	public var selectiveCc : boolean = false;
	
	public var selectiveFromColor : Color = Color.white;
	public var selectiveToColor : Color = Color.white;
	
	public var mode : ColorCorrectionMode;
	
	public var updateTextures : boolean = true;		
		
	public var colorCorrectionCurvesShader : Shader = null;
	public var simpleColorCorrectionCurvesShader : Shader = null;
	public var colorCorrectionSelectiveShader : Shader = null;
	
	private var initialUpdate : boolean = true;
		
	function Start () {
		CheckSupport (true);
	}
	
	function CreateMaterials () {
		ccMaterial = CheckShaderAndCreateMaterial (simpleColorCorrectionCurvesShader, ccMaterial);
		ccDepthMaterial = CheckShaderAndCreateMaterial (colorCorrectionCurvesShader, ccDepthMaterial);
		selectiveCcMaterial = CheckShaderAndCreateMaterial (colorCorrectionSelectiveShader, selectiveCcMaterial);
		
		if (!redChannel)
			 redChannel = new AnimationCurve(Keyframe(0, 0.0, 1.0, 1.0), Keyframe(1, 1.0, 1.0, 1.0));
		if (!greenChannel)
			 greenChannel = new AnimationCurve(Keyframe(0, 0.0, 1.0, 1.0), Keyframe(1, 1.0, 1.0, 1.0));
		if (!blueChannel)
			 blueChannel = new AnimationCurve(Keyframe(0, 0.0, 1.0, 1.0), Keyframe(1, 1.0, 1.0, 1.0));
		if (!zCurveChannel)
			 zCurveChannel = new AnimationCurve(Keyframe(0, 0.0, 1.0, 1.0), Keyframe(1, 1.0, 1.0, 1.0));
		if (!depthRedChannel)
			 depthRedChannel = new AnimationCurve(Keyframe(0, 0.0, 1.0, 1.0), Keyframe(1, 1.0, 1.0, 1.0));
		if (!depthGreenChannel)
			 depthGreenChannel = new AnimationCurve(Keyframe(0, 0.0, 1.0, 1.0), Keyframe(1, 1.0, 1.0, 1.0));
		if (!depthBlueChannel)
			 depthBlueChannel = new AnimationCurve(Keyframe(0, 0.0, 1.0, 1.0), Keyframe(1, 1.0, 1.0, 1.0));

		if (!rgbChannelTex) {
			rgbChannelTex = new Texture2D(256, 4, TextureFormat.ARGB32, false);
		}
		if (!rgbDepthChannelTex) {
			rgbDepthChannelTex = new Texture2D(256, 4, TextureFormat.ARGB32, false);
		}
		
		if (!zCurve) {
			zCurve = new Texture2D (256, 1, TextureFormat.ARGB32, false);
		}	
		
		rgbChannelTex.hideFlags = HideFlags.DontSave;
		rgbDepthChannelTex.hideFlags = HideFlags.DontSave;
		zCurve.hideFlags = HideFlags.DontSave;
		
		rgbChannelTex.wrapMode = TextureWrapMode.Clamp;
		rgbDepthChannelTex.wrapMode = TextureWrapMode.Clamp;
		zCurve.wrapMode = TextureWrapMode.Clamp;		
	}
	
	function OnEnable () {
		if(useDepthCorrection)
			camera.depthTextureMode |= DepthTextureMode.Depth;	
	}
	
	function OnDisable () {
	}
	
	public function UpdateParameters () 
	{			
		if (redChannel && greenChannel && blueChannel) {
			for (var i : float = 0.0; i <= 1.0; i += 1.0/255.0) {
				var rCh : float = Mathf.Clamp(redChannel.Evaluate(i), 0.0,1.0);
				var gCh : float = Mathf.Clamp(greenChannel.Evaluate(i), 0.0,1.0);
				var bCh : float = Mathf.Clamp(blueChannel.Evaluate(i), 0.0,1.0);
				
				rgbChannelTex.SetPixel( Mathf.Floor(i*255.0), 0, Color(rCh,rCh,rCh) );
				rgbChannelTex.SetPixel( Mathf.Floor(i*255.0), 1, Color(gCh,gCh,gCh) );
				rgbChannelTex.SetPixel( Mathf.Floor(i*255.0), 2, Color(bCh,bCh,bCh) );
				
				var zC : float = Mathf.Clamp(zCurveChannel.Evaluate(i), 0.0,1.0);
					
				zCurve.SetPixel( Mathf.Floor(i*255.0), 0, Color(zC,zC,zC) );
			
				rCh = Mathf.Clamp(depthRedChannel.Evaluate(i), 0.0,1.0);
				gCh = Mathf.Clamp(depthGreenChannel.Evaluate(i), 0.0,1.0);
				bCh = Mathf.Clamp(depthBlueChannel.Evaluate(i), 0.0,1.0);
				
				rgbDepthChannelTex.SetPixel( Mathf.Floor(i*255.0), 0, Color(rCh,rCh,rCh) );
				rgbDepthChannelTex.SetPixel( Mathf.Floor(i*255.0), 1, Color(gCh,gCh,gCh) );
				rgbDepthChannelTex.SetPixel( Mathf.Floor(i*255.0), 2, Color(bCh,bCh,bCh) );
			}
			
			rgbChannelTex.Apply();
			rgbDepthChannelTex.Apply();
			zCurve.Apply();				
		}
	}
	
	function UpdateTextures () {
		UpdateParameters ();			
	}
	
	function OnRenderImage (source : RenderTexture, destination : RenderTexture) {
		CreateMaterials ();
		
		if (initialUpdate) {
			initialUpdate = false;
			UpdateParameters ();
		}
		
		if (useDepthCorrection)
			camera.depthTextureMode |= DepthTextureMode.Depth;			
		
		var renderTarget2Use : RenderTexture = destination;
		
		if (selectiveCc) {
			renderTarget2Use = RenderTexture.GetTemporary (source.width, source.height);
		}
		
		if (useDepthCorrection) {
			ccDepthMaterial.SetTexture ("_RgbTex", rgbChannelTex);
			ccDepthMaterial.SetTexture ("_ZCurve", zCurve);
			ccDepthMaterial.SetTexture ("_RgbDepthTex", rgbDepthChannelTex);
	
			Graphics.Blit (source, renderTarget2Use, ccDepthMaterial); 	
		} 
		else {
			ccMaterial.SetTexture ("_RgbTex", rgbChannelTex);
			
			Graphics.Blit (source, renderTarget2Use, ccMaterial); 			
		}
		
		if (selectiveCc) {
			selectiveCcMaterial.SetColor ("selColor", selectiveFromColor);
			selectiveCcMaterial.SetColor ("targetColor", selectiveToColor);
			Graphics.Blit (renderTarget2Use, destination, selectiveCcMaterial); 	
			
			RenderTexture.ReleaseTemporary (renderTarget2Use);
		}
				
	}

}