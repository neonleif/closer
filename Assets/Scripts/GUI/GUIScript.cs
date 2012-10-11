using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
[RequireComponent(typeof(Score), typeof(MusicPlayer))]
public class GUIScript : MonoBehaviour
{

	public float columnLayoutWidth = 200.0f;
	public bool gamePaused = true;
	public float sensitivity { get; set;}
	public bool useAnalogStick = false;	//Game Pad Analogue Stick or Keyboard Arrow Keys
	public bool useTorque = true;	//The ball moves by torque instead of force
	private MusicPlayer musicPlayer;
	private Score score;
	[HideInInspector]
	public bool inWorkshop = false;
	public bool showOptions = true;
	public Rect windowRect;
	private int toolbarSelectedItem;
	
	
	void Start ()
	{
		// Fetch player preferences
		audio.volume = PlayerPrefs.GetFloat ("musicVolume", audio.volume);
		if (PlayerPrefs.GetInt ("musicMute", 0) == 0)
		{
			audio.mute = false;
		}
		else
		{
			audio.mute = true;
		}
		
		sensitivity = PlayerPrefs.GetFloat ("sensitivity");
		if(PlayerPrefs.GetInt("useAnalogueStick", 0) == 0) {
			useAnalogStick = false;
		} else {
			useAnalogStick = true;
		}
		if (PlayerPrefs.GetInt ("useTorque", 0) == 0) {
			useTorque = false;
		} else {
			useTorque = true;
		}
		
		/// this is the short way to get another component on the same GameObject
		musicPlayer = GetComponent<MusicPlayer> ();
		if (musicPlayer == null)
			Debug.LogError(this + " needs a MusicPlayer component... ");
		
		score = GetComponent<Score> ();
		if (score == null)
			Debug.LogError (this + " needs a Score component... ");
		
		
		//DontDestroyOnLoad (transform.gameObject);
		
		// Construct the window rectangle if not set from inspector
		windowRect = new Rect (
			Screen.width - Screen.width * 0.75f, 
			Screen.height - Screen.height * 0.75f, 
			Screen.width * 0.5f, 
			Screen.height * 0.5f);
	}

	void Update ()
	{
		/// pauses the game when in the menu
		if (gamePaused)
		{
			Time.timeScale = 0.0f;
			Screen.lockCursor = false;
			Screen.showCursor = true;
		}
		else if (!gamePaused)
		{
			Time.timeScale = 1.0f;
			Screen.lockCursor = true;
			Screen.showCursor = false;
		}
		
		///Makes sure that menu can be brought up with esc, it is closed again with esc.
		if (Input.GetKeyUp ("escape")) {
			if(!showOptions) {
				showOptions = true;
			}
			else {
				showOptions = false;
			}
		}
		
		/// Pauses the game if NOT already paused, inWorkshop, OR showing option menu
		if (inWorkshop || showOptions) {
			gamePaused = true;
		}
		else {
			gamePaused = false;
		}
	}

	void OnGUI ()
	{
		if (showOptions) 
		{
			GUILayout.BeginVertical ("box", GUILayout.Width (Screen.width), GUILayout.Height (Screen.height));
			{
				RenderTopMenu ();
			}
			GUILayout.EndVertical ();
		}
		else if (inWorkshop)
		{
			ShowWorkshopMenu();
		}
	}
	
	void ShowWorkshopMenu ()
	{
		// TODO: Layout workshop menu the same way as options menu, using GUILayout.Window
		if (GUILayout.Button ("Exit the Workshop", GUILayout.Width (Screen.width), GUILayout.Height (Screen.height * 0.3f)))
		{
			inWorkshop = false;
		}
		if (GUILayout.Button ("Save current score: " + score.score, GUILayout.Width (Screen.width), GUILayout.Height (Screen.height * 0.3f)))
		{
			PlayerPrefs.SetInt ("score", score.score);
		}
	}
	
	
#region Option menus
	private string[] menuToolbarItems = new string[] { "Instructions", "Levels", "Options", "Scores", "Sound", "Quit" };
	/// Rendering the top toolbar
	void RenderTopMenu ()
	{
		toolbarSelectedItem = GUILayout.Toolbar (toolbarSelectedItem, menuToolbarItems);
		if (toolbarSelectedItem == 0)
			windowRect = GUILayout.Window (0, windowRect, InstructionsMenu, menuToolbarItems[0]);
		if (toolbarSelectedItem == 1)
			windowRect = GUILayout.Window (0, windowRect, LevelMenu, menuToolbarItems[1]);
		if (toolbarSelectedItem == 2)
			windowRect = GUILayout.Window (0, windowRect, OptionsMenu, menuToolbarItems[2]);
		if (toolbarSelectedItem == 3)
			windowRect = GUILayout.Window (0, windowRect, ScoreMenu, menuToolbarItems[3]);
		if (toolbarSelectedItem == 4)
			windowRect = GUILayout.Window (0, windowRect, SoundMenu, menuToolbarItems[4]);
		if (toolbarSelectedItem == 5)
			windowRect = GUILayout.Window (0, windowRect, QuitMenu, menuToolbarItems[5]);
	}
	
