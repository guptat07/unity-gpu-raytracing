using UnityEngine;

public class RayTracedSphere : MonoBehaviour
{
    public RayTracingMaterial material;
    private bool initialized = false;
    private int materialObjectID;

    void OnValidate()
    {
        if (!initialized)
        {
            material.SetDefaults();
            initialized = true;
        }

        // This actually applies the RayTracingMaterial('s color) to the Mesh.
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            // We want to change just this instance of the material, not the material itself.
            // Otherwise, we can't have different colored spheres.
            if (materialObjectID != gameObject.GetInstanceID())
            {
                materialObjectID = gameObject.GetInstanceID();
                renderer.sharedMaterial = new Material(renderer.sharedMaterial);
            }
            
            renderer.sharedMaterial.color = material.color;
        }
    }
}
