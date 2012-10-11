using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Rigidbody), typeof(SphericalObstacleData))]
public class RandomObstacle : MonoBehaviour {

	void Start () {
		transform.localScale = new Vector3 (Random.value + 0.25f, Random.value + 0.25f, Random.value + 0.25f);
		renderer.material.shader = Shader.Find("Transparent/Diffuse");
		renderer.material.color = new Color(Random.value + 0.25f, Random.value + 0.25f, Random.value + 0.25f, 0.75f);
	}
}
