using UnityEngine;
using System.Collections;

public class Workshop : MonoBehaviour {
	
	private GUIScript guiScript;
	
	void Awake ()
	{
		guiScript = (GUIScript)GameObject.FindObjectOfType(typeof(GUIScript));
	}
	
	void OnTriggerEnter (Collider other)
	{
		if (other.tag == "Player") {
			guiScript.inWorkshop = true;
		}
	}

	
	void OnTriggerExit (Collider other)
	{
		if (other.tag == "Player") {
			guiScript.inWorkshop = false;
		}
	}
}
