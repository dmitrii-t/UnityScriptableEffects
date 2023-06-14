using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public abstract class Checks
{
    public static void CheckNotNull(object obj, string error)
    {
        if (obj is null)
        {
            throw new Exception(error);
        }
    }

    public void IfGameCamera(RenderingData renderingData, Action action)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            action();
        }
    }
}
