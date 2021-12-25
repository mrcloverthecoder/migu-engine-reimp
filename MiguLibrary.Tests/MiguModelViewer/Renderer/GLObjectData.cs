using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiguLibrary.Objects;
using PuyoTools.Core.Textures.Gim;

namespace MiguModelViewer.Renderer
{
    public class GLObjectData : IDisposable
    {
        public GLVertexSet[] Sets;
        public GLShader Shader;

        public Bone[] Bones;
        public string[] BoneInfluenceNames;
        public bool IsChara;

        private bool mDisposed = false;

        public GLObjectData(ObjectData obj, GLShader shader, string path)
        {
            Sets = new GLVertexSet[obj.VertexSets.Count];
            GLMaterial[] materials = new GLMaterial[obj.Materials.Count];

            for (int i = 0; i < obj.Materials.Count; i++)
            {
                /*if (File.Exists($"tests/PSP/{obj.Materials[i].TextureName.Replace(".png", "out.png").ToLower()}"))
                {
                    materials[i] = new GLMaterial(obj.Materials[i], "tests/PSP/" + obj.Materials[i].TextureName.Replace(".png", "out.png").ToLower());
                    continue;
                }*/

                if (obj.Materials[i].HasDiffuseTexture)
                {
                    GimTextureDecoder decoder = new GimTextureDecoder($"{Config.DataPath}/" + $"{path}/PSP/{obj.Materials[i].TextureName.Replace(".png", ".gim")}".ToUpper());

                    byte[] data = decoder.GetPixelData();

                    // Build cache up 
                    if (!Cache.TextureAlpha.ContainsKey(obj.Materials[i].TextureName))
                    {
                        bool _break = false;
                        for(int y = 0; y < decoder.Height; y++)
                        {
                            for(int x = 0; x < (decoder.Width / 4); x++)
                            {
                                //Console.WriteLine((y * decoder.Width) + ((4 * (x + 1)) - 1));
                                if(data[(y * decoder.Width) + ((4 * (x + 1)) - 1)] < 0xFF)
                                {
                                    Cache.TextureAlpha[obj.Materials[i].TextureName] = true;
                                    _break = true;
                                    break;
                                }
                            }

                            if (_break)
                                break;

                            Cache.TextureAlpha[obj.Materials[i].TextureName] = false;
                        }
                    }

                    Cache.WriteTextureAlphaCache();

                    materials[i] = new GLMaterial(obj.Materials[i], data, decoder.Width, decoder.Height, decoder.PixelFormat);

                    materials[i].HasAlpha = Cache.TextureAlpha[obj.Materials[i].TextureName];

                    decoder = null;
                    data = null;
                }
                else
                {
                    materials[i] = new GLMaterial(obj.Materials[i]);
                }
            }

            Bones = obj.Bones.ToArray();
            //Bones.OrderBy(o => o.ParentId);

            BoneInfluenceNames = new string[obj.Bones.Count];

            for(int i = 0; i < obj.Bones.Count; i++)
            {
                BoneInfluenceNames[i] = obj.Bones[i].Name;
                Console.WriteLine($"BONE AAAA:C {BoneInfluenceNames[i]}");
            }

            for (int i = 0; i < obj.VertexSets.Count; i++)
                Sets[i] = new GLVertexSet(obj.VertexSets[i], obj.FaceSets[i], materials[obj.FaceSets[i].MaterialIndex]);

            // sorting
            Sets = Sets.OrderBy(set => set.Material.HasAlpha).ToArray();
            for(int i = 0; i < Sets.Length; i++)
            {
                Console.WriteLine($"Sets #{i}:");
                Console.WriteLine($"    HasAlpha: {Sets[i].Material.HasAlpha}");
            }

            Shader = shader;

            IsChara = obj.IsChara;

            // Unload material cache
            materials = null;
        }

        public void Render()
        {
            foreach(GLVertexSet set in Sets)
            {
                set.Render(Shader);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!mDisposed)
            {
                for (int i = 0; i < Sets.Length; i++)
                    Sets[i].Dispose();

                Shader.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
