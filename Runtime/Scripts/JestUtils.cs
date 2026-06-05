using UnityEngine;

namespace com.jest.sdk
{
    /// <summary>
    /// General SDK utility helpers. Includes encoding textures and sprites into the base64
    /// data URLs that SDK APIs accept (for example, the referral share image). Encoding
    /// works regardless of the source texture's compression format or Read/Write import
    /// settings — the source is rendered into an uncompressed copy first, so callers don't
    /// have to special-case crunched/compressed or non-readable textures.
    /// </summary>
    public static class JestUtils
    {
        /// <summary>
        /// Encodes a sprite's source texture as a PNG base64 data URL
        /// ("data:image/png;base64,..."). Returns null when the sprite or its texture is
        /// null, or encoding fails. For an atlased sprite this encodes the whole atlas.
        /// </summary>
        public static string SpriteToDataUrl(Sprite sprite)
        {
            return sprite == null ? null : TextureToDataUrl(sprite.texture);
        }

        /// <summary>
        /// Encodes any texture (including GPU-compressed or non-readable) as a PNG base64
        /// data URL ("data:image/png;base64,..."). Returns null when the texture is null
        /// or encoding fails.
        /// </summary>
        public static string TextureToDataUrl(Texture source)
        {
            byte[] png = EncodeTextureToPng(source);
            if (png == null || png.Length == 0)
            {
                return null;
            }

            return "data:image/png;base64," + System.Convert.ToBase64String(png);
        }

        /// <summary>
        /// Renders any texture into a fresh uncompressed RGBA32 texture and PNG-encodes it.
        /// Works regardless of the source's compression or Read/Write import settings.
        /// Returns null when the source is null or encoding fails.
        /// </summary>
        public static byte[] EncodeTextureToPng(Texture source)
        {
            if (source == null)
            {
                return null;
            }

            RenderTexture rt = RenderTexture.GetTemporary(
                source.width, source.height, 0, RenderTextureFormat.ARGB32);
            RenderTexture previous = RenderTexture.active;
            Texture2D readable = null;
            try
            {
                // RenderTexture sampling origin differs by graphics API; flip vertically on
                // APIs whose UVs start at the bottom (OpenGL/WebGL) so the PNG is upright.
                if (SystemInfo.graphicsUVStartsAtTop)
                {
                    Graphics.Blit(source, rt);
                }
                else
                {
                    Graphics.Blit(source, rt, new Vector2(1f, -1f), new Vector2(0f, 1f));
                }

                RenderTexture.active = rt;
                readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
                readable.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
                readable.Apply();
                return readable.EncodeToPNG();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Jest] JestUtils.EncodeTextureToPng failed: {e}");
                return null;
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
                if (readable != null)
                {
                    Object.Destroy(readable);
                }
            }
        }
    }
}
