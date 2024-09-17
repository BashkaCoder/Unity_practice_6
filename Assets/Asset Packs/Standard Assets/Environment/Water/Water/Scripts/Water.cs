using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Water
{
    public class Water : MonoBehaviour
    {
        public enum WaterMode
        {
            Simple = 0,
            Reflective = 1,
            Refractive = 2,
        };

        public WaterMode waterMode = WaterMode.Refractive;
        public bool disablePixelLights = true;
        public int textureSize = 256;
        public float clipPlaneOffset = 0.07f;
        public LayerMask reflectLayers = -1;
        public LayerMask refractLayers = -1;

        private Renderer _renderer;
        private Dictionary<Camera, Camera> m_ReflectionCameras = new Dictionary<Camera, Camera>();
        private Dictionary<Camera, Camera> m_RefractionCameras = new Dictionary<Camera, Camera>();
        private RenderTexture m_ReflectionTexture;
        private RenderTexture m_RefractionTexture;
        private WaterMode m_HardwareWaterSupport = WaterMode.Refractive;
        private int m_OldReflectionTextureSize;
        private int m_OldRefractionTextureSize;
        private static bool s_InsideWater;

        private void Start()
        {
            _renderer = GetComponent<Renderer>();
        }

        public void OnWillRenderObject()
        {
            if (!enabled || !_renderer || !_renderer.sharedMaterial || !_renderer.enabled || Camera.current == null || s_InsideWater)
            {
                return;
            }

            s_InsideWater = true;

            Camera cam = Camera.current;
            m_HardwareWaterSupport = FindHardwareWaterSupport();
            WaterMode mode = GetWaterMode();

            CreateWaterObjects(cam, out var reflectionCamera, out var refractionCamera);

            Vector3 pos = transform.position;
            Vector3 normal = transform.up;

            if (disablePixelLights)
            {
                QualitySettings.pixelLightCount = 0;
            }

            UpdateCameraModes(cam, reflectionCamera);
            UpdateCameraModes(cam, refractionCamera);

            if (mode >= WaterMode.Reflective)
            {
                RenderReflection(cam, reflectionCamera, pos, normal);
            }

            if (mode >= WaterMode.Refractive)
            {
                RenderRefraction(cam, refractionCamera, pos, normal);
            }

            if (disablePixelLights)
            {
                QualitySettings.pixelLightCount = QualitySettings.pixelLightCount;
            }

            SetWaterShaderKeywords(mode);

            s_InsideWater = false;
        }

        private void RenderReflection(Camera cam, Camera reflectionCamera, Vector3 pos, Vector3 normal)
        {
            float d = -Vector3.Dot(normal, pos) - clipPlaneOffset;
            Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

            Matrix4x4 reflection = Matrix4x4.zero;
            CalculateReflectionMatrix(ref reflection, reflectionPlane);
            Vector3 oldpos = cam.transform.position;
            reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;

            Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, 1.0f);
            reflectionCamera.projectionMatrix = cam.CalculateObliqueMatrix(clipPlane);

            reflectionCamera.cullingMask = ~(1 << 4) & reflectLayers.value;
            reflectionCamera.targetTexture = m_ReflectionTexture;
            GL.invertCulling = true;
            reflectionCamera.transform.position = reflection.MultiplyPoint(oldpos);
            reflectionCamera.transform.rotation = Quaternion.LookRotation(reflection.MultiplyVector(cam.transform.forward), reflection.MultiplyVector(cam.transform.up));
            reflectionCamera.Render();
            GL.invertCulling = false;

            _renderer.sharedMaterial.SetTexture("_ReflectionTex", m_ReflectionTexture);
        }

        private void RenderRefraction(Camera cam, Camera refractionCamera, Vector3 pos, Vector3 normal)
        {
            refractionCamera.worldToCameraMatrix = cam.worldToCameraMatrix;

            Vector4 clipPlane = CameraSpacePlane(refractionCamera, pos, normal, -1.0f);
            refractionCamera.projectionMatrix = cam.CalculateObliqueMatrix(clipPlane);

            refractionCamera.cullingMask = ~(1 << 4) & refractLayers.value;
            refractionCamera.targetTexture = m_RefractionTexture;
            refractionCamera.transform.position = cam.transform.position;
            refractionCamera.transform.rotation = cam.transform.rotation;
            refractionCamera.Render();

            _renderer.sharedMaterial.SetTexture("_RefractionTex", m_RefractionTexture);
        }

        private void SetWaterShaderKeywords(WaterMode mode)
        {
            switch (mode)
            {
                case WaterMode.Simple:
                    Shader.EnableKeyword("WATER_SIMPLE");
                    Shader.DisableKeyword("WATER_REFLECTIVE");
                    Shader.DisableKeyword("WATER_REFRACTIVE");
                    break;
                case WaterMode.Reflective:
                    Shader.DisableKeyword("WATER_SIMPLE");
                    Shader.EnableKeyword("WATER_REFLECTIVE");
                    Shader.DisableKeyword("WATER_REFRACTIVE");
                    break;
                case WaterMode.Refractive:
                    Shader.DisableKeyword("WATER_SIMPLE");
                    Shader.DisableKeyword("WATER_REFLECTIVE");
                    Shader.EnableKeyword("WATER_REFRACTIVE");
                    break;
            }
        }

        private void OnDisable()
        {
            DestroyResources();
        }

        private void DestroyResources()
        {
            if (m_ReflectionTexture)
            {
                DestroyImmediate(m_ReflectionTexture);
                m_ReflectionTexture = null;
            }

            if (m_RefractionTexture)
            {
                DestroyImmediate(m_RefractionTexture);
                m_RefractionTexture = null;
            }

            foreach (var kvp in m_ReflectionCameras)
            {
                DestroyImmediate(kvp.Value.gameObject);
            }

            m_ReflectionCameras.Clear();

            foreach (var kvp in m_RefractionCameras)
            {
                DestroyImmediate(kvp.Value.gameObject);
            }

            m_RefractionCameras.Clear();
        }

        private void Update()
        {
            if (_renderer == null) return;

            Material mat = _renderer.sharedMaterial;
            if (!mat) return;

            Vector4 waveSpeed = mat.GetVector("WaveSpeed");
            float waveScale = mat.GetFloat("_WaveScale");
            Vector4 waveScale4 = new Vector4(waveScale, waveScale, waveScale * 0.4f, waveScale * 0.45f);

            double t = Time.timeSinceLevelLoad / 20.0;
            Vector4 offsetClamped = new Vector4(
                (float)Mathf.Repeat(waveSpeed.x * waveScale4.x * (float)t, 1.0f),
                (float)Mathf.Repeat(waveSpeed.y * waveScale4.y * (float)t, 1.0f),
                (float)Mathf.Repeat(waveSpeed.z * waveScale4.z * (float)t, 1.0f),
                (float)Mathf.Repeat(waveSpeed.w * waveScale4.w * (float)t, 1.0f)
            );

            mat.SetVector("_WaveOffset", offsetClamped);
            mat.SetVector("_WaveScale4", waveScale4);
        }

        private void UpdateCameraModes(Camera src, Camera dest)
        {
            if (dest == null) return;

            dest.clearFlags = src.clearFlags;
            dest.backgroundColor = src.backgroundColor;

            if (src.clearFlags == CameraClearFlags.Skybox)
            {
                Skybox sky = src.GetComponent<Skybox>();
                Skybox mysky = dest.GetComponent<Skybox>();
                if (sky != null && sky.material != null)
                {
                    mysky.enabled = true;
                    mysky.material = sky.material;
                }
                else
                {
                    mysky.enabled = false;
                }
            }

            dest.farClipPlane = src.farClipPlane;
            dest.nearClipPlane = src.nearClipPlane;
            dest.orthographic = src.orthographic;
            dest.fieldOfView = src.fieldOfView;
            dest.aspect = src.aspect;
            dest.orthographicSize = src.orthographicSize;
        }

        private void CreateWaterObjects(Camera currentCamera, out Camera reflectionCamera, out Camera refractionCamera)
        {
            WaterMode mode = GetWaterMode();

            reflectionCamera = null;
            refractionCamera = null;

            if (mode >= WaterMode.Reflective)
            {
                CreateTexture(ref m_ReflectionTexture, ref m_OldReflectionTextureSize, "__WaterReflection");
                CreateCamera(currentCamera, ref reflectionCamera, m_ReflectionCameras);
            }

            if (mode >= WaterMode.Refractive)
            {
                CreateTexture(ref m_RefractionTexture, ref m_OldRefractionTextureSize, "__WaterRefraction");
                CreateCamera(currentCamera, ref refractionCamera, m_RefractionCameras);
            }
        }

        private void CreateTexture(ref RenderTexture texture, ref int oldSize, string textureName)
        {
            if (texture == null || oldSize != textureSize)
            {
                if (texture != null)
                {
                    DestroyImmediate(texture);
                }
                texture = new RenderTexture(textureSize, textureSize, 16)
                {
                    name = textureName + GetInstanceID(),
                    isPowerOfTwo = true,
                    hideFlags = HideFlags.DontSave
                };
                oldSize = textureSize;
            }
        }

        private void CreateCamera(Camera currentCamera, ref Camera waterCamera, Dictionary<Camera, Camera> cameraDictionary)
        {
            if (!cameraDictionary.TryGetValue(currentCamera, out waterCamera))
            {
                GameObject go = new GameObject($"Water Camera id{GetInstanceID()} for {currentCamera.GetInstanceID()}", typeof(Camera), typeof(Skybox));
                waterCamera = go.GetComponent<Camera>();
                waterCamera.enabled = false;
                waterCamera.transform.position = transform.position;
                waterCamera.transform.rotation = transform.rotation;
                waterCamera.gameObject.AddComponent<FlareLayer>();
                go.hideFlags = HideFlags.HideAndDontSave;
                cameraDictionary[currentCamera] = waterCamera;
            }
        }

        private WaterMode FindHardwareWaterSupport()
        {
            if (!SystemInfo.supportsRenderTextures || !GetComponent<Renderer>())
            {
                return WaterMode.Simple;
            }
            return waterMode;
        }

        private WaterMode GetWaterMode()
        {
            if (m_HardwareWaterSupport < waterMode)
            {
                return m_HardwareWaterSupport;
            }
            return waterMode;
        }

        private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
        {
            reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
            reflectionMat.m01 = (-2F * plane[0] * plane[1]);
            reflectionMat.m02 = (-2F * plane[0] * plane[2]);
            reflectionMat.m03 = (-2F * plane[3] * plane[0]);

            reflectionMat.m10 = (-2F * plane[1] * plane[0]);
            reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
            reflectionMat.m12 = (-2F * plane[1] * plane[2]);
            reflectionMat.m13 = (-2F * plane[3] * plane[1]);

            reflectionMat.m20 = (-2F * plane[2] * plane[0]);
            reflectionMat.m21 = (-2F * plane[2] * plane[1]);
            reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
            reflectionMat.m23 = (-2F * plane[3] * plane[2]);

            reflectionMat.m30 = 0F;
            reflectionMat.m31 = 0F;
            reflectionMat.m32 = 0F;
            reflectionMat.m33 = 1F;
        }

        private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3 offsetPos = pos + normal * clipPlaneOffset;
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cPos = m.MultiplyPoint(offsetPos);
            Vector3 cNormal = m.MultiplyVector(normal).normalized * sideSign;

            return new Vector4(cNormal.x, cNormal.y, cNormal.z, -Vector3.Dot(cPos, cNormal));
        }
    }
}