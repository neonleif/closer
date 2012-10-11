using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Collider))]
public class DoorSwitch : MonoBehaviour
{
	
	public Animation doorsAnimation;
	public bool open;
	
	
	void Start ()
	{
		open = false;
		collider.isTrigger = true;
		doorsAnimation.playAutomatically = false;
	}
	
	void OnTriggerEnter(Collider other)
	{
		if (!open)
		{
			doorsAnimation.PlayQueued("DoorsOpen");
			open = true;
		}
	}
	
	void OnTriggerExit(Collider other)
	{
		if (open)
		{
			doorsAnimation.PlayQueued("DoorsClose");
			open = false;
		}
	}
}