
#pragma strict

@CustomEditor (DepthOfField34)
class DepthOfField34Editor extends Editor 
{	
	var serObj : SerializedObject;	

	var simpleTweakMode : SerializedProperty;
		
	var focalPoint : SerializedProperty;
	var smoothness : SerializedProperty;
	
	var focalSize : SerializedProperty;

	var focalZDistance : SerializedProperty;
	var focalStartCurve : SerializedProperty;
	var focalEndCurve : SerializedProperty;

	var visualizeCoc : SerializedProperty;

	var resolution : SerializedProperty;
	var quality : SerializedProperty;
	
	var objectFocus : SerializedProperty;
	
	var bokeh : SerializedProperty;
	var bokehScale : SerializedProperty;
	var bokehIntensity : SerializedProperty;
	var bokehThreshhold : SerializedProperty;
	var bokehBlendStrength : SerializedProperty;
	var bokehDownsample : SerializedProperty;
    var bokehTexture : SerializedProperty;
	
	var blurIterations : SerializedProperty;
	var foregroundBlurIterations : SerializedProperty;	
	var foregroundMaxBlurSpread : SerializedProperty;
	var maxBlurSpread : SerializedProperty;	
	var foregroundBlurExtrude : SerializedProperty;

	function OnEnable () {
		serObj = new SerializedObject (target);
		
		simpleTweakMode = serObj.FindProperty ("simpleTweakMode"); 
		
		// simple tweak mode
		focalPoint = serObj.FindProperty ("focalPoint");
		smoothness = serObj.FindProperty ("smoothness");
		
		// complex tweak mode
		focalZDistance = serObj.FindProperty ("focalZDistance");
		focalStartCurve = serObj.FindProperty ("focalZStartCurve");
		focalEndCurve = serObj.FindProperty ("focalZEndCurve");
		focalSize = serObj.FindProperty ("focalSize");
		
		visualizeCoc = serObj.FindProperty ("visualize");
		
		objectFocus = serObj.FindProperty ("objectFocus");
		
		resolution = serObj.FindProperty ("resolution");
		quality = serObj.FindProperty ("quality");
		bokehThreshhold = serObj.FindProperty ("bokehThreshhold");
	
		bokeh = serObj.FindProperty ("bokeh");
		bokehScale = serObj.FindProperty ("bokehScale");
		bokehIntensity = serObj.FindProperty ("bokehIntensity");
		bokehBlendStrength = serObj.FindProperty ("bokehBlendStrength");
		bokehDownsample = serObj.FindProperty ("bokehDownsample");
		bokehTexture = serObj.FindProperty ("bokehTexture");
		
		blurIterations = serObj.FindProperty ("blurIterations");
		foregroundBlurIterations = serObj.FindProperty ("foregroundBlurIterations");
		foregroundMaxBlurSpread = serObj.FindProperty ("foregroundMaxBlurSpread");
		maxBlurSpread = serObj.FindProperty ("maxBlurSpread");	
		foregroundBlurExtrude = serObj.FindProperty ("foregroundBlurExtrude");
	}
    		
