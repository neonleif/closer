using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Animation))]
public class TrapFire : MonoBehaviour
{
	
	public PlayerEnergy playerEnergy;
	public List<GameObject> victims = new List<GameObject>();
	public ParticleEmitter[] particles;
	public Light[] lights;

	
	void Start ()
	{
		playerEnergy = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerEnergy>();
		particles = GetComponentsInChildren<ParticleEmitter>();
		lights = GetComponentsInChildren<Light>();
	}
	
	void KillOthers ()
	{
		foreach (GameObject victim in victims)
		{
			if (victim.CompareTag("Player"))
			{
				//playerEnergy.lives--; //maybe insta-kill is too hard?
				StartCoroutine(victim.GetComponentInChildren<PlayerEnergy>().Kill());
			}
			if (victim.CompareTag("Agent"))
			{
				victim.GetComponentInChildren<AgentControl>().Kill();
			}
		}
	}
	
	
#region Victim Bookkeeping

	void OnTriggerEnter (Collider other)
	{
		//Debug.Log(other.name + " is has Entered the trigger...");
		if (other.gameObject.CompareTag("Agent") || other.gameObject.CompareTag("Player"))
		{
			victims.Add(other.gameObject);
		}
	}
	
	void OnTriggerExit (Collider other)
	{
		//Debug.Log(other.name + " is has Exited the trigger...");
		if (other.gameObject.CompareTag("Agent") || other.gameObject.CompareTag("Player"))
		{
			victims.Remove(other.gameObject);
		}
	}
	
#endregion
	
}
