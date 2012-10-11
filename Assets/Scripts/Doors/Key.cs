using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Collider))]
public class Key : MonoBehaviour
{
	private Doors doors;
	
	
	void Start ()
	{
		doors = (Doors)GameObject.FindObjectOfType(typeof(Doors));
		collider.isTrigger = true;
	}
	
    void OnTriggerEnter(Collider other)
	{
        if (other.gameObject.CompareTag("Player")) {
			doors.locked = false;
			Destroy(this.gameObject, 0.5f);
		}
    }
}