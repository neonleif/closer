using UnityEngine;

public enum WaterQuality {
		High = 2,
		Medium = 1,
		Low = 0,
}

[ExecuteInEditMode]
[RequireComponent(typeof(WaterBase))]
public class PerformanceSettings : MonoBehaviour 
{
	public WaterQuality waterQuality = WaterQuality.High;
	public bool edgeBlend = true;
	
	private WaterBase waterBase = null;
	
	public void Start() {
		waterBase = (WaterBase)gameObject.GetComponent(typeof(WaterBase));				
	}
	
	public void UpdateShader() 
	{		
		if(waterQuality > WaterQuality.Medium)
			waterBase.sharedMaterial.shader.maximumLOD = 501;
		else if(waterQuality> WaterQuality.Low)
			waterBase.sharedMaterial.shader.maximumLOD = 301;
		else 
			waterBase.sharedMaterial.shader.maximumLOD = 201;	
		
		if(edgeBlend) {
			Shader.EnableKeyword("WATER_EDGEBLEND_ON");
			Shader.DisableKeyword("WATER_EDGEBLEND_OFF");		
			Camera.main.depthTextureMode |= DepthTextureMode.Depth;			
		} 
		else {
			Shader.EnableKeyword("WATER_EDGEBLEND_OFF");
			Shader.DisableKeyword("WATER_EDGEBLEND_ON");			
		}		
	}
		
	public void Update () 
	{	
		if(!waterBase)	
			waterBase = (WaterBase)gameObject.GetComponent(typeof(WaterBase));				
		
		if(waterBase.sharedMaterial)		
			UpdateShader();
	}
	
	public void OnRenderObject () 
	{
		if(edgeBlend) {
			Camera.current.depthTextureMode |= DepthTextureMode.Depth;
		}
	}
}
