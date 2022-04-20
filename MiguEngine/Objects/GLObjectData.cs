using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiguEngine.Extensions;
using MiguEngine.Skeletons;
using MiguEngine.Textures;
using MiguLibrary.Objects;
using OpenTK;
using BufferHint = OpenTK.Graphics.OpenGL4.BufferUsageHint;

namespace MiguEngine.Objects
{
    public class GLObjectData : IDisposable
    {
        public GLMesh[] Meshes;
        public Shader Shader;

        public GLMorph[] Morphs;
        public GLBone[] Bones;

        public string[] BoneInfluenceNames;
        public string[] MorphNames;

        public int PhysicsId;

        private bool mDisposed = false;

        // Invert the Z axis
        // The scale of 0.08 is applied directly to the verts when reading the model
        public Matrix4 Model = Matrix4.CreateScale(1.0f, 1.0f, -1.0f);

        public string Name;

        public GLObjectData(ObjectData obj, Shader shader)
        {
            Meshes = new GLMesh[obj.VertexSets.Count];
            GLMaterial[] materials = new GLMaterial[obj.Materials.Count];
            Dictionary<int, string> vertexIndices = new Dictionary<int, string>();

            PhysicsId = obj.PhysicsNumber;

            for (int i = 0; i < obj.Materials.Count; i++)
            {
                if (obj.Materials[i].HasDiffuseTexture)
                {
                    string texName = obj.Materials[i].TextureName;

                    // Build cache up 
                    /*if (!Cache.TextureAlpha.ContainsKey(obj.Materials[i].TextureName))
                    {
                        bool _break = false;
                        for(int y = 0; y < obj.Textures[texName].Height; y++)
                        {
                            for(int x = 0; x < (obj.Textures[texName].Width / 4); x++)
                            {
                                if (obj.Textures[texName].Data[(y * obj.Textures[texName].Width) + ((4 * (x + 1)) - 1)] < 0xFF)
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
                    }*/

                    materials[i] = new GLMaterial(obj.Materials[i], new Texture(obj.Textures[texName]));
                }
                else
                {
                    materials[i] = new GLMaterial(obj.Materials[i]);
                }
            }

            for (int i = 0; i < obj.VertexSets.Count; i++)
            {
                // Set the position buffer usage hint to DynamicDraw if the object has morphs so glBufferSubData doesn't kill the performance
                Meshes[i] = new GLMesh(obj.VertexSets[i], obj.FaceSets[i], materials[obj.FaceSets[i].MaterialIndex], obj.Morphs.Count > 0 ? BufferHint.DynamicDraw : BufferHint.StaticDraw);
            }

            // Build vertex id cache for the morphs
            for (int i = 0; i < Meshes.Length; i++)
            {
                for (int j = 0; j < Meshes[i].Ids.Length; j++)
                    vertexIndices[(int)Meshes[i].Ids[j]] = $"{i}|{j}";
            }

            Morphs = new GLMorph[obj.Morphs.Count];
            MorphNames = new string[obj.Morphs.Count];
            for(int i = 0; i < obj.Morphs.Count; i++)
            {
                GLMorph morph = new GLMorph();
                morph.Name = obj.Morphs[i].Name;
                morph.Morphs = new GLVertexMorph[obj.Morphs[i].Morphs.Length];

                MorphNames[i] = morph.Name;

                for(int j = 0; j < obj.Morphs[i].Morphs.Length; j++)
                {
                    int id = obj.Morphs[i].Morphs[j].Index;

                    GLVertexMorph vertexMorph = new GLVertexMorph();

                    // "base" morph works like a pivot for the other morphs
                    if(i > 0)
                    {
                        int baseMorphIndex = obj.Morphs[0].Morphs[id].Index;

                        vertexMorph.SetIndex = int.Parse(vertexIndices[baseMorphIndex].Split('|')[0]);
                        vertexMorph.VertexIndex = int.Parse(vertexIndices[baseMorphIndex].Split('|')[1]);
                        vertexMorph.Transform = obj.Morphs[i].Morphs[j].Transform.ToGL();
                    }
                    else
                    {
                        vertexMorph.SetIndex = 0;
                        vertexMorph.VertexIndex = 0;
                        vertexMorph.Transform = Vector3.Zero;
                    }

                    morph.Morphs[j] = vertexMorph;
                }

                Morphs[i] = morph;
            }

            Bones = new GLBone[obj.Bones.Count];
            int idx = 0;
            foreach(Bone bone in obj.Bones)
            {
                Matrix4 bp = bone.Pose.ToGL();
                Bones[idx] = new GLBone(bone.Name, bp, bone.Position.ToGL(), bone.ParentId);
                idx++;
            }

            BoneInfluenceNames = new string[obj.Bones.Count];

            for(int i = 0; i < obj.Bones.Count; i++)
            {
                BoneInfluenceNames[i] = obj.Bones[i].Name;
            }

            Shader = shader;

            // Unload material cache
            materials = null;
        }

        public void SetMorph(int index, float progress = 1.0f)
        {
            for(int i = 0; i < Morphs[index].Morphs.Length; i++)
            {
                Meshes[Morphs[index].Morphs[i].SetIndex].UpdateVertexPosition(Morphs[index].Morphs[i].VertexIndex, Meshes[Morphs[index].Morphs[i].SetIndex].Positions[Morphs[index].Morphs[i].VertexIndex].ToGL() + (Morphs[index].Morphs[i].Transform * progress));
            }
        }

        public void ResetModelMatrix()
        {
            Model = Matrix4.CreateScale(1.0f, 1.0f, -1.0f);
        }

        public void Draw(Camera cam)
        {
            Shader.Uniform("uModel", Model);

            Rendering.MeshRenderer.Add(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!mDisposed)
            {
                for (int i = 0; i < Meshes.Length; i++)
                    Meshes[i].Dispose();

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
