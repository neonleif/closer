using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriggerTests : MonoBehaviour
{
	
	
	private List<GameObject> gameObjectList = new List<GameObject>();
	
	void OnTriggerEnter (Collider other)
	{
		Debug.Log(other.name + " is has Entered the trigger...");
		gameObjectList.Add(other.gameObject);
	}
	
	void OnTriggerExit (Collider other)
	{
		Debug.Log(other.name + " is has Exited the trigger...");
		gameObjectList.Remove(other.gameObject);
	}
}
