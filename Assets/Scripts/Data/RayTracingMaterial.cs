using System.ComponentModel;
using UnityEngine;

[System.Serializable]
public struct RayTracingMaterial
{
    public Color color;
    // Different approach than I took before
    // In CPU, my material didn't store a base color
    // Let's store a color now and then use a float for the strengths, and then just calculate the k values when needed.
    [Range(0.1f,1)] public float diffuseStrength;
    [Range(0.1f,1)] public float ambientStrength;
    // public Vector3 speciularCoefficient;
    // public float shininess;


    public void SetDefaults()
    {
        color = Color.white;
        diffuseStrength = 1.0f;
        ambientStrength = 0.1f;
    }
}
