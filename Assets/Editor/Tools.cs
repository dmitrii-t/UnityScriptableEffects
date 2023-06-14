using System.IO;
using UnityEditor;
using UnityEngine;
using System;

// Do not create Editor namespace for this class

public class Tools : MonoBehaviour
{
    [MenuItem("Tools/Clear shader cache")]
    public static void ClearShaderCache()
    {
        var shaderCachePath = Path.Combine(Application.dataPath, "../Library/ShaderCache");
        Directory.Delete(shaderCachePath, true);
    }
    
    [MenuItem("Tools/Take screenshot")]
    public static void Screenshot()
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss-ffff");
        ScreenCapture.CaptureScreenshot($"Assets/Screenshots/Screenshot-{timestamp}.png");
    }
}
