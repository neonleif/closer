using UnityEngine;
using System;
using UnityEditor;

[CustomEditor(typeof(NoiseDisplace))]
public class NoiseDisplaceEditor : Editor 
{    
    private SerializedObject serObj;

    private SerializedProperty firstMap;
    private SerializedProperty secondMap;
    private SerializedProperty thirdMap;
    
    private SerializedProperty firstMapCompressed;
    private SerializedProperty secondMapCompressed;
    private SerializedProperty thirdMapCompressed;
        
    private int textureCreatSize = 32;
	private float wavesFrequency = 4.0F;
    
	public void OnEnable () {
		serObj = new SerializedObject (target); 
    	
		firstMap = serObj.FindProperty("displacement2D");   		
		secondMap = serObj.FindProperty("secondDisplacement2D");   		
		thirdMap = serObj.FindProperty("thirdDisplacement2D");   		

		firstMapCompressed = serObj.FindProperty("firstCompressed");   		
		secondMapCompressed = serObj.FindProperty("secondCompressed");   		
		thirdMapCompressed = serObj.FindProperty("thirdCompressed");  
				
		if (null != firstMap.objectReferenceValue) {
			textureCreatSize = ((Texture2D)firstMap.objectReferenceValue).width;
		}
	}
	
    public override void OnInspectorGUI () 
    {
    	serObj.Update();
    	
    	GameObject go = ((NoiseDisplace)serObj.targetObject).gameObject;
    	NoiseDisplace displace = (NoiseDisplace)serObj.targetObject;
    	WaterBase wb = (WaterBase)go.GetComponent(typeof(WaterBase));    	
    	Material sharedWaterMaterial = wb.sharedMaterial;
    	
        GUILayout.Label ("Animating vertices via layering up to 3 noise textures", EditorStyles.miniBoldLabel);    	
        
		if(sharedWaterMaterial) {	
			GUILayout.Label ("Tiling & Direction", EditorStyles.boldLabel);
			 
			Vector4 animationTiling = WaterEditorUtility.GetMaterialVector("_AnimationTiling", sharedWaterMaterial);
			Vector4 animationDirection = WaterEditorUtility.GetMaterialVector("_AnimationDirection", sharedWaterMaterial);
			
			Vector2 firstTiling = new Vector2(animationTiling.x*100.0F,animationTiling.y*100.0F);
			Vector2 secondTiling = new Vector2(animationTiling.z*100.0F,animationTiling.w*100.0F);
	
			Vector2 firstDirection = new Vector2(animationDirection.x,animationDirection.y);
			Vector2 secondDirection = new Vector2(animationDirection.z,animationDirection.w);
	
			firstTiling = EditorGUILayout.Vector2Field("First displacement tiling", firstTiling);
			secondTiling = EditorGUILayout.Vector2Field("Second displacement tiling", secondTiling);
			animationTiling = new Vector4(firstTiling.x/100.0F,firstTiling.y/100.0F, secondTiling.x/100.0F,secondTiling.y/100.0F);
	
			firstDirection = EditorGUILayout.Vector2Field("First displacement direction", firstDirection);
			secondDirection = EditorGUILayout.Vector2Field("Second displacement direction", secondDirection);
			animationDirection = new Vector4(firstDirection.x,firstDirection.y, secondDirection.x,secondDirection.y);
			
			WaterEditorUtility.SetMaterialVector("_AnimationTiling", animationTiling, sharedWaterMaterial);
			WaterEditorUtility.SetMaterialVector("_AnimationDirection", animationDirection, sharedWaterMaterial);
			
			EditorGUILayout.Separator ();		
			
	    	GUILayout.Label ("Displacement Amounts", EditorStyles.boldLabel);				
			
			float heightDisplacement = WaterEditorUtility.GetMaterialFloat("_HeightDisplacement", sharedWaterMaterial);
			float normalsDisplacement = WaterEditorUtility.GetMaterialFloat("_NormalsDisplacement", sharedWaterMaterial);
			
			heightDisplacement = EditorGUILayout.Slider("Height", heightDisplacement, 0.0F, 7.5F);
			normalsDisplacement = EditorGUILayout.Slider("Normals", normalsDisplacement, 0.0F, 400.0F);
			
			WaterEditorUtility.SetMaterialFloat("_HeightDisplacement", heightDisplacement, sharedWaterMaterial);
			WaterEditorUtility.SetMaterialFloat("_NormalsDisplacement", normalsDisplacement, sharedWaterMaterial);    	
		}
		

		EditorGUILayout.Separator();		
       	
    	GUILayout.Label ("Create Displacement Textures", EditorStyles.boldLabel);
		GUILayout.Label ("Displacement textures as set in the (shared) material \'"+sharedWaterMaterial.name+"\'", EditorStyles.miniBoldLabel);  	
		
       	/*
        EditorGUILayout.BeginHorizontal();
       	firstMap.objectReferenceValue = (Texture2D)EditorGUILayout.ObjectField("Displacement A", firstMap.objectReferenceValue, typeof(Texture2D), false);
       	secondMap.objectReferenceValue = (Texture2D)EditorGUILayout.ObjectField("Displacement B", secondMap.objectReferenceValue, typeof(Texture2D), false);
		EditorGUILayout.EndHorizontal();       	
       	
		EditorGUILayout.BeginHorizontal();
       	thirdMap.objectReferenceValue = (Texture2D)EditorGUILayout.ObjectField("Displacement C", thirdMap.objectReferenceValue, typeof(Texture2D), false);		  	        
		EditorGUILayout.EndHorizontal();       	
      	*/
      	
        EditorGUILayout.BeginHorizontal();
       	        	
    	wavesFrequency = EditorGUILayout.FloatField("Frequency", wavesFrequency);    	
    	textureCreatSize = EditorGUILayout.IntField("Texture Size", textureCreatSize);	    	
    	
		EditorGUILayout.EndHorizontal();
    	
    	if(GUILayout.Button("Generate Textures")) {
    		if(EditorUtility.DisplayDialog
    		(
    			"Update displacement textures?", 
    			"Overwrite the textures 'Displacement A', 'B' and 'C' with (Perlin) noise starting at frequency " + 
    			wavesFrequency + 
    			" with size "+textureCreatSize+" X " + 
    			textureCreatSize, 
    			"Yes", "No") 
    		) 
    		{
    			// no need to delete existing assets first, AssetDatabase.CreateAsset() takes care of that
 
    			Texture2D a = new Texture2D(textureCreatSize,textureCreatSize, TextureFormat.ARGB32, false);
    			Texture2D b = new Texture2D(textureCreatSize,textureCreatSize, TextureFormat.ARGB32, false);
    			Texture2D c = new Texture2D(textureCreatSize,textureCreatSize, TextureFormat.ARGB32, false);
    			
    			AssetDatabase.CreateAsset(a, AssetDatabase.GetAssetPath(firstMap.objectReferenceValue));
    			AssetDatabase.CreateAsset(b, AssetDatabase.GetAssetPath(secondMap.objectReferenceValue));
    			AssetDatabase.CreateAsset(c, AssetDatabase.GetAssetPath(thirdMap.objectReferenceValue));
    			
    			firstMap.objectReferenceValue = a;
    			secondMap.objectReferenceValue = b;
    			thirdMap.objectReferenceValue = c;
    			
    			serObj.ApplyModifiedProperties();  			
    			
    			//((NoiseDisplace)serObj.targetObject).UpdateAndSetHeightfields();
    			PrepareHeightfield(firstMap.objectReferenceValue, wavesFrequency, 1.0f);
    			PrepareHeightfield(secondMap.objectReferenceValue, wavesFrequency * 2.0f, 0.85f);
    			PrepareHeightfield(thirdMap.objectReferenceValue, wavesFrequency * 4.0f, 0.65f);
    			
    			Texture2D aCompressed = new Texture2D(textureCreatSize,textureCreatSize, TextureFormat.ARGB32, false);
    			Texture2D bCompressed = new Texture2D(textureCreatSize,textureCreatSize, TextureFormat.ARGB32, false);
    			Texture2D cCompressed = new Texture2D(textureCreatSize,textureCreatSize, TextureFormat.ARGB32, false);
    			
    			aCompressed.SetPixels(a.GetPixels());
				bCompressed.SetPixels(b.GetPixels());
    			cCompressed.SetPixels(c.GetPixels());
    			
    			aCompressed.Apply();
    			bCompressed.Apply();
    			cCompressed.Apply();
    			
    			aCompressed.Compress(true);
    			bCompressed.Compress(true);
    			cCompressed.Compress(true);

    			AssetDatabase.CreateAsset(aCompressed, AssetDatabase.GetAssetPath(firstMapCompressed.objectReferenceValue));
    			AssetDatabase.CreateAsset(bCompressed, AssetDatabase.GetAssetPath(secondMapCompressed.objectReferenceValue));
    			AssetDatabase.CreateAsset(cCompressed, AssetDatabase.GetAssetPath(thirdMapCompressed.objectReferenceValue));        				
       		
    			firstMapCompressed.objectReferenceValue = aCompressed;
    			secondMapCompressed.objectReferenceValue = bCompressed;
    			thirdMapCompressed.objectReferenceValue = cCompressed;     
    			
    			serObj.ApplyModifiedProperties();  	  
    			
    			WaterEditorUtility.SetMaterialTexture(displace.firstDisplacementSampler, (Texture2D)firstMapCompressed.objectReferenceValue, sharedWaterMaterial);  			  		
    			WaterEditorUtility.SetMaterialTexture(displace.secondDisplacementSampler, (Texture2D)secondMapCompressed.objectReferenceValue, sharedWaterMaterial);  			  		
    			WaterEditorUtility.SetMaterialTexture(displace.thirdDisplacementSampler, (Texture2D)thirdMapCompressed.objectReferenceValue, sharedWaterMaterial);  			  		
       		}
    	}

    	serObj.ApplyModifiedProperties();
    }
    
