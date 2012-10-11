using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AgentManager : MonoBehaviour {
	
	public List<GameObject> agentList = new List<GameObject>();
	public Transform theKey;
	private GameObject lastAgent;
	
	
	public void Start ()
	{
		GameObject[] agents = GameObject.FindGameObjectsWithTag("Agent");
		foreach (GameObject agent in agents) {
			agentList.Add(agent);
		}
	}
	
	public void AnAgentHasDied (GameObject agent)
	{
		lastAgent = agent;
		agentList.Remove (agent);
		
		if (agentList.Count <= 0) {
			Instantiate(theKey, lastAgent.transform.position, Quaternion.identity);
		}
	}
}
