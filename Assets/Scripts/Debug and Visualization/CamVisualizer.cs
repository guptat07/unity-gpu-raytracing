using Unity.Collections;
using UnityEngine;

[ExecuteAlways]
public class CamVisualizer : MonoBehaviour
{
    [Min(1)] public int numRows = 16;
    [Min(1)] public int numCols = 9;
    [Min(0.001f)] public float dotRadius = 0.025f;
    public Color dotColor = Color.white;

    void OnDrawGizmos()
    {
        CameraVisualization();
    }

    void CameraVisualization()
    {
        Camera camera = Camera.main;
        Transform cameraT = camera.transform;
        Gizmos.color = dotColor;

        // Determine the projection plane
        float planeHeight = camera.nearClipPlane * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f) * 2f;
        float planeWidth = planeHeight * camera.aspect;
        Vector3 planeLowerLeft = new Vector3(-planeWidth / 2f, -planeHeight / 2f, camera.nearClipPlane);

        for (int x = 0; x < numRows; x++)
        {
            for (int y = 0; y < numCols; y++)
            {
                float u = x / (float)(numRows - 1);
                float v = y / (float)(numCols - 1);

                Vector3 pointOnPlane = planeLowerLeft + new Vector3(u * planeWidth, v * planeHeight, 0f);
                Vector3 worldPoint = cameraT.position + cameraT.right * pointOnPlane.x + cameraT.up * pointOnPlane.y + cameraT.forward * pointOnPlane.z;
                Vector3 direction = (worldPoint - cameraT.position);

                Gizmos.DrawSphere(worldPoint, dotRadius);
                Gizmos.DrawRay(cameraT.position, direction);
            }
        }
    }
}
