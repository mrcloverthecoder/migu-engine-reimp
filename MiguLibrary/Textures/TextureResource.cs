using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuyoTools.Core.Textures.Gim;

namespace MiguLibrary.Textures
{
    public class TextureResource
    {
        public int Width;
        public int Height;
        public byte[] Data;
        public TextureFormat Format;

        public static TextureResource Load(string path) => Load(path, true);

        public static TextureResource Load(string path, bool decode)
        {
            TextureResource tex = new TextureResource();

            if (!File.Exists(path))
            {
                // Write a blank image to data
                tex.Width = 16;
                tex.Height = 16;
                tex.Data = new byte[tex.Width * tex.Height];
                for (int i = 1; i <= tex.Width; i++)
                {
                    for (int j = 1; j <= tex.Height; j++)
                    {
                        tex.Data[(i * j) - 1] = 0xFF;
                    }
                }

                return tex;
            }

            tex.Load(File.Open(path, FileMode.Open), decode);
            return tex;
        }

        private void Load(Stream stream, bool decode)
        {
            GimTextureDecoder decoder = new GimTextureDecoder(stream);

            Data = decode ? decoder.GetPixelData() : decoder.GetRawPixelData();
            Format = decode ? TextureFormat.Argb8888 : (TextureFormat)(int)decoder.PixelFormat;
            Width = decoder.Width;
            Height = decoder.Height;

            stream.Close();
        }
    }
}
