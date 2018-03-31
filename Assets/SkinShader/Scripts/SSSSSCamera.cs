using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent (typeof(Camera))]
public class SSSSSCamera : MonoBehaviour
{
	#region MASK_SPACE

	[System.NonSerialized]
	public bool isScreenChanged;
	[System.NonSerialized]
	public int width;
	[System.NonSerialized]
	public int height;
	Camera cam;
	Camera depthCam;
	Shader depthShader;
	Material mat;
	[Range (0.02f, 100)]
	public float blurRange = 20;

	[Range (0, 10)]
	public float offset = 0.5f;
	public LayerMask skinLayer = 1 << 31;
	int tempID1;
	int tempID2;
	int _RangeID;
	// Use this for initialization
	void Awake ()
	{
		width = Screen.width;
		height = Screen.height;
		cam = GetComponent<Camera> ();
		cam.depthTextureMode |= DepthTextureMode.Depth;
		var camG = new GameObject ("Depth Camera", typeof(Camera));
		camG.hideFlags = HideFlags.HideAndDontSave;
		depthCam = camG.GetComponent<Camera> ();
		depthCam.CopyFrom (cam);
		camG.transform.SetParent (transform);
		camG.transform.localPosition = Vector3.zero;
		camG.transform.localRotation = Quaternion.identity;
		camG.transform.localScale = Vector3.one;
		depthCam.renderingPath = RenderingPath.Forward;
		depthCam.SetReplacementShader (Shader.Find ("Hidden/Depth"), "RenderType");
		depthCam.farClipPlane = blurRange;
		depthCam.clearFlags = CameraClearFlags.Color;
		depthCam.backgroundColor = Color.red;
		depthCam.cullingMask = (~skinLayer) & depthCam.cullingMask;
		depthCam.depthTextureMode = DepthTextureMode.None;
		depthCam.enabled = false;
		_RangeID = Shader.PropertyToID ("_Range");
		originMapID = Shader.PropertyToID ("_OriginTex");
		blendTexID = Shader.PropertyToID ("_BlendTex");
		mat = new Material (Shader.Find ("Hidden/SSSSS"));
		blendWeightID = Shader.PropertyToID ("_BlendWeight");
		offsetID = Shader.PropertyToID ("_Offset");
		mat.SetFloat (offsetID, offset);
		camG.AddComponent<SSSSSDepthCamera> ().current = this;
		tempID1 = Shader.PropertyToID ("_CullFrontDepthTex");
		tempID2 = Shader.PropertyToID ("_Temp2");
	}

	void OnPreRender ()
	{
		isScreenChanged = (width != Screen.width) || (height != Screen.height);
		Shader.SetGlobalFloat (_RangeID, blurRange);
		depthCam.projectionMatrix = cam.projectionMatrix;
		depthCam.Render ();
	}

	void OnDestroy ()
	{
		if (depthCam)
			Destroy (depthCam.gameObject);
	}

	#endregion

	#region POST_PROCESS

	int originMapID;
	int blendTexID;
	int blendWeightID;
	int offsetID;

	void OnRenderImage (RenderTexture src, RenderTexture dest)
	{


		#if UNITY_EDITOR
		mat.SetFloat (offsetID, offset);
		#endif
		RenderTexture origin = RenderTexture.GetTemporary (src.descriptor);
		RenderTexture copyOri = RenderTexture.GetTemporary (src.descriptor);
		Graphics.Blit (src, origin);
		mat.SetTexture (originMapID, src);
		RenderTexture blur1 = RenderTexture.GetTemporary (src.descriptor);
		RenderTexture blur2 = RenderTexture.GetTemporary (src.descriptor);

		mat.SetTexture (blendTexID, origin);
		mat.SetVector (blendWeightID, new Vector4 (0.33f, 0.45f, 0.36f));
		Graphics.Blit (origin, blur1, mat, 0);
		Graphics.Blit (blur1, blur2, mat, 1);
		Graphics.Blit (blur2, copyOri, mat, 2);

		mat.SetTexture (blendTexID, copyOri);
		mat.SetVector (blendWeightID, new Vector4 (0.34f, 0.19f));
		Graphics.Blit (copyOri, blur1, mat, 0);
		Graphics.Blit (blur1, blur2, mat, 1);
		Graphics.Blit (blur2, origin, mat, 2);

		mat.SetTexture (blendTexID, origin);
		mat.SetVector (blendWeightID, new Vector4 (0.46f, 0f, 0.04f));
		Graphics.Blit (origin, blur1, mat, 0);
		Graphics.Blit (blur1, blur2, mat, 1);
		Graphics.Blit (blur2, copyOri, mat, 2);

		Graphics.Blit (copyOri, dest, mat, 3);
		RenderTexture.ReleaseTemporary (blur1);
		RenderTexture.ReleaseTemporary (blur2);
		RenderTexture.ReleaseTemporary (origin);
		RenderTexture.ReleaseTemporary (copyOri);

	}

	#endregion
}
