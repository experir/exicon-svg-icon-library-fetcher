#if EXICON_VECTORGRAPHICS
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.VectorGraphics;

namespace SvgIconFetcher.Core
{
    /// <summary>
    /// Converts SVG markup to PNG textures using Unity.VectorGraphics.
    /// Pipeline: SVGParser → TessellateScene → FillMesh → CommandBuffer render → PNG.
    /// Avoids BuildSprite / Sprite.OverrideGeometry which is restricted to
    /// the asset-import phase and throws at editor-runtime.
    /// Non-square SVGs are centered inside the square output with transparent padding.
    /// </summary>
    public static class SvgToPngConverter
    {
        /// <summary>
        /// Converts an SVG string to a PNG byte array at the specified size.
        /// </summary>
        /// <param name="svgContent">Raw SVG markup</param>
        /// <param name="size">Width and height in pixels (square output)</param>
        /// <returns>PNG-encoded byte array, or null on failure</returns>
        public static byte[] Convert(string svgContent, int size = 256)
        {
            if (string.IsNullOrEmpty(svgContent))
                return null;

            Mesh mesh = null;
            Material mat = null;
            RenderTexture rt = null;
            Texture2D tex = null;

            try
            {
                // 1. Parse SVG
                SVGParser.SceneInfo sceneInfo;
                using (var reader = new StringReader(svgContent))
                    sceneInfo = SVGParser.ImportSVG(reader);

                if (sceneInfo.Scene?.Root == null)
                    return null;

                // 2. Tessellate
                var tessOptions = new VectorUtils.TessellationOptions
                {
                    StepDistance = 100f,
                    MaxCordDeviation = 0.5f,
                    MaxTanAngleDeviation = 0.1f,
                    SamplingStepSize = 0.01f
                };

                var geom = VectorUtils.TessellateScene(sceneInfo.Scene, tessOptions);
                if (geom == null || geom.Count == 0)
                    return null;

                // 3. Build a standard Unity Mesh from the tessellated geometry.
                //    This avoids Sprite.OverrideGeometry entirely.
                mesh = new Mesh();
                VectorUtils.FillMesh(mesh, geom, 1.0f);

                if (mesh.vertexCount == 0)
                    return null;

                // 4. Compute a square orthographic viewport from the SVG scene rect.
                //    Non-square content is centered with transparent padding.
                var svgRect = sceneInfo.SceneViewport;
                float maxDim = Mathf.Max(svgRect.width, svgRect.height);
                if (maxDim <= 0f) maxDim = 1f;

                float cx = svgRect.x + svgRect.width  * 0.5f;
                float cy = svgRect.y + svgRect.height * 0.5f;
                float half = maxDim * 0.5f;

                // SVG coordinate system is Y-down.
                // Ortho(left, right, bottom, top, ...) with bottom > top flips Y
                // so that SVG-Y=0 lands at the top of the texture.
                float left   = cx - half;
                float right  = cx + half;
                float bottom = cy + half;   // larger SVG-Y  → screen bottom
                float top    = cy - half;   // smaller SVG-Y → screen top

                var projMatrix = Matrix4x4.Ortho(left, right, bottom, top, -1f, 1f);
                var viewMatrix = Matrix4x4.identity;

                // 5. Render target (with MSAA for smooth edges)
                int aa = 8;
                rt = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, aa);

                // 6. Material — Sprites/Default respects vertex colours produced by FillMesh
                mat = new Material(Shader.Find("Sprites/Default"));

                // 7. Draw via CommandBuffer
                using (var cmd = new CommandBuffer { name = "SVG→PNG" })
                {
                    cmd.SetRenderTarget(rt);
                    cmd.ClearRenderTarget(true, true, Color.clear);
                    cmd.SetViewProjectionMatrices(viewMatrix, projMatrix);
                    cmd.DrawMesh(mesh, Matrix4x4.identity, mat, 0, 0);
                    Graphics.ExecuteCommandBuffer(cmd);
                }

                // 8. Resolve MSAA → read pixels
                //    When MSAA > 1 we must resolve into a non-AA texture first.
                if (aa > 1)
                {
                    var resolve = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
                    Graphics.Blit(rt, resolve);
                    var prevRT = RenderTexture.active;
                    RenderTexture.active = resolve;
                    tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                    tex.ReadPixels(new Rect(0, 0, size, size), 0, 0);
                    tex.Apply();
                    RenderTexture.active = prevRT;
                    RenderTexture.ReleaseTemporary(resolve);
                }
                else
                {
                    var prevRT = RenderTexture.active;
                    RenderTexture.active = rt;
                    tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                    tex.ReadPixels(new Rect(0, 0, size, size), 0, 0);
                    tex.Apply();
                    RenderTexture.active = prevRT;
                }

                return tex.EncodeToPNG();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"SVG→PNG conversion error: {e.Message}");
                return null;
            }
            finally
            {
                if (mesh != null) Object.DestroyImmediate(mesh);
                if (mat  != null) Object.DestroyImmediate(mat);
                if (tex  != null) Object.DestroyImmediate(tex);
                if (rt   != null) RenderTexture.ReleaseTemporary(rt);
            }
        }

        /// <summary>
        /// Converts an SVG string to PNG and writes it to disk.
        /// Returns true on success.
        /// </summary>
        public static bool SavePng(string svgContent, string pngPath, int size = 256)
        {
            var pngBytes = Convert(svgContent, size);
            if (pngBytes == null)
                return false;

            var directory = Path.GetDirectoryName(pngPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllBytes(pngPath, pngBytes);
            return true;
        }
    }
}
#endif
