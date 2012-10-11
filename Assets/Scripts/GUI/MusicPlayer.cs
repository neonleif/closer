using UnityEngine;
//using System.Collections;


[RequireComponent(typeof (AudioSource))]
public class MusicPlayer : MonoBehaviour {
	
	public AudioClip[] tracks;
	
	
	void Awake ()
	{
		//FIXME: cast object to AudioClip properly before this works...
		if (tracks.Length <= 0 || !audio) {
			Debug.LogError ("The must be an AudioSource component on " + this + ", and you must assign music tracks to the music player on " + this);
			// why do I have to specify "UnityEngine" here? I don't normally have to...
			//tracks = (AudioClip[])UnityEngine.Resources.LoadAll("Music", typeof(AudioClip));
		}
		audio.loop = true;
		audio.volume = .5f;
		audio.clip = tracks[Random.Range (0, tracks.Length)];
		audio.Play();
	}
}
