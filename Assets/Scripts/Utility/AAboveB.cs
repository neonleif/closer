using UnityEngine;
using System.Collections;

public class AAboveB : MonoBehaviour
{
	
	public bool aShouldBeInstantiated = true;
	public Transform a;
	public Transform b;
	public Vector3 offset = Vector3.zero;


	void Start ()
	{
		if (!a || !b) {
			Debug.LogError ("Assign Transforms to " + this + " in " + gameObject.name);
		}
		
		if (aShouldBeInstantiated) {
			a = (Transform)Instantiate(a, b.position, Quaternion.AngleAxis(90f, Vector3.right));
		}
	}


	void Update ()
	{
		a.position = b.position + offset;
	}
}
