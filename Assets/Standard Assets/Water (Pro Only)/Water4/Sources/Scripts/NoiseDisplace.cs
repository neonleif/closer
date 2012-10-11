using UnityEngine;
using System.Collections;
using System;

[ExecuteInEditMode]
[RequireComponent(typeof(WaterBase))]
public class NoiseDisplace : MonoBehaviour 
{		
	private WaterBase waterBase = null;
		
	public bool updateTextures = true;
		
	public String firstDisplacementSampler = "_DisplacementHeightMap";
	public String secondDisplacementSampler = "_SecondDisplacementHeightMap";
	public String thirdDisplacementSampler = "_ThirdDisplacementHeightMap";
	
	// shadow CPU maps for CPU displacement and floating stuff
	public Texture2D displacement2D;
	public Texture2D secondDisplacement2D;
	public Texture2D thirdDisplacement2D;

	// compressed GPU maps for optimized tex lookups
	public Texture2D firstCompressed;
	public Texture2D secondCompressed;
	public Texture2D thirdCompressed;
		
	public void Start() 
	{
		if(!waterBase)
			waterBase = (WaterBase)gameObject.GetComponent(typeof(WaterBase));
	}
	
	public void OnEnable() 
	{
		Shader.EnableKeyword("WATER_VERTEX_DISPLACEMENT_ON");
		Shader.DisableKeyword("WATER_VERTEX_DISPLACEMENT_OFF");		
	}	

	public void OnDisable() 
	{
		Shader.EnableKeyword("WATER_VERTEX_DISPLACEMENT_OFF");
		Shader.DisableKeyword("WATER_VERTEX_DISPLACEMENT_ON");		
	}
	
	public void Update() 
	{
		if(updateTextures)
		{
			if(!waterBase)
				waterBase = (WaterBase)gameObject.GetComponent(typeof(WaterBase));
			if(waterBase && waterBase.sharedMaterial)
			{
				waterBase.sharedMaterial.SetTexture(firstDisplacementSampler,firstCompressed);	
				waterBase.sharedMaterial.SetTexture(secondDisplacementSampler,secondCompressed);	
				waterBase.sharedMaterial.SetTexture(thirdDisplacementSampler,thirdCompressed);	
			}	
		}	
	}
	
	public float GetOffsetAt(Vector3 pos)
	{
		if(null == displacement2D || null == secondDisplacement2D || null == thirdDisplacement2D)
			return 0.0f;
		if(null == waterBase)
			waterBase = (WaterBase)gameObject.GetComponent(typeof(WaterBase));
		if(null == waterBase)
			return 0.0f;
			
		Vector4 tilingVec4 = waterBase.sharedMaterial.GetVector("_AnimationTiling");
		Vector4 speedVec4 = waterBase.sharedMaterial.GetVector("_AnimationDirection");
		float heightDisplacement = waterBase.sharedMaterial.GetFloat("_HeightDisplacement");
				
		float timeAdd = Time.time * 0.05f;
		float heightDisplace = heightDisplacement;
				
		Vector3 lookupA;
		Vector3 lookupB;

		lookupA.x = (pos.x * tilingVec4.x) + timeAdd * speedVec4.x;
		lookupA.y = (pos.z * tilingVec4.y) + timeAdd * speedVec4.y;

		lookupB.x = (pos.x * tilingVec4.z) + timeAdd * speedVec4.z;
		lookupB.y = (pos.z * tilingVec4.w) + timeAdd * speedVec4.w;
		
		Color pxlA = displacement2D.GetPixelBilinear(lookupA.x,lookupA.y);
		Color pxlB = secondDisplacement2D.GetPixelBilinear(lookupB.x,lookupB.y);
		Color pxlC = thirdDisplacement2D.GetPixelBilinear(lookupA.x,lookupB.y);
		
		float offset = ((pxlA.a + pxlB.a + pxlC.a) / 3.0f) - 0.5f;
		return offset * heightDisplace;
	}
	
	public Vector3 GetNormalAt(Vector3 pos, float scale = 1.0F) 
	{
		Vector3 pointA = new Vector3(-scale,0.0f,0.0f) + Vector3.up * (GetOffsetAt(pos+new Vector3(-scale,0.0f,0.0f)));
		Vector3 pointB = new Vector3(-scale,0.0f,scale) + Vector3.up * (GetOffsetAt(pos+new Vector3(-scale,0.0f,scale)));
		Vector3 pointC = new Vector3(0.0f,0.0f,0.0f) + Vector3.up * (GetOffsetAt(pos+new Vector3(0.0f,0.0f,0.0f)));
				
		Vector3 baseX = pointA-pointB;
		Vector3 baseY = pointA-pointC;
				
		Vector3 normal = Vector3.Cross(baseX,baseY);
		normal.Normalize();
		
		return normal;
	}		
}