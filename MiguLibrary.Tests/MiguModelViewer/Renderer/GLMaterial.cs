using System;
using Sn = System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using MiguLibrary.Objects;
using PuyoTools.Core.Textures.Gim;

namespace MiguModelViewer.Renderer
{
    public class GLMaterial
    {
        public GLTexture DiffuseTexture;
        public Sn.Vector4 DiffuseColor;
        public Sn.Vector4 AmbientColor;
        public Sn.Vector4 SpecularColor;

        public bool HasAlpha;

        public GLMaterial(Material mat, byte[] textureData, int width, int height, GimPixelFormat fmt)
        {
            DiffuseTexture = new GLTexture(width, height, fmt, textureData);
            DiffuseColor = mat.DiffuseColor.AsVector4();
            AmbientColor = mat.AmbientColor.AsVector4();
            SpecularColor = mat.SpecularColor.AsVector4();

            HasAlpha = false;
        }

        public GLMaterial(Material mat)
        {
            DiffuseColor = mat.DiffuseColor.AsVector4();
            AmbientColor = mat.AmbientColor.AsVector4();
            SpecularColor = mat.SpecularColor.AsVector4();

            HasAlpha = DiffuseColor.W < 1.0f;
        }

        public GLMaterial(Material mat, string path)
        {
            DiffuseTexture = new GLTexture(path);
            DiffuseColor = mat.DiffuseColor.AsVector4();
            AmbientColor = mat.AmbientColor.AsVector4();
            SpecularColor = mat.SpecularColor.AsVector4();

            HasAlpha = DiffuseColor.W < 1.0f;
        }

        public void Use(GLShader shader)
        {
            shader.Use();

            DiffuseTexture?.Use();

            shader.Uniform("uMaterial.DiffuseTexture", 0);
            shader.Uniform("uMaterial.Diffuse", DiffuseColor);
            shader.Uniform("uMaterial.Ambient", AmbientColor);
            shader.Uniform("uMaterial.Specular", SpecularColor);
        }
    }
}
