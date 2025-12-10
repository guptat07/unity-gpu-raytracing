using UnityEngine;

[System.Serializable]
public struct RayTracingMaterial
{
    public Color color;

    public void SetDefaults()
    {
        color = Color.white;
    }
}
