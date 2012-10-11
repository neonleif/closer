using UnityEngine;
using System.Collections;

public class WinTrigger : MonoBehaviour {
	
	private GUIScript guiScript;
	
	
	void OnTriggerEnter ()
	{
		guiScript = (GUIScript)GameObject.FindObjectOfType(typeof(GUIScript));
		guiScript.showOptions = true;
	}
}
