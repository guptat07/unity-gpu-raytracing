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

            struct RayTracingMaterial
            {
                float4 color;
                float diffuseStrength;
                float ambientStrength;
                float specularStrength;
                float shininess;
                int metallic;
            };

            struct Sphere
            {
                float3 origin;
                float radius;
                RayTracingMaterial material;
            };

            struct HitRecord
            {
                bool hit;
                float tParameter;
                float3 intersectionPoint;
                float3 surfaceNormal;
                RayTracingMaterial material;
            };

            StructuredBuffer<Sphere> spheres;
            int numSpheres;

            #define MAX_DIRECTIONAL_LIGHTS 16
            float4 lightDirections[MAX_DIRECTIONAL_LIGHTS];
            float4 lightColors[MAX_DIRECTIONAL_LIGHTS];
            int numDirectionalLights;

            float4 ambientLight;

            HitRecord RaySphereIntersect(Ray ray, float3 sphereCenter, float sphereRadius)
            {
                HitRecord recordToReturn;
                recordToReturn.hit = false;

                // https://github.com/SebLague/Ray-Tracing/blob/Episode01/Assets/Scripts/Shaders/RayTracing.shader
                // More readable version of discriminant calculation than in my CPU Ray Tracer.
                // Calculates a, b, c separately to use for the formula.
                float a = dot(ray.direction, ray.direction);
                float b = 2.0 * dot(ray.direction, ray.origin - sphereCenter);
                float c = dot(ray.origin - sphereCenter, ray.origin - sphereCenter) - sphereRadius * sphereRadius;
                float discriminant = b * b - 4.0 * a * c;

                if (discriminant >= 0)
                {
                    float t = (-b - sqrt(discriminant)) / (2.0 * a);

                    if (t > 0)
                    {
                        recordToReturn.hit = true;
                        recordToReturn.tParameter = t;
                        recordToReturn.intersectionPoint = ray.origin + t * ray.direction;
                        recordToReturn.surfaceNormal = normalize(recordToReturn.intersectionPoint - sphereCenter);
                    }
                }
                return recordToReturn;
            }

            HitRecord RayHit(Ray ray)
            {
                // Check for intersections with all spheres
                HitRecord closestHit;
                closestHit.hit = false;
                closestHit.tParameter = 1e20;

                for (int i = 0; i < numSpheres; i++)
                {
                    Sphere currentSphere = spheres[i];
                    HitRecord currentHit = RaySphereIntersect(ray, currentSphere.origin, currentSphere.radius);

                    if (currentHit.hit && currentHit.tParameter < closestHit.tParameter)
                    {
                        closestHit = currentHit;
                        closestHit.material = currentSphere.material;
                    }
                }

                return closestHit;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 pixelColor = float4(0, 0, 0, 1);
                
                // Note that i is the current pixel (so i.uv.x is the x position of the pixel in UV space)
                float3 pointOnPlane = float3(i.uv - 0.5, 1.0) * ViewPlane;
                float3 worldPoint = mul(CameraLocalToWorld, float4(pointOnPlane, 1));

                Ray ray;
                // _WorldSpaceCameraPos is a built-in variable giving the camera position in world space
                ray.origin = _WorldSpaceCameraPos;
                ray.direction = normalize(worldPoint - ray.origin);

                // Check for intersections with all spheres
                HitRecord closestHit = RayHit(ray);
                
                // Lighting
                if (closestHit.hit)
                {
                    for (int i = 0; i < numDirectionalLights; i++)
                    {
                        // Check if the point is in shadow
                        Ray shadowRay;
                        shadowRay.origin = closestHit.intersectionPoint + closestHit.surfaceNormal * 0.001;
                        shadowRay.direction = lightDirections[i].xyz;
                        HitRecord shadowHit = RayHit(shadowRay);

                        if (shadowHit.hit)
                        {
                            continue;
                        }
                        else
                        {
                            float nDotL = max(dot(closestHit.surfaceNormal, lightDirections[i].xyz), 0.0);
                            float3 kDiffuse = closestHit.material.diffuseStrength * closestHit.material.color.rgb;
                            float3 temp = kDiffuse * nDotL;
                            pixelColor.rgb += temp * lightColors[i].rgb;

                            float3 vH = normalize(-ray.direction + lightDirections[i].xyz);
                            float nDotH = max(dot(closestHit.surfaceNormal, vH), 0.0);
                            float3 kSpecular;
                            if (closestHit.material.metallic == 1)
                            {
                                kSpecular = closestHit.material.specularStrength * closestHit.material.color.rgb;
                            }
                            else
                            {
                                kSpecular = closestHit.material.specularStrength * float3(1.0, 1.0, 1.0);
                            }
                            temp = kSpecular * pow(nDotH, closestHit.material.shininess);
                            pixelColor.rgb += temp * lightColors[i].rgb;
                        }
                    }

                    // Ambient Lighting
                    float3 kAmbient = closestHit.material.ambientStrength * closestHit.material.color.rgb;
                    pixelColor.rgb += kAmbient * ambientLight.rgb;
                }
                else
                {
                    // Skybox...
                }
                
                return pixelColor;
            }
            ENDCG
        }
    }
}
