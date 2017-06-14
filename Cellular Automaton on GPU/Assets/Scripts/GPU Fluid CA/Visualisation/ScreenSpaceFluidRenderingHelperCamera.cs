using UnityEngine;

namespace GPUFluid
{
    [RequireComponent(typeof(Camera))]
    public class ScreenSpaceFluidRenderingHelperCamera : MonoBehaviour
    {
        private Camera cam;
        private ScreenSpaceFluidRendering ssfr;

        void Awake()
        {
            cam = GetComponent<Camera>();
            ssfr = GetComponentInParent<ScreenSpaceFluidRendering>();
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            var p = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);
            p[2, 3] = p[3, 2] = 0.0f;
            p[3, 3] = 1.0f;
            var clipToWorld = Matrix4x4.Inverse(p * cam.worldToCameraMatrix) * Matrix4x4.TRS(new Vector3(0, 0, -p[2, 2]), Quaternion.identity, Vector3.one);
            ssfr.blend.SetMatrix("clipToWorld", clipToWorld);
            ssfr.blend.SetMatrix("viewProj", cam.worldToCameraMatrix * cam.projectionMatrix);
            Graphics.Blit(source, destination, ssfr.blend);
        }
    }
}
