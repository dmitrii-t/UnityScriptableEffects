using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DitheringRenderFeature : ScriptableRendererFeature
{
    private DitheringEffectPass m_DitheringPass;

    private RTHandle m_TmpTexRT;

    private const string m_TmpTexName = "_TempTexture";

    /// <inheritdoc/>
    public override void Create()
    {
        // we configure where the render pass should be injected.
        m_DitheringPass = new DitheringEffectPass();
        // m_DitheringPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType != CameraType.Game)
        {
            return;
        }

        // Setting up camera color RT
        var descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.msaaSamples = 1;
        descriptor.depthBufferBits = 0;

        // Setting up tmp color RT 
        RenderingUtils.ReAllocateIfNeeded(ref m_TmpTexRT, descriptor, FilterMode.Point, TextureWrapMode.Clamp, name: m_TmpTexName);

        m_DitheringPass.SetRenderTargets(renderer.cameraColorTargetHandle, m_TmpTexRT);
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // we only want to run this pass on the main camera
        if (renderingData.cameraData.cameraType != CameraType.Game)
        {
            return;
        }

        renderer.EnqueuePass(m_DitheringPass);
    }

    protected override void Dispose(bool disposing)
    {
        m_TmpTexRT?.Release();
    }
}
