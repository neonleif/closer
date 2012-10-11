using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ReflectionRefraction))]
public class ReflectionRefractionEditor : Editor 
{    
    private SerializedObject serObj;
    
    //private SerializedProperty wavesFrequency;
    
	// reflection
	private SerializedProperty reflectionMask;
	private SerializedProperty reflectSkybox;
	private SerializedProperty clearColor;
	
	// refraction
	private SerializedProperty refractionMask;
	private SerializedProperty useGrabPassForRefraction;
	//private SerializedProperty refractionSampler;

	// height
	private SerializedProperty waterHeight;
	private SerializedProperty clipPlaneOffset;
    
	public void OnEnable () {
		serObj = new SerializedObject (target); 
		
		reflectionMask = serObj.FindProperty("reflectionMask");   		
		reflectSkybox = serObj.FindProperty("reflectSkybox");   		
		clearColor = serObj.FindProperty("clearColor");   		

		refractionMask = serObj.FindProperty("refractionMask");   		
		useGrabPassForRefraction = serObj.FindProperty("useGrabPassForRefraction");   		

		waterHeight = serObj.FindProperty("waterHeight");   		
		clipPlaneOffset = serObj.FindProperty("clipPlaneOffset");   		
	}
	
    public override void OnInspectorGUI () 
    {
        GUILayout.Label ("Render planar reflections and refractions and put them into textures", EditorStyles.miniBoldLabel);    	
    	
    	serObj.Update();
    	
    	EditorGUILayout.PropertyField(reflectionMask, new GUIContent("Reflection layers"));
    	EditorGUILayout.PropertyField(reflectSkybox, new GUIContent("Use skybox"));
    	//if(!reflectSkybox.boolValue)
		EditorGUILayout.PropertyField(clearColor, new GUIContent("Clear color"));
			
		EditorGUILayout.Separator ();

    	EditorGUILayout.PropertyField(useGrabPassForRefraction, new GUIContent("Use GrabPass"));
    	if(!useGrabPassForRefraction.boolValue)
    		EditorGUILayout.PropertyField(refractionMask, new GUIContent("Refraction layers"));
    	
    	EditorGUILayout.Separator ();
    	
    	EditorGUILayout.PropertyField(waterHeight, new GUIContent("Height"));
    	EditorGUILayout.PropertyField(clipPlaneOffset, new GUIContent("Clip plane offset"));
    	
    	serObj.ApplyModifiedProperties();
    }
    
}