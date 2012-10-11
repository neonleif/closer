using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(AgentEnergy))]
[RequireComponent(typeof(AutonomousVehicle), typeof(Radar))]
public class AgentControl : MonoBehaviour
{
	
	//private float energyLastTick;
	//public float energyMultiplier = 10f;
	//public float energy = 1f;
	//public float depletionThreshold = .1f;

	//public float tickMyHealth = 0.1f;
	//public float healthHigh = 0.8f;
	//public float healthMedium = 0.5f;
	//public float healthLow = 0.2f;
	
	private GameObject player;
	public float distanceToPlayer;
	public float pingPlayerLocationInterval = .1f;
	public ParticleEmitter fire;
	public Light fireLight;
	public ParticleEmitter smoke;
	public ParticleEmitter sparks;
	public float pursueRange = 3.0f;
	
	
	public AudioClip sfxDrained;
	public AudioClip sfxHitPlayer;
	public AudioClip sfxDepleted;

	private bool isDepleted = false;
	private AgentManager agentManager;

	void Start ()
	{
		player = GameObject.FindGameObjectWithTag ("Player");
		fireLight.enabled = fire.emit = smoke.emit = sparks.emit = false;
		
		if (!audio) {
			gameObject.AddComponent<AudioSource> ();
		}
		
		//energyLastTick = energy;
		//StartCoroutine (TickMyHealthRoutine ());
		
		StartCoroutine (PingPlayerLocationsLoop ());
		StartCoroutine (TickAStarPath ());
		
		agentManager = (AgentManager)GameObject.FindObjectOfType(typeof(AgentManager));
	}

	IEnumerator TickAStarPath ()
	{
		//(GetComponent (typeof(Seeker)) as Seeker).StartPath (startPoint, endPoint);
		yield return new WaitForSeconds (5.0f);
		StartCoroutine (TickAStarPath ());
	}
	
	public void PathComplete (Vector3[] points)
	{
		
		//The points are all the waypoints you need to follow to get to the target
		
	}

	IEnumerator PingPlayerLocationsLoop ()
	{
		distanceToPlayer = Vector3.Distance (transform.position, player.transform.position);
		if (distanceToPlayer < pursueRange)
		{
			SteerForPursuit pursuit = GetComponent<SteerForPursuit> ();
			pursuit.enabled = true;
			pursuit.Quarry = player.GetComponent<Vehicle>();
		}
		yield return new WaitForSeconds (pingPlayerLocationInterval);
		StartCoroutine (PingPlayerLocationsLoop ());
	}


	
/*	IEnumerator TickMyHealthRoutine ()
	{
		if (energy < healthLow)
		{
			fireLight.enabled = true;
			fire.emit = true;
			smoke.emit = true;
			sparks.emit = true;
		}
		else if (energy < healthMedium)
		{
			fireLight.enabled = false;
			fire.emit = false;
			smoke.emit = true;
			sparks.emit = true;
		}
		else if (energy < healthHigh)
		{
			fireLight.enabled = false;
			fire.emit = false;
			smoke.emit = false;
			sparks.emit = true;
		}
		else
		{
			fireLight.enabled = false;
			fire.emit = false;
			smoke.emit = false;
			sparks.emit = false;
		}
		
		if (energy != energyLastTick)
		{
			audio.PlayOneShot (sfxDrained, Mathf.Abs (energy - 1f));
		}
		
		energyLastTick = energy;
		
		yield return new WaitForSeconds (tickMyHealth);
		StartCoroutine (TickMyHealthRoutine ());
	}
	 */

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
			audio.PlayOneShot (sfxHitPlayer, player.rigidbody.velocity.magnitude);
		}
	}


	public void Kill ()
	{
		if (!isDepleted)
		{
			audio.PlayOneShot (sfxDepleted);
			DisableSteering ();
			Score.liveAgentsInLevel--;

			fireLight.enabled = true;
			fire.emit = true;
			smoke.emit = true;
			sparks.emit = true;
			
			agentManager.AnAgentHasDied (gameObject);
		}
	}


	void DisableSteering ()
	{
		isDepleted = true;
		AutonomousVehicle autonomousVehicle = GetComponent<AutonomousVehicle> ();
		autonomousVehicle.enabled = false;
		Radar radar = GetComponent<Radar> ();
		radar.enabled = false;
		SteerForSphericalObstacleAvoidance avoid = GetComponent<SteerForSphericalObstacleAvoidance> ();
		avoid.enabled = false;
		SteerForPursuit pursuit = GetComponent<SteerForPursuit> ();
		pursuit.enabled = false;
	}
}
