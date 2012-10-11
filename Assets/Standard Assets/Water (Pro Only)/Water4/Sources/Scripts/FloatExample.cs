using UnityEngine;

public class FloatExample : MonoBehaviour 
{
	public float stability = 0.1f;
	public float smoothness = 0.25f;
	public float rotationSmoothness = 0.1f;
	public NoiseDisplace displace;
	
	private float baseY;
	private Quaternion baseRotation = Quaternion.identity;
	private Quaternion internalRotation = Quaternion.identity;
		
	void Start () 
	{
		baseRotation = transform.rotation;
		internalRotation = baseRotation;		
		baseY = transform.position.y;
	}
	
	void Update () 
	{
		if (displace) 
		{
			// displace in Y
			Vector3 pos = Vector3.up * (baseY + displace.GetOffsetAt(transform.position));
			transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime *  1.0F/smoothness);
			
			// smooth rotate
			Vector3 norm = displace.GetNormalAt(transform.position, rotationSmoothness);
			Quaternion normalRotated = Quaternion.identity; 
			normalRotated.SetFromToRotation(internalRotation * Vector3.up, Vector3.Lerp(norm, baseRotation * Vector3.up,  Mathf.Clamp01(stability)));
			internalRotation = Quaternion.Lerp(internalRotation, normalRotated,  Time.deltaTime *  1.0F/smoothness);
			transform.rotation = internalRotation; 			
		}
	}
}
