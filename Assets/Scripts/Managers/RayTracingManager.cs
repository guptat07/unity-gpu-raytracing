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
    // We need to pass scene info into the shader (like where the spheres are).

    public bool enableSceneView = true;
    public Shader rayTracingShader;
    Material rayTracingMaterial;

    // This function will run when we attach the script to a camera and the camera has rendered.
    // So, it is basically a post-processing effect.
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (Camera.current.name != "SceneCamera" || enableSceneView)
        {
            // Create the material
            if (rayTracingMaterial == null || (rayTracingMaterial.shader != rayTracingShader && rayTracingShader != null))
            {
                if (rayTracingShader == null)
                {
                    rayTracingShader = Shader.Find("Unlit/Texture");
                }

                rayTracingMaterial = new Material(rayTracingShader);
            }

            UpdateViewPlane(Camera.current);

            // Pass the spheres into the shader
            // We can use the component we put on the spheres to collect them all
            RayTracedSphere[] rayTracedSpheres = FindObjectsByType<RayTracedSphere>(FindObjectsSortMode.None);
            // We need this array to be in a format the shader expects
            Sphere[] spheres = new Sphere[rayTracedSpheres.Length];
            // Populate the array
            for (int i = 0; i < rayTracedSpheres.Length; i++)
            {
                spheres[i].origin = rayTracedSpheres[i].transform.position;
                spheres[i].radius = rayTracedSpheres[i].transform.localScale.x * 0.5f;
                spheres[i].material = rayTracedSpheres[i].material;
            }
            // Create the buffer
            // Exact same idea as Assignment 3 and 4
            // Need the number of elements and the size of each element
            // https://discussions.unity.com/t/lenght-of-structs/97487
            // ^ provided the function call for size of struct
            ComputeBuffer sphereBuffer = new ComputeBuffer(spheres.Length, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Sphere)), ComputeBufferType.Structured);
            // Set the data
            sphereBuffer.SetData(spheres);
            // Pass to shader
            rayTracingMaterial.SetBuffer("spheres", sphereBuffer);
            rayTracingMaterial.SetInt("numSpheres", spheres.Length);

            // Pass lights into the shader
            // Collect directional lights
            RayTracedDirectionalLight[] rayTracedDirectionalLights = FindObjectsByType<RayTracedDirectionalLight>(FindObjectsSortMode.None);
            // I think making a sphere struct is maybe redundant, so just pass arrays of data
            Vector4[] lightDirections = new Vector4[rayTracedDirectionalLights.Length];
            Vector4[] lightColors = new Vector4[rayTracedDirectionalLights.Length];
            // Populate arrays
            for (int i = 0; i < rayTracedDirectionalLights.Length; i++)
            {
                Light light = rayTracedDirectionalLights[i].GetComponent<Light>();

                lightDirections[i] =  (-rayTracedDirectionalLights[i].transform.forward).normalized;
                lightColors[i] = light.color * light.intensity;
            }
            // Pass to shader
            rayTracingMaterial.SetVectorArray("lightDirections", lightDirections);
            rayTracingMaterial.SetVectorArray("lightColors", lightColors);
            rayTracingMaterial.SetInt("numDirectionalLights", rayTracedDirectionalLights.Length);

            // Get the ambient light
            Color ambientLight = RenderSettings.ambientLight;
            // Pass to shader
            rayTracingMaterial.SetVector("ambientLight", ambientLight);

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
