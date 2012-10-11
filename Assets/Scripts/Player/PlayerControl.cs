using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class PlayerControl : MonoBehaviour
{

	public float forceMultiplier = 25f;
	private Vector3 dir;

	// Gui object and its script, is found in Start()
	private GameObject guiObject;
	private GUIScript options;

	public bool m_useAnalogStick = false;
	public bool m_useTorque = false;


	void Start ()
	{
		//TODO: find it another way
		//Find the Gui Object and gets the script so we can use the options on the Player character.
		if (!guiObject) {
			guiObject = GameObject.Find ("GameManager");
			try {
				options = guiObject.GetComponent<GUIScript> ();
			} catch (System.Exception ex) {
				Debug.Log (ex.ToString());
			}
		}
	}

	void FixedUpdate ()
	{
		try {
			m_useAnalogStick = options.useAnalogStick;
			m_useTorque = options.useTorque;
			
			if (options) {
				forceMultiplier = options.GetSensitivity ();
			} else {
				forceMultiplier = 25f;
			}
		} 
		catch (System.Exception ex) 
		{
		}
		
		if (m_useAnalogStick) {
			dir = new Vector3 (Input.GetAxis ("Horizontal"), 0f, Input.GetAxis ("Vertical"));
		} else {
			dir = new Vector3 (Input.GetAxis ("Mouse X"), 0f, Input.GetAxis ("Mouse Y"));
		}
		
		
		if (m_useTorque) {
			rigidbody.AddTorque (forceMultiplier * Vector3.Cross (dir, Vector3.down));
		} else {
			rigidbody.AddForce (forceMultiplier * dir);
			
		}
		
	}
}
