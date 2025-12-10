Shader "Unlit/RayTracing"
{
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Vertex shader, just passes through info
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float3 ViewPlane;
            float4x4 CameraLocalToWorld;
            
            struct Ray
            {
                float3 origin;
                float3 direction;
            };

            fixed4 frag (v2f i) : SV_Target
            {
                // Note that i is the current pixel (so i.uv.x is the x position of the pixel in UV space)
                float3 pointOnPlane = float3(i.uv - 0.5, 1.0) * ViewPlane;
                float3 worldPoint = mul(CameraLocalToWorld, float4(pointOnPlane, 1));

                Ray ray;
                // _WorldSpaceCameraPos is a built-in variable giving the camera position in world space
                ray.origin = _WorldSpaceCameraPos;
                ray.direction = normalize(worldPoint - ray.origin);
                return float4(ray.direction, 0);
            }
            ENDCG
        }
    }
}
