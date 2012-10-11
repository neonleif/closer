using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Animation), typeof(Collider))]
public class Doors : MonoBehaviour
{
	
	public bool locked { get; set; }
	public bool open;
	
	
	void Start ()
	{
		locked = true;
		open = false;
		collider.isTrigger = true;
		animation.playAutomatically = false;
	}
	
	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player") && !open)
		{
			Open();
		}
	}
	
	void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("Player") && open)
		{
			Close();
		}
	}
	
	public void ToggleLock ()
	{
		if (locked)
		{
			locked = false;
		}
		else
		{
			locked = true;
		}
	}
	
	public void Open ()
	{
		if (locked)
		{
			return;
		}
		animation.PlayQueued("DoorsOpen");
		open = true;
	}

	public void Close ()
	{
		animation.PlayQueued("DoorsClose");
		open = false;
	}
}