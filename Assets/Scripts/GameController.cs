using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	void Awake ()
	{
	
		GameObject gameController = GameObject.FindWithTag("GameController");
		if (gameController != null)
		{
			Destroy(gameController.gameObject);
		}
	}
}