 	private void PrepareHeightfield(UnityEngine.Object sourceTex, float frequency, float amplitude) 
	{			
		if(null == sourceTex)
			return;
			
		Texture2D tex = (Texture2D)sourceTex;
			
		Perlin perlin = new Perlin();
		
		// tileable 2D perlin noise for heightfield generation
				
		for(int i = 0; i < tex.width; i++) 
		{
			for(int j = 0; j < tex.height; j++) 
			{
				float x = (float)(i);
				float y = (float)(j);

				float w = (float)tex.width;
				float h = (float)tex.height;
				
				x = (x/w)*frequency;
				y = (y/h)*frequency;
				
				w = frequency;
				h = frequency;
				
				x = x%w;
				y = y%h;
				
				float value = (	perlin.Noise(x, y) * (w - x) * (h - y) +
								perlin.Noise(x - w, y) * (x) * (h - y) +
								perlin.Noise(x - w, y - h) * (x) * (y) +
								perlin.Noise(x, y - h) * (w - x) * (y) ) / (w * h);
																
				value = 0.5F * amplitude * value + 0.5F;
								
				tex.SetPixel(i,j, new Color(value,value,value,value));
			}	
		}		
		
		tex.Apply();
		
		// smooth and get gradiants
		
		for(int i = 0; i < tex.width; i++) {
			for(int j = 0; j < tex.height; j++) {
        		Color displ = tex.GetPixel(i,j);
        		Color displR = tex.GetPixel(i-4,j);	
				displR += tex.GetPixel(i-3,j);	
				displR += tex.GetPixel(i-2,j);	
				displR += tex.GetPixel(i-1,j);	
				displR /= 4.0F;
        		Color displU = tex.GetPixel(i,j-4);	
				displU += tex.GetPixel(i,j-3);	
				displU += tex.GetPixel(i,j-2);	
				displU += tex.GetPixel(i,j-1);	
				displU /= 4.0F;
				float r = (displR.a-displ.a);
				float g = (displU.a-displ.a);
				float b = 1.0F-(r*r+g*g);
        		tex.SetPixel(i,j, new Color(r*0.5F+0.5F,g*0.5F+0.5F,b*0.5F+0.5F,displ.a));
			}	
		}
		
		tex.Apply();
	}	   
}


