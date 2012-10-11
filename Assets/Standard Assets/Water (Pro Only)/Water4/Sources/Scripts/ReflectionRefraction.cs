
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(WaterBase))]
public class ReflectionRefraction : MonoBehaviour 
{	
	// reflection
	public LayerMask reflectionMask;
	public bool reflectSkybox = false;
	public Color clearColor;
	public System.String reflectionSampler = "_ReflectionTex";
	
	// refraction
	public LayerMask refractionMask;
	public bool useGrabPassForRefraction = false;
	public System.String refractionSampler = "_RefractionTex";

	// height
	public Transform waterHeight;
	public float clipPlaneOffset = 0.07F;
		
	// private members
	private Vector3 oldpos = Vector3.zero;
	private Camera reflectionCamera;
	private Camera refractionCamera;
	private Material sharedMaterial = null;
	
	private bool hasRefractionComponent = true;
		
	public void Start () 
	{
		sharedMaterial = ((WaterBase)gameObject.GetComponent(typeof(WaterBase))).sharedMaterial;
		hasRefractionComponent = sharedMaterial.HasProperty(refractionSampler);
	}
	
	private Camera CreateReflectionCameraFor(Camera cam) 
	{		
		System.String reflName = gameObject.name+"Reflection"+cam.name;
		GameObject go = GameObject.Find(reflName);
		
		if(!go)
			go = new GameObject(reflName, typeof(Camera)); 
		if(!go.GetComponent(typeof(Camera)))
			go.AddComponent(typeof(Camera));
		Camera reflectCamera = go.camera;				
		
		reflectCamera.backgroundColor = clearColor;
		reflectCamera.clearFlags = reflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;				
		
		SetStandardCameraParameter(reflectCamera,reflectionMask);		
		
		if(!reflectCamera.targetTexture) 
			reflectCamera.targetTexture = CreateTextureFor(cam);
		
		return reflectCamera;
	}

	private Camera CreateRefractionCameraFor(Camera cam) 
	{
		System.String reflName = gameObject.name+"Refraction"+cam.name;
		GameObject go = GameObject.Find(reflName);
		
		if(!go)		
			go = new GameObject(reflName, typeof(Camera)); 
		if(!go.GetComponent(typeof(Camera)))
			go.AddComponent(typeof(Camera));
		Camera refractCamera = go.camera;				
		
		SetStandardCameraParameter(refractCamera, refractionMask);	
		
		if(!refractCamera.targetTexture)
			refractCamera.targetTexture = CreateTextureFor(cam) ;
		
		return refractCamera;
	}
	
	private void SetStandardCameraParameter(Camera cam, LayerMask mask)
	{
		cam.cullingMask = mask & ~(1<<LayerMask.NameToLayer("Water"));
		cam.backgroundColor = Color.black;
		cam.enabled = false;			
	}
	
	private RenderTexture CreateTextureFor(Camera cam) 
	{
		RenderTexture rt = new RenderTexture(Mathf.FloorToInt(cam.pixelWidth*0.5F), Mathf.FloorToInt(cam.pixelHeight*0.5F), 24);	
		rt.hideFlags = HideFlags.DontSave;
		return rt;
	}	
	
	
	/*public void OnDisable () 
	{
		if(reflectionCamera) {
			if(reflectionCamera.targetTexture)
				DestroyImmediate(reflectionCamera.targetTexture);
			reflectionCamera.targetTexture = null;
			DestroyImmediate(reflectionCamera.gameObject);
			reflectionCamera = null;
		}
		if(refractionCamera) {
			if(refractionCamera.targetTexture)
				DestroyImmediate(refractionCamera.targetTexture);
			refractionCamera.targetTexture = null;
			DestroyImmediate(refractionCamera.gameObject);
			refractionCamera = null;
		}	
		
		if(sharedMaterial) {
			if(sharedMaterial.HasProperty(reflectionSampler)) 
				sharedMaterial.SetTexture(reflectionSampler, null);		
			if(sharedMaterial.HasProperty(refractionSampler)) 
				sharedMaterial.SetTexture(refractionSampler, null);		
		}
	}*/
	
