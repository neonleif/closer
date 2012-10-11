using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpecularLighting))]
public class SpecularLightingEditor : Editor 
{    
    private SerializedObject serObj;
    private SerializedProperty specularLight;
    
	public void OnEnable () {
		serObj = new SerializedObject (target); 
		specularLight = serObj.FindProperty("specularLight");   		
	}
	
    public override void OnInspectorGUI () 
    {
    	serObj.Update();
    	
    	GameObject go = ((SpecularLighting)serObj.targetObject).gameObject;
    	WaterBase wb = (WaterBase)go.GetComponent(typeof(WaterBase));
    	
    	if(wb.sharedMaterial.HasProperty("_WorldLightDir")) {
    		GUILayout.Label ("Currently using a forward shader, chose a Transform for specular highlights", EditorStyles.miniBoldLabel);    		
    		EditorGUILayout.PropertyField(specularLight, new GUIContent("Specular light"));
    	}
    	else
    		GUILayout.Label ("This material is using a surface shader.\nSpecular lighting will be applied automatically.", EditorStyles.miniBoldLabel);
    	
    	serObj.ApplyModifiedProperties();
    }
    
}