public class SmoothRandom
{
	public static Vector3 GetVector3 (float speed)
	{
		float time = Time.time * 0.01F * speed;
		return new Vector3(Get().HybridMultifractal(time, 15.73F, 0.58F), Get().HybridMultifractal(time, 63.94F, 0.58F), Get().HybridMultifractal(time, 0.2F, 0.58F));
	}
	
	public static float Get (float speed)
	{
		float time = Time.time * 0.01F * speed;
		return Get().HybridMultifractal(time * 0.01F, 15.7F, 0.65F);
	}

	private static FractalNoise Get () { 
		if (s_Noise == null)
			s_Noise = new FractalNoise (1.27F, 2.04F, 8.36F);
		return s_Noise;		
	 }

	private static FractalNoise s_Noise;
}

public class Perlin
{
	// Original C code derived from 
	// http://astronomy.swin.edu.au/~pbourke/texture/perlin/perlin.c
	// http://astronomy.swin.edu.au/~pbourke/texture/perlin/perlin.h
	const int B = 0x100;
	const int BM = 0xff;
	const int N = 0x1000;

	int[] p = new int[B + B + 2];
	float[,] g3 = new float [B + B + 2 , 3];
	float[,] g2 = new float[B + B + 2,2];
	float[] g1 = new float[B + B + 2];

	float s_curve(float t)
	{
		return t * t * (3.0F - 2.0F * t);
	}
	
	float lerp (float t, float a, float b)
	{ 
		return a + t * (b - a);
	}