	public void Update()
	{
		useGrabPassForRefraction = (useGrabPassForRefraction || !hasRefractionComponent);
	}
	
	public void LateUpdate () 
	{
		if(!reflectionCamera)			
			reflectionCamera = CreateReflectionCameraFor(Camera.main);
		if(!refractionCamera && !useGrabPassForRefraction)
			refractionCamera = CreateRefractionCameraFor(Camera.main);	
		
		RenderReflectionFor(Camera.main, reflectionCamera);	
		if(!useGrabPassForRefraction)
			RenderRefractionFor(Camera.main, refractionCamera);
		else {
			// @NOTE: this fixes the current problem that oin forward & D3D, _GrabTexture results are upside down
			if (Camera.main && Camera.main.renderingPath == RenderingPath.Forward)
				sharedMaterial.SetVector("_GrabPassFix", new Vector4(1.0f,-1.0f,0.0f,1.0f));
			else
				sharedMaterial.SetVector("_GrabPassFix", new Vector4(1.0f,1.0f,0.0f,0.0f));
		}
	}
	
	public void OnRenderObject () 
	{			
		// OnRenderObject called *after* camera has rendered the object
		// UNCOOL: this will display a wrong reflection / refraction in scene view
		
		if(reflectionCamera && sharedMaterial)
			sharedMaterial.SetTexture(reflectionSampler, reflectionCamera.targetTexture);	
		if(!useGrabPassForRefraction && refractionCamera && sharedMaterial)		
			sharedMaterial.SetTexture(refractionSampler, refractionCamera.targetTexture);					
	}
	
	private void RenderRefractionFor (Camera cam, Camera refractCamera) 
	{
		if(!refractCamera)
			return;
		
		SaneCameraSettings(refractCamera);
			
		refractCamera.cullingMask = refractionMask & ~(1<<LayerMask.NameToLayer("Water"));
		
		refractCamera.transform.position = cam.transform.position;
		refractCamera.transform.rotation = cam.transform.rotation;
		refractCamera.projectionMatrix = cam.projectionMatrix;			
				
		refractCamera.Render();			
	}
	
	private void RenderReflectionFor (Camera cam, Camera reflectCamera) 
	{
		if(!reflectCamera)
			return;
			
		reflectCamera.cullingMask = reflectionMask & ~(1<<LayerMask.NameToLayer("Water"));
		
		SaneCameraSettings(reflectCamera);
		
		reflectCamera.backgroundColor = clearColor;				
		reflectCamera.clearFlags = reflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;				
		if(reflectSkybox) 
		{ 			
			if(!reflectCamera.gameObject.GetComponent(typeof(Skybox)) && cam.gameObject.GetComponent(typeof(Skybox))) 
			{
				Skybox sb = (Skybox)reflectCamera.gameObject.AddComponent(typeof(Skybox));
				sb.material = ((Skybox)cam.GetComponent(typeof(Skybox))).material;
			}	
		}
							
		GL.SetRevertBackfacing(true);		
							
		Transform reflectiveSurface = waterHeight;
			
		Vector3 eulerA = cam.transform.eulerAngles;
					
		reflectCamera.transform.eulerAngles = new Vector3(-eulerA.x, eulerA.y, eulerA.z);
		reflectCamera.transform.position = cam.transform.position;
				
		Vector3 pos = reflectiveSurface.transform.position;
		pos.y = waterHeight.position.y;
		Vector3 normal = reflectiveSurface.transform.up;
		float d = -Vector3.Dot(normal, pos) - clipPlaneOffset;
		Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);
				
		Matrix4x4 reflection = Matrix4x4.zero;
		reflection = CalculateReflectionMatrix(reflection, reflectionPlane);		
		oldpos = cam.transform.position;
		Vector3 newpos = reflection.MultiplyPoint (oldpos);
						
		reflectCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;
				
		Vector4 clipPlane = CameraSpacePlane(reflectCamera, pos, normal, 1.0f);
				
		Matrix4x4 projection =  cam.projectionMatrix;
		projection = CalculateObliqueMatrix(projection, clipPlane);
		reflectCamera.projectionMatrix = projection;
		
		reflectCamera.transform.position = newpos;
		Vector3 euler = cam.transform.eulerAngles;
		reflectCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);	
														
		reflectCamera.Render();	
		
		GL.SetRevertBackfacing(false);					
	}
	
	private void SaneCameraSettings(Camera helperCam) 
	{
		helperCam.depthTextureMode = DepthTextureMode.None;		
		helperCam.backgroundColor = Color.black;				
		helperCam.clearFlags = CameraClearFlags.SolidColor;				
		helperCam.renderingPath = RenderingPath.Forward;	
	}	
		
	static Matrix4x4 CalculateObliqueMatrix (Matrix4x4 projection, Vector4 clipPlane) 
	{
		Vector4 q = projection.inverse * new Vector4(
			sgn(clipPlane.x),
			sgn(clipPlane.y),
			1.0F,
			1.0F
		);
		Vector4 c = clipPlane * (2.0F / (Vector4.Dot (clipPlane, q)));
		// third row = clip plane - fourth row
		projection[2] = c.x - projection[3];
		projection[6] = c.y - projection[7];
		projection[10] = c.z - projection[11];
		projection[14] = c.w - projection[15];
		
		return projection;
	}	
	 
	// Helper function for getting the reflection matrix that will be multiplied with camera matrix
	static Matrix4x4 CalculateReflectionMatrix (Matrix4x4 reflectionMat, Vector4 plane) 
	{
	    reflectionMat.m00 = (1.0F - 2.0F*plane[0]*plane[0]);
	    reflectionMat.m01 = (   - 2.0F*plane[0]*plane[1]);
	    reflectionMat.m02 = (   - 2.0F*plane[0]*plane[2]);
	    reflectionMat.m03 = (   - 2.0F*plane[3]*plane[0]);
	
	    reflectionMat.m10 = (   - 2.0F*plane[1]*plane[0]);
	    reflectionMat.m11 = (1.0F - 2.0F*plane[1]*plane[1]);
	    reflectionMat.m12 = (   - 2.0F*plane[1]*plane[2]);
	    reflectionMat.m13 = (   - 2.0F*plane[3]*plane[1]);
	
	   	reflectionMat.m20 = (   - 2.0F*plane[2]*plane[0]);
	   	reflectionMat.m21 = (   - 2.0F*plane[2]*plane[1]);
	   	reflectionMat.m22 = (1.0F - 2.0F*plane[2]*plane[2]);
	   	reflectionMat.m23 = (   - 2.0F*plane[3]*plane[2]);
	
	   	reflectionMat.m30 = 0.0F;
	   	reflectionMat.m31 = 0.0F;
	   	reflectionMat.m32 = 0.0F;
	   	reflectionMat.m33 = 1.0F;
	   	
	   	return reflectionMat;
	}
	
	// Extended sign: returns -1, 0 or 1 based on sign of a
	static float sgn (float a) {
	       if (a > 0.0F) return 1.0F;
	       if (a < 0.0F) return -1.0F;
	       return 0.0F;
	}	
	
	// Given position/normal of the plane, calculates plane in camera space.
	private Vector4 CameraSpacePlane (Camera cam, Vector3 pos, Vector3 normal, float sideSign) 
	{
		Vector3 offsetPos = pos + normal * clipPlaneOffset;
		Matrix4x4 m = cam.worldToCameraMatrix;
		Vector3 cpos = m.MultiplyPoint (offsetPos);
		Vector3 cnormal = m.MultiplyVector (normal).normalized * sideSign;
		
		return new Vector4 (cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot (cpos,cnormal));
	}
}