	/// The score
	void ScoreMenu (int winId)
	{
		if (GUILayout.Button ("Save the current Score: " + score))
		{
			PlayerPrefs.SetInt ("score", score.score);
		}
		GUILayout.Label ("Your saved score is " + PlayerPrefs.GetInt ("score", 0));
		GUI.DragWindow ();
	}

	/// The Instructions on how to play
	void InstructionsMenu (int winId)
	{
		GUILayout.Label ("Roll the ball to safety behind the doors.");
		GUILayout.Label ("To get the key from the robots, lure them into a trap, but be careful not to fall into the trap yourself...");
		GUILayout.Label ("");
		GUILayout.Label ("Swipe trackpad or move your Mouse to control the game.\nChange to Arrow Keys in Options.");
		GUILayout.Label ("Press ESC to enter this menu.");
		GUILayout.Label ("Change sensitivity in Options.");
		GUI.DragWindow ();
	}
	
	/// Only has sensitivity, this should be saved throughout
	void OptionsMenu (int winId)
	{
		GUILayout.Label ("Controls");
				
		GUILayout.BeginHorizontal();
		sensitivity = PlayerPrefs.GetFloat ("sensitivity", sensitivity);
		GUILayout.Label ("sensitivity = " + Mathf.CeilToInt ((sensitivity / 50) * 100) + "%", GUILayout.Width (columnLayoutWidth));
		sensitivity = GUILayout.HorizontalSlider (sensitivity, 0.0f, 50.0f);
		PlayerPrefs.SetFloat ("sensitivity", sensitivity);
		GUILayout.EndHorizontal();
		
		
		if (PlayerPrefs.GetInt ("useAnalogueStick", 0) == 0) {
			useAnalogStick = false;
		} else {
			useAnalogStick = true;
		}
		useAnalogStick = GUILayout.Toggle (useAnalogStick, "Use Arrow-Keys or Game-Pad analogue sticks.");
		if (useAnalogStick) {
			PlayerPrefs.SetInt ("useAnalogueStick", 1);
		} else {
			PlayerPrefs.SetInt ("useAnalogueStick", 0);
		}

		
		if (PlayerPrefs.GetInt ("useTorque", 0) == 0) {
			useTorque = false;
		} else {
			useTorque = true;
		}
		useTorque = GUILayout.Toggle (useTorque, "The ball uses Torque instead of Force to move forward.");
		if (useTorque) {
			PlayerPrefs.SetInt ("useTorque", 1);
		} else {
			PlayerPrefs.SetInt ("useTorque", 0);
		}

		
		GUI.DragWindow ();
	}

	void SoundMenu (int winId)
	{
		///Music
		GUILayout.Label ("Music");
		GUILayout.BeginHorizontal ();
		
		audio.volume = PlayerPrefs.GetFloat ("musicVolume", audio.volume);
		GUILayout.Label ("volume = " + Mathf.CeilToInt ((audio.volume / 1) * 100) + "%", GUILayout.Width (columnLayoutWidth));
		audio.volume = GUILayout.HorizontalSlider (audio.volume, 0f, 1f);
		PlayerPrefs.SetFloat("musicVolume", audio.volume);
		GUILayout.EndHorizontal ();
		
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Now playing " + audio.clip.name, GUILayout.Width (columnLayoutWidth));
		if (GUILayout.Button ("Pick new random track")) {
			audio.clip = musicPlayer.tracks[Random.Range (0, musicPlayer.tracks.Length)];
			audio.Play ();
		}
		GUILayout.EndHorizontal ();
		
		
		if (PlayerPrefs.GetInt ("musicMute", 0) == 0) {
			audio.mute = false;
		} else {
			audio.mute = true;
		}
		audio.mute = GUILayout.Toggle (audio.mute, "Mute Music");
		if (audio.mute) {
			PlayerPrefs.SetInt ("musicMute", 1);
		} else {
			PlayerPrefs.SetInt ("musicMute", 0);
		}
		GUI.DragWindow ();
	}
	
	/// TODO: This should be made so that only unlocked levels is clickable. 
	void LevelMenu (int winId)
	{
		if (Application.loadedLevelName != "IntroScene")
		{
			if (GUILayout.Button ("Retry"))
			{
				Application.LoadLevel (Application.loadedLevel);
			}
		}
		
		for (int i = 0; i < Application.levelCount; i++)
		{
			if (GUILayout.Button ("Load Level " + i))
			{
				Application.LoadLevel (i);
			}
		}
		GUI.DragWindow ();
	}
	
	/// Asks player about quiting. 
	void QuitMenu (int winId)
	{
		GUI.DragWindow ();
		GUILayout.FlexibleSpace ();
#if UNITY_STANDALONE_OSX || UNITY_DASHBOARD_WIDGET || UNITY_STANDALONE_WIN || UNITY_ANDROID
		if (GUILayout.Button ("Quit!\n\n(Too bad, we started to like your playing style...)"))
			Application.Quit ();
#elif UNITY_WEBPLAYER
		GUILayout.Label ("Thank you for playing, please leave some feedback below.");
#elif UNITY_EDITOR
		GUILayout.Label ("Can't Quit the editor...");
#endif
	}
#endregion
	
	public float GetSensitivity()
	{
		return sensitivity;
	}
	
}
