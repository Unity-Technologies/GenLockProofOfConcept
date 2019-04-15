using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowLastRenderTexture : MonoBehaviour
{
    public GenLockedRecorder GenLockedRecorder;
    public RawImage RawImage;
    
    void Update()
    {
        RenderTexture tex = GenLockedRecorder.GetLastRender();
        if (tex)
		{
            RawImage.texture = tex;
			RawImage.SetNativeSize();
		}
    }
}
