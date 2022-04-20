using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using MiguLibrary.Textures;

namespace MiguEngine.Textures
{
    public class Texture : IDisposable
    {
        public int Id;
        public float Width, Height;
        public bool IsAlpha;

        private bool mDisposed = false;

        public Texture(TextureResource tex)
        {
            Init(TextureTarget.Texture2D, tex.Width, tex.Height);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, tex.Width, tex.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, tex.Data);

            IsAlpha = ScanForAlpha(tex);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public Texture(int width, int height)
        {
            Init(TextureTarget.Texture2D, width, height);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public Texture(int width, int height, IntPtr pixels)
        {
            Init(TextureTarget.Texture2D, width, height);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Init(TextureTarget target, int width, int height)
        {
            IsAlpha = false;
            Width = width;
            Height = height;

            Id = GL.GenTexture();

            GL.BindTexture(target, Id);

            GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        }

        public void Use()
        {
            Use(TextureUnit.Texture0);
        }

        public void Use(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Id);
        }

        private bool ScanForAlpha(TextureResource tex)
        {
            if (tex.Format != TextureFormat.Argb8888)
                return false;

            bool isAlpha = false;

            for(int i = 0; i < tex.Data.Length; i += 4)
            {
                if(tex.Data[i + 3] < 255) // Check if alpha channel isn't opaque
                {
                    isAlpha = true; // If so, then the texture is transparent
                    break; // and finally early-out
                }
            }

            return isAlpha;
        }

        protected void Dispose(bool disposing)
        {
            if (!mDisposed)
                GL.DeleteTexture(Id);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