    function OnInspectorGUI () {         
    	serObj.Update ();
    	
    	var go : GameObject = (target as DepthOfField34).gameObject;
    	
    	if (!go)
    		return;
    		
    	if (!go.camera)
    		return;
    		    		
    	if (simpleTweakMode.boolValue)
    		GUILayout.Label ("Current: "+go.camera.name+", near "+go.camera.nearClipPlane+", far: "+go.camera.farClipPlane+", focal: "+focalPoint.floatValue, EditorStyles.miniBoldLabel);
    	else
    		GUILayout.Label ("Current: "+go.camera.name+", near "+go.camera.nearClipPlane+", far: "+go.camera.farClipPlane+", focal: "+focalZDistance.floatValue, EditorStyles.miniBoldLabel);
    	
    	GUILayout.Label ("General Settings", EditorStyles.boldLabel);    	
    	
   		EditorGUILayout.PropertyField (resolution, new GUIContent("Resolution"));
   		EditorGUILayout.PropertyField (quality, new GUIContent("Quality"));
    	
		EditorGUILayout.PropertyField (simpleTweakMode, new GUIContent("Simple tweak"));  
		EditorGUILayout.PropertyField (visualizeCoc, new GUIContent("Visualize focus"));  		  	
   		EditorGUILayout.PropertyField (bokeh, new GUIContent("Enable bokeh"));  
 	

   		EditorGUILayout.Separator ();

    	GUILayout.Label ("Focal Settings", EditorStyles.boldLabel);    	
		GUILayout.Label ("Chose either a fixed z distance or a transform", EditorStyles.miniBoldLabel);
    	
    	if (simpleTweakMode.boolValue) {
   			focalPoint.floatValue = EditorGUILayout.Slider ("Distance", focalPoint.floatValue, go.camera.nearClipPlane, go.camera.farClipPlane);
			EditorGUILayout.PropertyField (objectFocus, new GUIContent("Transform"));
   			EditorGUILayout.PropertyField (smoothness, new GUIContent("Smoothness"));
			
    	}
    	else {
			focalZDistance.floatValue = EditorGUILayout.Slider ("Distance", focalZDistance.floatValue, go.camera.nearClipPlane, go.camera.farClipPlane);  
			EditorGUILayout.PropertyField (objectFocus, new GUIContent("Transform"));			
			focalSize.floatValue = EditorGUILayout.Slider ("Size", focalSize.floatValue, 0.0f, 50.0f / (go.camera.farClipPlane - go.camera.nearClipPlane));  
			focalStartCurve.floatValue = EditorGUILayout.Slider ("Start curve", focalStartCurve.floatValue, 0.05f, 20.0f);  
			focalEndCurve.floatValue = EditorGUILayout.Slider ("End curve", focalEndCurve.floatValue, 0.05f, 20.0f);  
    	}
    	
    	if (bokeh.boolValue) {
   			EditorGUILayout.Separator ();
	    	GUILayout.Label ("Bokeh Settings", EditorStyles.boldLabel);    	
    		bokehIntensity.floatValue = EditorGUILayout.Slider ("Intensity", bokehIntensity.floatValue, 1.0f, 20.0f);  
    		bokehThreshhold.floatValue = EditorGUILayout.Slider ("Threshhold", bokehThreshhold.floatValue, 0.0f, 1.0f);  
    		bokehDownsample.intValue = EditorGUILayout.IntSlider ("Downsample", bokehDownsample.intValue, 1, 3);  
    		bokehScale.floatValue = EditorGUILayout.Slider ("Size", bokehScale.floatValue, 1.0f, 20.0f);     	
    		bokehBlendStrength.floatValue = EditorGUILayout.Slider ("Blend Strength", bokehBlendStrength.floatValue, 0.0f, 4.0f); 	
    		EditorGUILayout.PropertyField (bokehTexture , new GUIContent("Texture mask"));	
    	}
    	    	
   		EditorGUILayout.Separator ();
   		
   		GUILayout.Label ("Background Blur", EditorStyles.boldLabel);  
   		blurIterations.intValue = EditorGUILayout.Slider ("Iterations", blurIterations.intValue, 1, 4);
   		EditorGUILayout.PropertyField (maxBlurSpread, new GUIContent("Blur spread"));
				
		if (quality.enumValueIndex > 0) {
			EditorGUILayout.Separator ();
		
			GUILayout.Label ("Foreground Blur", EditorStyles.boldLabel);  
			foregroundBlurIterations.intValue = EditorGUILayout.Slider ("Iterations", foregroundBlurIterations.intValue, 1, 4);
			EditorGUILayout.PropertyField (foregroundMaxBlurSpread, new GUIContent("Blur spread"));
			EditorGUILayout.PropertyField (foregroundBlurExtrude, new GUIContent("Extrude"));	
		}
    	    	
    	serObj.ApplyModifiedProperties();
    }
}