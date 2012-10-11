using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AgentEnergy : MonoBehaviour
{

	public float energyMultiplier = 10f;
	public float energy = 1f;
	private GameObject player;
	[HideInInspector]
	public float distanceToPlayer;
	public float pingPlayerLocationInterval = .1f;
	public float depletionThreshold = .1f;

	public float tickMyHealth = 0.1f;
	public float healthHigh = 0.8f;
	public float healthMedium = 0.5f;
	public float healthLow = 0.2f;
	public ParticleEmitter fire;
	public Light fireLight;
	public ParticleEmitter smoke;
	public ParticleEmitter sparks;
	
	public AudioClip sfxDrained;
	public AudioClip sfxHitPlayer;
	public AudioClip sfxDepleted;

	private float energyLastTick;
	private bool isDepleted = false;

	void Awake ()
	{
		if (!player) {
			player = GameObject.FindGameObjectWithTag ("Player");
		}
		
		if (!fire || !smoke || !sparks) {
			Debug.LogError ("Fire, Smoke, and Sparks must be set on the Agent prefab... ");
		} else {
			fireLight.enabled = fire.emit = smoke.emit = sparks.emit = false;
		}
		
		if (!audio) {
			gameObject.AddComponent<AudioSource> ();
		}
		
		if (!sfxDrained || !sfxHitPlayer || !sfxDepleted) {
			Debug.LogError ("AudioClips must be assigned on " + this);
		}
		
		energyLastTick = energy;
		
		StartCoroutine (PingPlayerLocationsLoop ());
		StartCoroutine (TickMyHealthRoutine ());
	}



	IEnumerator PingPlayerLocationsLoop ()
	{
		distanceToPlayer = Vector3.Distance (transform.position, player.transform.position);
		
		yield return new WaitForSeconds (pingPlayerLocationInterval);
		
		StartCoroutine (PingPlayerLocationsLoop ());
	}



	IEnumerator TickMyHealthRoutine ()
	{
		if (energy < healthLow) {
			fireLight.enabled = true;
			fire.emit = true;
			smoke.emit = true;
			sparks.emit = true;
		} else if (energy < healthMedium) {
			fireLight.enabled = false;
			fire.emit = false;
			smoke.emit = true;
			sparks.emit = true;
		} else if (energy < healthHigh) {
			fireLight.enabled = false;
			fire.emit = false;
			smoke.emit = false;
			sparks.emit = true;
		} else {
			fireLight.enabled = false;
			fire.emit = false;
			smoke.emit = false;
			sparks.emit = false;
		}
		
		if (energy != energyLastTick) {
			audio.PlayOneShot(sfxDrained, Mathf.Abs(energy - 1f));
		}
		
		energyLastTick = energy;
		
		yield return new WaitForSeconds (tickMyHealth);
		StartCoroutine (TickMyHealthRoutine ());
	}

	
	void OnCollisionEnter (Collision collisionInfo)
	{
		if (collisionInfo.gameObject.tag == "Player") 
		{
			foreach (ContactPoint contact in collisionInfo.contacts) 
			{
				//TODO: Make sparks emit from collision tangents
				//save sparks local rotation
				//rotate sparks particle -normal
				//sparks.Emit ();
				//sparks.Emit (contact.point, contact.normal, sparks.maxSize, sparks.maxEnergy, Color.white);
				sparks.Emit (Mathf.RoundToInt (10 * player.rigidbody.velocity.magnitude));
				//reset sparks local rotation
				
				Debug.DrawRay (contact.point, contact.normal * 10, Color.white);
			}
			audio.PlayOneShot(sfxHitPlayer, player.rigidbody.velocity.magnitude);
		}
	}
	
	
	public bool IsEnergyDepleted ()
	{
		if (energy < depletionThreshold && !isDepleted) {
			audio.PlayOneShot (sfxDepleted);
			DisableSteering ();
			Score.liveAgentsInLevel--;
			return true;
		} else {
			return false;
		}
	}


	void DisableSteering ()
	{
		isDepleted = true;
		AutonomousVehicle autonomousVehicle = GetComponent<AutonomousVehicle> ();
		autonomousVehicle.enabled = false;
		Radar radar = GetComponent<Radar> ();
		radar.enabled = false;
		SteerForWander wander = GetComponent<SteerForWander> ();
		wander.enabled = false;
		SteerForSphericalObstacleAvoidance avoid = GetComponent<SteerForSphericalObstacleAvoidance> ();
		avoid.enabled = false;
		SteerForPursuit pursuit = GetComponent<SteerForPursuit> ();
		pursuit.enabled = false;
	}
	
	
}
