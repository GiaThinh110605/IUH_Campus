using System;
using UnityEngine;

namespace IUHCampus.Guardian.Backend
{
    public static class GuardianImageEncoder
    {
        public static Vector2Int OutputSize(int width, int height, int maxSide)
        {
            if (width < 1 || height < 1 || maxSide < 1) throw new ArgumentOutOfRangeException();
            float scale = Mathf.Min(1f, (float)maxSide / Mathf.Max(width, height));
            return new Vector2Int(Mathf.Max(1, Mathf.RoundToInt(width * scale)), Mathf.Max(1, Mathf.RoundToInt(height * scale)));
        }
        public static byte[] Encode(Texture2D frame, int maxImageSide = 640, int jpgQuality = 80) => EncodeTexture(frame, maxImageSide, jpgQuality);
        public static byte[] Encode(WebCamTexture frame, int maxImageSide = 640, int jpgQuality = 80) => EncodeTexture(frame, maxImageSide, jpgQuality);

        private static byte[] EncodeTexture(Texture source, int maxSide, int quality)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var size = OutputSize(source.width, source.height, maxSide);
            var previous = RenderTexture.active;
            var buffer = RenderTexture.GetTemporary(size.x, size.y, 0, RenderTextureFormat.ARGB32);
            Texture2D copy = null;
            try
            {
                Graphics.Blit(source, buffer); RenderTexture.active = buffer;
                copy = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
                copy.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0); copy.Apply();
                return copy.EncodeToJPG(Mathf.Clamp(quality, 1, 100));
            }
            finally
            {
                RenderTexture.active = previous; RenderTexture.ReleaseTemporary(buffer);
                if (copy != null)
                { if (Application.isPlaying) UnityEngine.Object.Destroy(copy); else UnityEngine.Object.DestroyImmediate(copy); }
            }
        }
    }
}
