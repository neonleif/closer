using UnityEngine;
using System.Collections;


public class Score : MonoBehaviour {

	private int bonusMultiplier = 100;
	private float playerEnergy;
	[HideInInspector]
	public static int liveAgentsInLevel;
	public float tickBonusDepletion = 1.0f;
	public float tickScoreUpdate = 0.5f;
	public int score { get; set; }
	public Rect scoreGUIArea;

	
	void Start ()
	{
		// disabled in Intro
		if(Application.loadedLevelName != "IntroScene") {
			liveAgentsInLevel = Mathf.RoundToInt (GameObject.FindGameObjectsWithTag("Agent").Length);
			StartCoroutine (UpdateScore ());
			StartCoroutine (BonusDiminisher ());
		}
	}
	
	/// Timed bonus is subtracted every tickBonusDepletion
	IEnumerator BonusDiminisher ()
	{
		if (bonusMultiplier > 1)
			bonusMultiplier--;
		else
			bonusMultiplier = 1;
			
		yield return new WaitForSeconds (tickBonusDepletion);
		StartCoroutine (BonusDiminisher ());
	}
	
	/// Updates the score every tickScoreUpdate
	IEnumerator UpdateScore ()
	{
		score = CalculateScore ();
		yield return new WaitForSeconds (tickScoreUpdate);
		StartCoroutine (UpdateScore ());
	}

	/// Returns the score
	public int CalculateScore ()
	{
		playerEnergy = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerEnergy> ().energy;
		return Mathf.CeilToInt(playerEnergy * bonusMultiplier + liveAgentsInLevel);
	}
	
	// Placeholder for GUI. We will create a HUD class that calls values in this script when needed
	void OnGUI ()
	{
		// only force w/h if too small
		if (scoreGUIArea.width < 150.0f)
			scoreGUIArea.width = 150.0f;
		if (scoreGUIArea.height < 100.0f)
			scoreGUIArea.height = 100.0f;
			
		scoreGUIArea.x = Screen.width - scoreGUIArea.width;
		scoreGUIArea.y = Screen.height - scoreGUIArea.height;
		
		GUILayout.BeginArea(scoreGUIArea);
		GUI.color = Color.green;
		GUILayout.BeginVertical("box");
		GUILayout.Label ("Bonus: " + bonusMultiplier);
		GUILayout.Label ("Score: " + score);
		GUILayout.Label ("Energy: " + playerEnergy);
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
}
