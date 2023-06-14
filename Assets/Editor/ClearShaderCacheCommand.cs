using System.IO;
using UnityEditor;
using UnityEngine;


// Do not create Editor namespace for this class

public class ClearShaderCacheCommand : MonoBehaviour
{
    [MenuItem("Tools/Clear shader cache")]
    public static void ClearShaderCache()
    {
        var shaderCachePath = Path.Combine(Application.dataPath, "../Library/ShaderCache");
        Directory.Delete(shaderCachePath, true);
    }
}