	void setup (float value, out int b0, out int b1, out float r0, out float r1)
	{ 
        float t = value + N;
        b0 = ((int)t) & BM;
        b1 = (b0+1) & BM;
        r0 = t - (int)t;
        r1 = r0 - 1.0F;
	}
	
	float at2(float rx, float ry, float x, float y) { return rx * x + ry * y; }
	float at3(float rx, float ry, float rz, float x, float y, float z) { return rx * x + ry * y + rz * z; }

	public float Noise(float arg)
	{
		int bx0, bx1;
		float rx0, rx1, sx, u, v;
		setup(arg, out bx0, out bx1, out rx0, out rx1);
		
		sx = s_curve(rx0);
		u = rx0 * g1[ p[ bx0 ] ];
		v = rx1 * g1[ p[ bx1 ] ];
		
		return(lerp(sx, u, v));
	}

	public float Noise(float x, float y)
	{
		int bx0, bx1, by0, by1, b00, b10, b01, b11;
		float rx0, rx1, ry0, ry1, sx, sy, a, b, u, v;
		int i, j;
		
		setup(x, out bx0, out bx1, out rx0, out rx1);
		setup(y, out by0, out by1, out ry0, out ry1);
		
		i = p[ bx0 ];
		j = p[ bx1 ];
		
		b00 = p[ i + by0 ];
		b10 = p[ j + by0 ];
		b01 = p[ i + by1 ];
		b11 = p[ j + by1 ];
		
		sx = s_curve(rx0);
		sy = s_curve(ry0);
		
		u = at2(rx0,ry0, g2[ b00, 0 ], g2[ b00, 1 ]);
		v = at2(rx1,ry0, g2[ b10, 0 ], g2[ b10, 1 ]);
		a = lerp(sx, u, v);
		
		u = at2(rx0,ry1, g2[ b01, 0 ], g2[ b01, 1 ]);
		v = at2(rx1,ry1, g2[ b11, 0 ], g2[ b11, 1 ]);
		b = lerp(sx, u, v);
		
		return lerp(sy, a, b);
	}
	
	public float Noise(float x, float y, float z)
	{
		int bx0, bx1, by0, by1, bz0, bz1, b00, b10, b01, b11;
		float rx0, rx1, ry0, ry1, rz0, rz1, sy, sz, a, b, c, d, t, u, v;
		int i, j;
		
		setup(x, out bx0, out bx1, out rx0, out rx1);
		setup(y, out by0, out by1, out ry0, out ry1);
		setup(z, out bz0, out bz1, out rz0, out rz1);
		
		i = p[ bx0 ];
		j = p[ bx1 ];
		
		b00 = p[ i + by0 ];
		b10 = p[ j + by0 ];
		b01 = p[ i + by1 ];
		b11 = p[ j + by1 ];
		
		t  = s_curve(rx0);
		sy = s_curve(ry0);
		sz = s_curve(rz0);
		
		u = at3(rx0,ry0,rz0, g3[ b00 + bz0, 0 ], g3[ b00 + bz0, 1 ], g3[ b00 + bz0, 2 ]);
		v = at3(rx1,ry0,rz0, g3[ b10 + bz0, 0 ], g3[ b10 + bz0, 1 ], g3[ b10 + bz0, 2 ]);
		a = lerp(t, u, v);
		
		u = at3(rx0,ry1,rz0, g3[ b01 + bz0, 0 ], g3[ b01 + bz0, 1 ], g3[ b01 + bz0, 2 ]);
		v = at3(rx1,ry1,rz0, g3[ b11 + bz0, 0 ], g3[ b11 + bz0, 1 ], g3[ b11 + bz0, 2 ]);
		b = lerp(t, u, v);
		
		c = lerp(sy, a, b);
		
		u = at3(rx0,ry0,rz1, g3[ b00 + bz1, 0 ], g3[ b00 + bz1, 2 ], g3[ b00 + bz1, 2 ]);
		v = at3(rx1,ry0,rz1, g3[ b10 + bz1, 0 ], g3[ b10 + bz1, 1 ], g3[ b10 + bz1, 2 ]);
		a = lerp(t, u, v);
		
		u = at3(rx0,ry1,rz1, g3[ b01 + bz1, 0 ], g3[ b01 + bz1, 1 ], g3[ b01 + bz1, 2 ]);
		v = at3(rx1,ry1,rz1,g3[ b11 + bz1, 0 ], g3[ b11 + bz1, 1 ], g3[ b11 + bz1, 2 ]);
		b = lerp(t, u, v);
		
		d = lerp(sy, a, b);
		
		return lerp(sz, c, d);
	}
	
