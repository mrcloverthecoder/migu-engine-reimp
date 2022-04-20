using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MiguEngine;
using MiguEngine.Textures;
using OpenTK.Graphics.OpenGL;

namespace MiguModelViewer.Renderer
{
    public class GLFont
    {
        public Texture Bitmap;
        public Dictionary<ushort, FontChar> Chars;
        public Shader Shader;

        private int mVertexBuffer, mVertexArray;

        public GLFont(string fontmapPath, string shaderPath = "Resource/Shader", string shaderName = "FONT")
        {
            // Load shader
            Shader = new Shader(File.ReadAllText($"{shaderPath}/{shaderName}.vert.shp"), File.ReadAllText($"{shaderPath}/{shaderName}.frag.shp"));

            // Load fontmap
            XmlDocument doc = new XmlDocument();
            doc.Load(fontmapPath);

            foreach(XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.Name == "pages")
                    Bitmap = new Texture(MiguLibrary.Textures.TextureResource.Load("Resource/Font/" + node.ChildNodes[0].Attributes["file"].Value));
                else if(node.Name == "chars")
                {
                    Chars = new Dictionary<ushort, FontChar>(int.Parse(node.Attributes["count"].Value));

                    foreach(XmlNode charNode in node.ChildNodes)
                    {
                        FontChar fontChar = new FontChar();
                        ushort id = ushort.Parse(charNode.Attributes["id"].Value);

                        fontChar.Id = id;
                        fontChar.Position = new Vector2(float.Parse(charNode.Attributes["x"].Value), float.Parse(charNode.Attributes["y"].Value));
                        fontChar.Size = new Vector2(float.Parse(charNode.Attributes["width"].Value), float.Parse(charNode.Attributes["height"].Value));
                        fontChar.HeightOffset = float.Parse(charNode.Attributes["yoffset"].Value);

                        Chars.Add(id, fontChar);
                    }
                }
            }

            // Set up the buffers
            mVertexArray = GL.GenVertexArray();
            GL.BindVertexArray(mVertexArray);

            mVertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBuffer);

            GL.BufferData(BufferTarget.ArrayBuffer, 6 * 4 * sizeof(float), new float[0], BufferUsageHint.DynamicDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            GL.BindVertexArray(0);
        }

        /*public void RenderText(float x, float y, string text, Color color, float scale = 1.0f, DrawAlignment alignment = DrawAlignment.TopLeft)
        {
            Shader.Use();

            // Updating uniforms
            Shader.Uniform("uProjection", Matrix4.CreateOrthographicOffCenter(0.0f, Config.Width, 0.0f, Config.Height, 1f, -1f));
            //Shader.Uniform("uProjection", Matrix4.CreateOrthographic(960.0f, 540.0f, 0.1f, 1000.0f));
            Shader.Uniform("uColor", color.AsVector4());

            // Bind vertex array before rendering
            GL.BindVertexArray(mVertexArray);

            // Bind texture
            Bitmap.Use();

            // Looping the string to render the chars
            foreach (char c in text)
            {
                // Getting the id
                ushort id = (ushort)c;

                // Creating the quads
                FontChar chr = Chars[id];

                float width = chr.Size.X * scale;
                float height = chr.Size.Y * scale;

                // Bottom left
                Vector2 TexCoord0 = Utils.Normalize(new Vector2(chr.Position.X, chr.Position.Y + chr.Size.Y), new Vector2(Bitmap.Width, Bitmap.Height));
                // Top right
                Vector2 TexCoord1 = Utils.Normalize(new Vector2(chr.Position.X + chr.Size.X, chr.Position.Y), new Vector2(Bitmap.Width, Bitmap.Height));
                // Size
                Vector2 Size = Utils.Normalize(chr.Size.ToGL(), new Vector2(Bitmap.Width, Bitmap.Height));

                float yoffset = 0;

                if (alignment == DrawAlignment.TopLeft)
                {
                    yoffset += Config.Height - height;
                    yoffset -= y * 2;
                }
                else if(alignment == DrawAlignment.BottomLeft)
                {

                }

                float[] vertices = new float[24]
                {
                    x,         y + height + yoffset, TexCoord0.X, TexCoord1.Y,
                    x,         y + yoffset,          TexCoord0.X, TexCoord0.Y,
                    x + width, y + yoffset,          TexCoord1.X, TexCoord0.Y,

                    x,         y + height + yoffset, TexCoord0.X, TexCoord1.Y,
                    x + width, y + yoffset,          TexCoord1.X, TexCoord0.Y,
                    x + width, y + height + yoffset, TexCoord1.X, TexCoord1.Y
                };

                GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBuffer);
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Length * 4, vertices);

                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

                x += width;
            }
        }

        public void RenderText(float x, float y, string text) => RenderText(x, y, text, new Color(255, 255, 255, 255));*/
    }

    public class FontChar
    {
        public ushort Id;
        public Vector2 Position;
        public Vector2 Size;

        public float HeightOffset;

        public override string ToString()
        {
            return $"{Id} / <({Position.X}, {Position.Y}), ({Size.X}, {Size.Y})>";
        }
    }
}
