using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using MiguLibrary.Objects;
using MiguEngine.Textures;
using MiguEngine.Extensions;

namespace MiguEngine.Objects
{
    public class GLMaterial
    {
        public Texture DiffuseTexture;
        public Vector4 DiffuseColor;
        public Vector4 AmbientColor;
        public Vector4 SpecularColor;

        public bool IsAlpha;

        public GLMaterial(Material mat, Texture diffuseTexture)
        {
            Init(mat);
            DiffuseTexture = diffuseTexture;

            IsAlpha = IsAlpha || diffuseTexture.IsAlpha;
        }

        public GLMaterial(Material mat)
        {
            Init(mat);
        }

        private void Init(Material mat)
        {
            DiffuseColor = mat.DiffuseColor.AsVector4().ToGL();
            AmbientColor = mat.AmbientColor.AsVector4().ToGL();
            SpecularColor = mat.SpecularColor.AsVector4().ToGL();

            IsAlpha = DiffuseColor.W < 1.0f;
        }

        public void Use(Shader shader)
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
