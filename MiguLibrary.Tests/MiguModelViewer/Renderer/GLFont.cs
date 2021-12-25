using System;
using System.IO;
using Sn = System.Numerics;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiguLibrary;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace MiguModelViewer.Renderer
{
    public class GLFont
    {
        public GLTexture Bitmap;
        public Dictionary<ushort, FontChar> Chars;
        public GLShader Shader;

        private int VertexBuffer, VertexArray;

        public GLFont(string fontmapPath, string shaderPath = "Resource/Shader", string shaderName = "FONT")
        {
            // Load shader
            Shader = new GLShader(File.ReadAllText($"{shaderPath}/{shaderName}.vert.shp"), File.ReadAllText($"{shaderPath}/{shaderName}.frag.shp"));

            // Load fontmap
            XmlDocument doc = new XmlDocument();
            doc.Load(fontmapPath);

            foreach(XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.Name == "pages")
                    Bitmap = new GLTexture("Resource/Font/" + node.ChildNodes[0].Attributes["file"].Value);
                else if(node.Name == "chars")
                {
                    Chars = new Dictionary<ushort, FontChar>(int.Parse(node.Attributes["count"].Value));

                    foreach(XmlNode charNode in node.ChildNodes)
                    {
                        //Console.WriteLine(charNode.Name);
                        FontChar fontChar = new FontChar();
                        ushort id = ushort.Parse(charNode.Attributes["id"].Value);

                        fontChar.Id = id;
                        fontChar.Position = new Sn.Vector2(float.Parse(charNode.Attributes["x"].Value), float.Parse(charNode.Attributes["y"].Value));
                        fontChar.Size = new Sn.Vector2(float.Parse(charNode.Attributes["width"].Value), float.Parse(charNode.Attributes["height"].Value));
                        fontChar.HeightOffset = float.Parse(charNode.Attributes["yoffset"].Value);

                        Chars.Add(id, fontChar);
                    }
                }
            }

            // Set up the buffers
            VertexArray = GL.GenVertexArray();
            GL.BindVertexArray(VertexArray);

            VertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);

            GL.BufferData(BufferTarget.ArrayBuffer, 6 * 4 * sizeof(float), new float[0], BufferUsageHint.DynamicDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            GL.BindVertexArray(0);
        }

        public void RenderText(float x, float y, string text, Color color, float scale = 1.0f, DrawAlignment alignment = DrawAlignment.TopLeft)
        {
            Shader.Use();

            // Updating uniforms
            Shader.Uniform("uProjection", Matrix4.CreateOrthographicOffCenter(0.0f, Config.Width, 0.0f, Config.Height, 1f, -1f));
            //Shader.Uniform("uProjection", Matrix4.CreateOrthographic(960.0f, 540.0f, 0.1f, 1000.0f));
            Shader.Uniform("uColor", color.AsVector4());

            // Bind vertex array before rendering
            GL.BindVertexArray(VertexArray);

            // Bind texture
            Bitmap.Use();

            // Looping the string to render the chars
            foreach (char c in text)
            {
                // Getting the id
                // I thought this was gonna be extensively slow but turns out... it runs great!
                byte[] charBytes = Encoding.UTF8.GetBytes(c.ToString());
                if (charBytes.Length < 2)
                    charBytes = new byte[] { charBytes[0], 0x00 };


                /*Console.Write($"Char bytes: ");
                for (int i = 0; i < charBytes.Length; i++)
                    Console.Write($"{charBytes[i]} ");
                Console.WriteLine($"Char: {c}");*/

                ushort id = BitConverter.ToUInt16(charBytes, 0);

                // Creating the quads
                FontChar chr = Chars[id];

                float width = chr.Size.X * scale;
                float height = chr.Size.Y * scale;

                // Bottom left
                Sn.Vector2 TexCoord0 = MiguLibrary.MathHelper.Normalize(new Sn.Vector2(chr.Position.X, chr.Position.Y + chr.Size.Y), new Sn.Vector2(Bitmap.Width, Bitmap.Height));
                // Top right
                Sn.Vector2 TexCoord1 = MiguLibrary.MathHelper.Normalize(new Sn.Vector2(chr.Position.X + chr.Size.X, chr.Position.Y), new Sn.Vector2(Bitmap.Width, Bitmap.Height));
                // Size
                Sn.Vector2 Size = MiguLibrary.MathHelper.Normalize(chr.Size, new Sn.Vector2(Bitmap.Width, Bitmap.Height));

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

                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Length * 4, vertices);

                //GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

                //Console.WriteLine(Chars[id]);

                //Console.WriteLine(width);

                x += width;
            }
        }

        public void RenderText(float x, float y, string text) => RenderText(x, y, text, new Color(255, 255, 255, 255));
    }

    public class FontChar
    {
        public ushort Id;
        public Sn.Vector2 Position;
        public Sn.Vector2 Size;

        public float HeightOffset;

        public override string ToString()
        {
            return $"{Id} / <({Position.X}, {Position.Y}), ({Size.X}, {Size.Y})>";
        }
    }
}
