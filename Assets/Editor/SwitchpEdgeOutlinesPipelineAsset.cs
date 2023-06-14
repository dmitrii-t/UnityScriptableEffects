using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class SwitchpEdgeOutlinesPipelineAsset : MonoBehaviour
{
    public static RenderPipelineAsset edgeOutlinesPipelineAsset; // TODO make it working

    [MenuItem("Tools/Enable Edge Outlines")]
    public static void ClearShaderCache()
    {
        if (edgeOutlinesPipelineAsset != null)
        {
            QualitySettings.renderPipeline = edgeOutlinesPipelineAsset;
            GraphicsSettings.defaultRenderPipeline = edgeOutlinesPipelineAsset;
        }
    }
}
