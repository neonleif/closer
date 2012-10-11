
#pragma strict

@CustomEditor (AntialiasingAsPostEffect)

class AntialiasingAsPostEffectEditor extends Editor 
{	
	var serObj : SerializedObject;	
		
	var mode : SerializedProperty;
	
	var showGeneratedNormals : SerializedProperty;
	var offsetScale : SerializedProperty;
	var blurRadius : SerializedProperty;

	function OnEnable () {
		serObj = new SerializedObject (target);
		
		mode = serObj.FindProperty ("mode");
		
		showGeneratedNormals = serObj.FindProperty ("showGeneratedNormals");
		offsetScale = serObj.FindProperty ("offsetScale");
		blurRadius = serObj.FindProperty ("blurRadius");

	}
    		
    function OnInspectorGUI () {        
    	serObj.Update ();
    	
		GUILayout.Label("Various luminance based fullscreen Antialiasing techniques", EditorStyles.miniBoldLabel);
    	
    	EditorGUILayout.PropertyField (mode, new GUIContent ("AA Technique"));

		if (mode.enumValueIndex == AAMode.NFAA) {
			EditorGUILayout.Separator ();  	
    		EditorGUILayout.PropertyField (offsetScale, new GUIContent ("Edge Detect Ofs"));
    		EditorGUILayout.PropertyField (blurRadius, new GUIContent ("Blur Radius"));
    		EditorGUILayout.PropertyField (showGeneratedNormals, new GUIContent ("Show Normals"));	
		}
    	
    	serObj.ApplyModifiedProperties();
    }
}
