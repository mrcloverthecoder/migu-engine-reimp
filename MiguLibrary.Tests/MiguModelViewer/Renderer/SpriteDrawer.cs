using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using MiguEngine;
using MiguEngine.Textures;

namespace MiguModelViewer.Renderer
{
    public class SpriteDrawer
    {
        private static Shader mShader;

        private static int mVertexArray;
        private static int mVertexBuffer;

        public static void Init()
        {
            // Load shader
            mShader = new Shader(File.ReadAllText("Resource/Shader/2D.vert.shp"), File.ReadAllText("Resource/Shader/2D.frag.shp"));

            // Set up the buffers
            mVertexArray = GL.GenVertexArray();
            GL.BindVertexArray(mVertexArray);

            mVertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBuffer);

            GL.BufferData(BufferTarget.ArrayBuffer, 6 * 4 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

            GL.BindVertexArray(0);
        }

        public static void Draw(Texture texture, float x, float y, Vector2 spritePosition, Vector2 size, float fragScaleX = 1.0f, float scale = 1.0f, float alpha = 1.0f, float uTranslation = 0.0f, float vTranslation = 0.0f, bool forceNormalUv = false, bool autoScale = false)
        {
            mShader.Use();

            mShader.Uniform("uProjection", Matrix4.CreateOrthographicOffCenter(0.0f, Config.Width, Config.Height, 0.0f, -1.0f, 1.0f));

            if (autoScale)
                mShader.Uniform("uModel", Matrix4.CreateScale(Config.ScaleX, Config.ScaleY, 1.0f));

            mShader.Uniform("uFragmentModel", Matrix4.CreateScale(fragScaleX, 1.0f, 1.0f));

            mShader.Uniform("uTexCoordTranslation", new Vector2(uTranslation, vTranslation));

            mShader.Uniform("uScale", scale);

            mShader.Uniform("uTexture", 0);

            mShader.Uniform("uAlphaMultiplier", alpha);

            // Calculate properties
            Vector2 texCoord0 = Utils.Normalize(spritePosition, new Vector2(texture.Width, texture.Height));

            Vector2 texCoord1 = Utils.Normalize(spritePosition + size, new Vector2(texture.Width, texture.Height));

            x = (float)Math.Ceiling(x);
            y = (float)Math.Ceiling(y);

            // Build vertex array
            float[] vertices = new float[24]
            {
                x         , y + size.Y, texCoord0.X, texCoord1.Y,
                x         , y         , texCoord0.X, texCoord0.Y,
                x + size.X, y         , texCoord1.X, texCoord0.Y,

                x         , y + size.Y, texCoord0.X, texCoord1.Y,
                x + size.X, y         , texCoord1.X, texCoord0.Y,
                x + size.X, y + size.Y, texCoord1.X, texCoord1.Y
            };

            GL.BindVertexArray(mVertexArray);

            // Update buffers
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBuffer);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Length * 4, vertices);

            // Bind the texture
            texture.Use();

            // And finally render
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }
    }
}
