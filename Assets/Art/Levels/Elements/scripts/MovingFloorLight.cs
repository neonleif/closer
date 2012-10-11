using UnityEngine;
using System.Collections;

public class MovingFloorLight : MonoBehaviour {
	
	public float scrollSpeed = 550.5F;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		float offset = Time.time * scrollSpeed;
		renderer.material.SetTextureOffset("_MainTex", new Vector2(0, -offset));
		renderer.material.SetTextureOffset("_Illumin", new Vector2(0, -offset));
	}
}
