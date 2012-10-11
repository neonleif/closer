using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PerformanceSettings))]
public class PerformanceSettingsEditor : Editor 
{    
    private SerializedObject serObj;

	public SerializedProperty waterQuality;
	public SerializedProperty edgeBlend;
    
	public void OnEnable () {
		serObj = new SerializedObject (target); 
	
		waterQuality = serObj.FindProperty("waterQuality");   		
		edgeBlend = serObj.FindProperty("edgeBlend");   		

	}
	
    public override void OnInspectorGUI () 
    {
        GUILayout.Label ("Tweak shader LOD and compilation settings for max performance", EditorStyles.miniBoldLabel);    	
    	
    	serObj.Update();
    	   		
   		EditorGUILayout.PropertyField(waterQuality, new GUIContent("Quality"));
    	
    	//EditorGUILayout.BeginHorizontal();
   		EditorGUILayout.PropertyField(edgeBlend, new GUIContent("Edge blend?"));    	
    	//EditorGUILayout.EndHorizontal();
    	
    	serObj.ApplyModifiedProperties();
    }
    
}