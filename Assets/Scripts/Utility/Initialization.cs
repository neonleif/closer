using UnityEngine;
using System.Collections;

public class Initialization : MonoBehaviour
{
	public bool lockCursorInPlaymode = true;

	void Start ()
	{
		if (lockCursorInPlaymode)
			Screen.lockCursor = true;
		else
			Screen.lockCursor = false;
	}
	
	
}
