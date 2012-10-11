using UnityEngine;
using System.Collections;

public class PlayerEnergy : MonoBehaviour
{
	[System.Serializable]
	public class DistanceIntervals
	{
		public string name;
		public float distance;
		public Color color;
	}

	public int lives = 3;
	public float energyMultiplier = 100f;
	
	public DistanceIntervals[] distanceIntervals;
	public Color colorOff = Color.cyan;
	public Color colorFar = Color.red;
	public Color colorMedium = Color.yellow;
	public Color colorClose = Color.green;
	public Material illuminationMaterial;
	
	[HideInInspector]
	public float energy;
	
	private GameObject[] agents;
	private AgentControl agentControl;
	private float agentDistToPlayer;
	
	void Start ()
	{
		energy = energyMultiplier;
		agents = GameObject.FindGameObjectsWithTag ("Agent");
		if (!illuminationMaterial)
		{
			Debug.LogError ("No Illumination Material has been set on " + this);
		}
	}

	void Update ()
	{
		if (agents.Length > 0)
		{
			foreach (GameObject agent in agents)
			{
				DrainAgents (agent);
			}
		}
		
		if (lives <= 0)
		{
			Kill ();
		}
	}
	
	void FadeColorAccordingToDistanceIntervals (Color a, Color b)
	{
		illuminationMaterial.color = Color.Lerp (a, b, agentDistToPlayer);
	}

	
	void OnCollisionEnter (Collision collision)
	{
		if (collision.gameObject.tag == "Agent")
		{
			if (lives > 0)
			{
				lives--;		
			}
		}
	}
	
	public AudioClip playerDeath;
	public IEnumerator Kill ()
	{
		audio.PlayOneShot(playerDeath);
		yield return new WaitForSeconds(playerDeath.length);
		RestartLevel ();
	}
	
	void RestartLevel ()
	{
		Application.LoadLevel(Application.loadedLevel);
	}

	
	void DrainAgents (GameObject agent)
	{
		//TODO: consider only the closest agent if any agents at all
		agentControl = agent.gameObject.GetComponent<AgentControl> ();
		agentDistToPlayer = agentControl.distanceToPlayer;
		//AutonomousVehicle agentVehicle = agent.GetComponent<AutonomousVehicle> ();
		
		if (Input.GetButtonDown ("Jump")) {
			agentControl.Kill ();
		}
		
		// TODO: (consider code design) refactor to add or subtract a value from a base value of the player's and agent's energy
		if (agentDistToPlayer > distanceIntervals[0].distance) {
			//TODO: (consider game design) do we want to punish the player for trying to discharge agents out of range?
			//energy *= .9f;
			if (Input.GetButtonDown ("Jump")) {
				//Debug.Log ("Too far away from any target...");
			}
			//FadeColorAccordingToDistanceIntervals (illuminationMaterial.color, colorOff);
		}
		else if (agentDistToPlayer > distanceIntervals[1].distance) {
			if (Input.GetButtonDown ("Jump")) {
				//agentControl.energy *= .9f;
				//agentVehicle.MaxSpeed *= agentControl.energy;
				//energy *= 1.1f;
			}
			//FadeColorAccordingToDistanceIntervals (colorOff, colorFar);
		} else if (agentDistToPlayer > distanceIntervals[2].distance) {
			if (Input.GetButtonDown ("Jump")) {
				//agentControl.energy *= .6f;
				//agentVehicle.MaxSpeed *= agentControl.energy;
				//energy *= 1.4f;
			}
			//FadeColorAccordingToDistanceIntervals (colorFar, colorMedium);
		} else {
			if (Input.GetButtonDown ("Jump")) {
				//agentControl.energy *= .3f;
				//agentVehicle.MaxSpeed *= agentControl.energy;
				//energy *= 1.7f;
			}
			//FadeColorAccordingToDistanceIntervals (colorMedium, colorClose);
		}		
	}
}
