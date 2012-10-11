using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WaterBase))]
public class WaterBaseEditor : Editor 
{    
    public GameObject oceanBase;
    private WaterBase waterBase;
    private Material oceanMaterial = null;
    
    private SerializedObject serObj;
    private SerializedProperty sharedMaterial;
    
	public void OnEnable () {
		serObj = new SerializedObject (target); 
		sharedMaterial = serObj.FindProperty("sharedMaterial"); 
	}
	
    public override void OnInspectorGUI () 
    {
    	serObj.Update();
    	    	
    	oceanBase = ((WaterBase)serObj.targetObject).gameObject;
    	if(!oceanBase) 
      		return;	

        GUILayout.Label ("Tweak standard shader values as stored in the materials' property list", EditorStyles.miniBoldLabel);
    	
    	waterBase = (WaterBase)oceanBase.GetComponent(typeof(WaterBase));
    	
       	if(!waterBase) {
       		GUILayout.Label ("No WaterBase component found");	
	    	EditorGUILayout.EndScrollView();   					 		
    		return;	
    	} 	
    	
    	EditorGUILayout.PropertyField(sharedMaterial, new GUIContent("Material"));
    	oceanMaterial = (Material)sharedMaterial.objectReferenceValue;

		if (!oceanMaterial)
	        return;
	        
		EditorGUILayout.Separator ();
		
    	GUILayout.Label ("Main Textures", EditorStyles.boldLabel);  	

		EditorGUILayout.BeginHorizontal();
		WaterEditorUtility.SetMaterialTexture("_BumpMap",(Texture)EditorGUILayout.ObjectField("Bump", WaterEditorUtility.GetMaterialTexture("_BumpMap", waterBase.sharedMaterial), typeof(Texture), false), waterBase.sharedMaterial);  
		WaterEditorUtility.SetMaterialTexture("_ShoreTex", (Texture)EditorGUILayout.ObjectField("Shore/Foam", WaterEditorUtility.GetMaterialTexture("_ShoreTex", waterBase.sharedMaterial), typeof(Texture), false), waterBase.sharedMaterial);  
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.Separator ();

		Vector4 animationTiling;
		Vector4 animationDirection;
		Vector2 firstTiling;
		Vector2 secondTiling;
		Vector2 firstDirection;
		Vector2 secondDirection;
    	
    	GUILayout.Label ("Bump Animation (Small Waves)", EditorStyles.boldLabel);  	
    	
		animationTiling = WaterEditorUtility.GetMaterialVector("_BumpTiling", oceanMaterial);
		animationDirection = WaterEditorUtility.GetMaterialVector("_BumpDirection", oceanMaterial);
		
		firstTiling = new Vector2(animationTiling.x*100.0F,animationTiling.y*100.0F);
		secondTiling = new Vector2(animationTiling.z*100.0F,animationTiling.w*100.0F);

		firstDirection = new Vector2(animationDirection.x,animationDirection.y);
		secondDirection = new Vector2(animationDirection.z,animationDirection.w);

		firstTiling = EditorGUILayout.Vector2Field("First bump tiling", firstTiling);
		secondTiling = EditorGUILayout.Vector2Field("Second bump tiling", secondTiling);
		animationTiling = new Vector4(firstTiling.x/100.0F,firstTiling.y/100.0F, secondTiling.x/100.0F,secondTiling.y/100.0F);

		firstDirection = EditorGUILayout.Vector2Field("First bump direction", firstDirection);
		secondDirection = EditorGUILayout.Vector2Field("Second bump direction", secondDirection);
		animationDirection = new Vector4(firstDirection.x,firstDirection.y, secondDirection.x,secondDirection.y);
		
		WaterEditorUtility.SetMaterialVector("_BumpTiling", animationTiling, oceanMaterial);
		WaterEditorUtility.SetMaterialVector("_BumpDirection", animationDirection, oceanMaterial);    	

		EditorGUILayout.Separator ();
		
		
    	GUILayout.Label ("Bump intensity (Small Waves)", EditorStyles.boldLabel);				

		Vector4 displacementParameter = WaterEditorUtility.GetMaterialVector("_DistortParams", oceanMaterial);
		
		displacementParameter.x = EditorGUILayout.Slider("Overall", displacementParameter.x, -2.0F, 2.0F);
		displacementParameter.y = EditorGUILayout.Slider("Realtime textures", displacementParameter.y, -2.0F, 2.0F);
		
		EditorGUILayout.Separator ();
		
		
    	GUILayout.Label ("Fresnel & Fading", EditorStyles.boldLabel);				
		
		if(!oceanMaterial.HasProperty("_Fresnel")) {
			if(oceanMaterial.HasProperty("_FresnelScale")) {
				float fresnelScale = EditorGUILayout.Slider("Intensity", WaterEditorUtility.GetMaterialFloat("_FresnelScale", oceanMaterial), 0.1F, 4.0F);
				WaterEditorUtility.SetMaterialFloat("_FresnelScale", fresnelScale, oceanMaterial);
			}			
			displacementParameter.z = EditorGUILayout.Slider("Power", displacementParameter.z, 0.1F, 10.0F);
			displacementParameter.w = EditorGUILayout.Slider("Bias", displacementParameter.w, -3.0F, 3.0F);
		}
		else
		{
			Texture fresnelTex = (Texture)EditorGUILayout.ObjectField(
					"Ramp", 
					(Texture)WaterEditorUtility.GetMaterialTexture("_Fresnel", 
					oceanMaterial), 
					typeof(Texture),
					false);
			WaterEditorUtility.SetMaterialTexture("_Fresnel", fresnelTex, oceanMaterial);
		}
		
		WaterEditorUtility.SetMaterialVector("_DistortParams", displacementParameter, oceanMaterial);

		if(oceanMaterial.HasProperty("_InvFadeParemeter")) {
			Vector4 fade = WaterEditorUtility.GetMaterialVector("_InvFadeParemeter", oceanMaterial);
			
			fade.x = EditorGUILayout.Slider("Edge fade", fade.x, 0.0f, 0.3f);
			fade.y = EditorGUILayout.Slider("Shore fade", fade.y, 0.0f, 0.3f);			
			
			WaterEditorUtility.SetMaterialVector("_InvFadeParemeter", fade, oceanMaterial);
			EditorGUILayout.Separator ();					
		}
								
		if(oceanMaterial.HasProperty("_Foam")) {
    		GUILayout.Label ("Foam", EditorStyles.boldLabel);		
		
			Vector4 foam = WaterEditorUtility.GetMaterialVector("_Foam", oceanMaterial);
			
			foam.x = EditorGUILayout.Slider("Foam intensity", foam.x, 0.0F, 40.0F);
			foam.y = EditorGUILayout.Slider("Foam cutoff", foam.y, 0.0F, 1.0F);
			
			WaterEditorUtility.SetMaterialVector("_Foam", foam, oceanMaterial);
			
			EditorGUILayout.Separator ();
		}


    	GUILayout.Label ("Shading & Colors", EditorStyles.boldLabel);		
        GUILayout.Label ("Alpha values define blending from realtime reflection/refraction", EditorStyles.miniBoldLabel);
		
		WaterEditorUtility.SetMaterialColor("_BaseColor", EditorGUILayout.ColorField("Refraction", WaterEditorUtility.GetMaterialColor("_BaseColor", oceanMaterial)), oceanMaterial);
		if(oceanMaterial.HasProperty("_DepthColor"))
		WaterEditorUtility.SetMaterialColor("_DepthColor", EditorGUILayout.ColorField("Depth", WaterEditorUtility.GetMaterialColor("_DepthColor", oceanMaterial)), oceanMaterial);
		if(oceanMaterial.HasProperty("_SpecularColor"))
			WaterEditorUtility.SetMaterialColor("_SpecularColor", EditorGUILayout.ColorField("Specular", WaterEditorUtility.GetMaterialColor("_SpecularColor", oceanMaterial)), oceanMaterial);
		WaterEditorUtility.SetMaterialColor("_ReflectionColor", EditorGUILayout.ColorField("Reflection", WaterEditorUtility.GetMaterialColor("_ReflectionColor", oceanMaterial)), oceanMaterial);
	
		if(oceanMaterial.HasProperty("_Shininess"))
			WaterEditorUtility.SetMaterialFloat("_Shininess", EditorGUILayout.Slider("Specular power", WaterEditorUtility.GetMaterialFloat("_Shininess", oceanMaterial), 0.0F, 500.0F), oceanMaterial);
		
		// other (tweakable) components
		
		//if(oceanMaterial.HasProperty("_SpecularColor"))
		//	WaterEditorUtility.CheckForSpecularComponent(oceanBase);
		//WaterEditorUtility.CheckForOceanPerformanceComponent(oceanBase);		
		//WaterEditorUtility.CheckForReflectionRefractionComponent(oceanBase);	
				
    	serObj.ApplyModifiedProperties();
		
    }
       
}