	void normalize2(ref float x, ref float y)
	{
	   float s;
	
		s = (float)Math.Sqrt(x * x + y * y);
		x = y / s;
		y = y / s;
	}
	
	void normalize3(ref float x, ref float y, ref float z)
	{
		float s;
		s = (float)Math.Sqrt(x * x + y * y + z * z);
		x = y / s;
		y = y / s;
		z = z / s;
	}
	
	public Perlin()
	{
		int i, j, k;
		System.Random rnd = new System.Random();
	
	   for (i = 0 ; i < B ; i++) {
		  p[i] = i;
		  g1[i] = (float)(rnd.Next(B + B) - B) / B;
	
		  for (j = 0 ; j < 2 ; j++)
			 g2[i,j] = (float)(rnd.Next(B + B) - B) / B;
		  normalize2(ref g2[i, 0], ref g2[i, 1]);
	
		  for (j = 0 ; j < 3 ; j++)
			 g3[i,j] = (float)(rnd.Next(B + B) - B) / B;
			 
	
		  normalize3(ref g3[i, 0], ref g3[i, 1], ref g3[i, 2]);
	   }
	
	   while (--i != 0) {
		  k = p[i];
		  p[i] = p[j = rnd.Next(B)];
		  p[j] = k;
	   }
	
	   for (i = 0 ; i < B + 2 ; i++) {
		  p[B + i] = p[i];
		  g1[B + i] = g1[i];
		  for (j = 0 ; j < 2 ; j++)
			 g2[B + i,j] = g2[i,j];
		  for (j = 0 ; j < 3 ; j++)
			 g3[B + i,j] = g3[i,j];
	   }
	}
}

public class FractalNoise
{
	public FractalNoise (float inH, float inLacunarity, float inOctaves)
		: this (inH, inLacunarity, inOctaves, null)
	{
		
	}

	public FractalNoise (float inH, float inLacunarity, float inOctaves, Perlin noise)
	{
		m_Lacunarity = inLacunarity;
		m_Octaves = inOctaves;
		m_IntOctaves = (int)inOctaves;
		m_Exponent = new float[m_IntOctaves+1];
		float frequency = 1.0F;
		for (int i = 0; i < m_IntOctaves+1; i++)
		{
			m_Exponent[i] = (float)Math.Pow (m_Lacunarity, -inH);
			frequency *= m_Lacunarity;
		}
		
		if (noise == null)
			m_Noise = new Perlin();
		else
			m_Noise = noise;
	}
	
	
	public float HybridMultifractal(float x, float y, float offset)
	{
		float weight, signal, remainder, result;
		
		result = (m_Noise.Noise (x, y)+offset) * m_Exponent[0];
		weight = result;
		x *= m_Lacunarity; 
		y *= m_Lacunarity;
		int i;
		for (i=1;i<m_IntOctaves;i++)
		{
			if (weight > 1.0F) weight = 1.0F;
			signal = (m_Noise.Noise (x, y) + offset) * m_Exponent[i];
			result += weight * signal;
			weight *= signal;
			x *= m_Lacunarity; 
			y *= m_Lacunarity;
		}
		remainder = m_Octaves - m_IntOctaves;
		result += remainder * m_Noise.Noise (x,y) * m_Exponent[i];
		
		return result;
	}
	
	public float RidgedMultifractal (float x, float y, float offset, float gain)
	{
		float weight, signal, result;
		int i;
		
		signal = Mathf.Abs (m_Noise.Noise (x, y));
		signal = offset - signal;
		signal *= signal;
		result = signal;
		weight = 1.0F;
	
		for (i=1;i<m_IntOctaves;i++)
		{
			x *= m_Lacunarity; 
			y *= m_Lacunarity;
			
			weight = signal * gain;
			weight = Mathf.Clamp01 (weight);
			
			signal = Mathf.Abs (m_Noise.Noise (x, y));
			signal = offset - signal;
			signal *= signal;
			signal *= weight;
			result += signal * m_Exponent[i];
		}
	
		return result;
	}

	public float BrownianMotion (float x, float y)
	{
		float value, remainder;
		long i;
		
		value = 0.0F;
		for (i=0;i<m_IntOctaves;i++)
		{
			value = m_Noise.Noise (x,y) * m_Exponent[i];
			x *= m_Lacunarity;
			y *= m_Lacunarity;
		}
		remainder = m_Octaves - m_IntOctaves;
		value += remainder * m_Noise.Noise (x,y) * m_Exponent[i];
		
		return value;
	}

	
	private Perlin  m_Noise;
	private float[] m_Exponent;
	private int     m_IntOctaves;
	private float   m_Octaves;
	private float   m_Lacunarity;
}