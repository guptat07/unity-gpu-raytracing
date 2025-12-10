// Thanks to Sebastian Lague and David Kuri for their tutorials on GPU ray tracing in Unity.
// https://www.youtube.com/watch?v=Qz0KTGYJtUk
// https://www.gamedeveloper.com/programming/gpu-ray-tracing-in-unity-part-1
using UnityEngine;

[ExecuteAlways, ImageEffectAllowedInSceneView]
public class RayTracingManager : MonoBehaviour
{
    // Basically, we need to create a fragment shader and then dispatch it.
    // This fragment shader needs the ray that it should use for the calculation.
    // Instead of calculating the ray, we can pass in the camera parameters and have the shader calculate it.
    // The fragment shader will perform a common calculation for each pixel using its unique ray.

    public bool enableSceneView = true;
    Material rayTracingMaterial;

    // This function will run when we attach the script to a camera and the camera has rendered.
    // So, it is basically a post-processing effect.
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (Camera.current.name != "SceneCamera" || enableSceneView)
        {
            // Create the material
            //ShaderManager.CreateMaterial();

            UpdateViewPlane(Camera.current);
            Graphics.Blit(null, destination, rayTracingMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    void UpdateViewPlane(Camera camera)
    {
        // Determine the projection plane
        float planeHeight = camera.nearClipPlane * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f) * 2f;
        float planeWidth = planeHeight * camera.aspect;

        rayTracingMaterial.SetVector("ViewPlane", new Vector3(planeWidth, planeHeight, camera.nearClipPlane));
        rayTracingMaterial.SetMatrix("CameraLocalToWorld", camera.transform.localToWorldMatrix);
    }
}
