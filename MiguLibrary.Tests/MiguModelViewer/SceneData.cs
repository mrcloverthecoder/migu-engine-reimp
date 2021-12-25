using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sn = System.Numerics;
using MiguModelViewer.Renderer;
using OpenTK.Graphics.OpenGL;
using MiguLibrary.Objects;
using MiguLibrary.Scene;
using MiguLibrary.Motions;
using MiguLibrary;
using OpenTK;
using System.Diagnostics;

namespace MiguModelViewer
{
    public class SceneData
    {
        List<GLObjectData> Objects;
        List<Motion> Motions;

        Sn.Vector4 ClearColor;

        string Id = String.Empty;
        string RoomId = String.Empty;
        string SongName = String.Empty;

        private GLFont mFont;

        private bool mPlayingAnim = false;
        private int mCurrentFrame = 0;

        public SceneData(int id)
        {
            Objects = new List<GLObjectData>();
            Motions = new List<Motion>();

            ClearColor = new Sn.Vector4(1.0f);

            Load("S" + id.ToString("000"));
        }

        public void Load(string id)
        {
            Objects.Clear();
            Motions.Clear();

            BSX sceneInfo = BSX.FromFile($"{Config.DataPath}/SCENEDATA/{id}/{id}.BSX");

            BimStack table = BimStack.FromFile($"{Config.DataPath}/SCENEDATA/{id}/{id}_TABLE.MBITS");

            foreach(object value in table.Values)
            {
                if(value.GetType() == typeof(string) && Id == String.Empty)
                    Id = (string)value;
                else if (value.GetType() == typeof(string) && RoomId == String.Empty)
                    RoomId = (string)value;
                else if (value.GetType() == typeof(string) && SongName == String.Empty)
                    SongName = (string)value;
            }

            ClearColor = sceneInfo.BackgroundColor;

            foreach (FileEntry entry in sceneInfo.ObjectEntries)
            {
                Console.WriteLine($"{Config.DataPath}/{entry.Path}");
                Objects.Add(new GLObjectData(ObjectData.FromFile($"{Config.DataPath}/{entry.Path}"), new GLShader(File.ReadAllText("Resource/Shader/OBJ_01.vert.shp"), File.ReadAllText("Resource/Shader/OBJ_01.frag.shp")), Path.GetDirectoryName(entry.Path)));
            }

            foreach(FileEntry entry in sceneInfo.MotionEntries)
            {
                Console.WriteLine($"{Config.DataPath}/{entry.Path}");
                Motions.Add(Motion.FromFile($"{Config.DataPath}/{entry.Path}"));
            }

            mFont = new GLFont("Resource/Font/map_seurat_pro_b.xml");

            GL.ClearColor(ClearColor.X, ClearColor.Y, ClearColor.Z, ClearColor.W);

            /*
            string s = "";
            foreach (Motion mot in Motions)
            {
                s += mot.Name + '\n';
                int i = 0;
                foreach (MotionBone bone in mot.Bones)
                {
                    s += ($" {bone.Name} => {bone.Keyframes.Length} => {bone.Name == Objects[1].BoneInfluenceNames[i]} => {Objects[1].BoneInfluenceNames[i]}\n");
                    i++;
                }
            }

            File.WriteAllText("out.txt", s);*/
        }

        public void Update()
        {
            /*if(mPlayingAnim == false && mCurrentFrame == 0)
            {
                foreach(GLObjectData obj in Objects)
                {
                    for(int i = 0; i < 60; i++)
                        obj.Shader.Uniform($"uBoneTransforms[{i}]", OpenTK.Matrix4.Identity);
                }
            }*/

            /*foreach(Motion mot in Motions)
            {
                if (!mPlayingAnim)
                    break;

                foreach(GLObjectData obj in Objects)
                {
                    if(mot.Name == Id && obj.IsChara)
                    {
                        for (int i = 0; i < 60; i++)
                            obj.Shader.Uniform($"uBoneTransforms[{i}]", Matrix4.Identity);

                        if (mot.Bones.Count < 1)
                            continue;

                        // Assume every bone has the same number of keyframes
                        int keyframeCount = mot.Bones[0].Keyframes.Length;

                        if (keyframeCount < 1)
                            break;

                        int currentMotionFrame = mot.Bones[0].Keyframes[0].Frame;

                        if (mCurrentFrame >= keyframeCount)
                            break;

                        if (mCurrentFrame < currentMotionFrame)
                            break;

                        for (int i = 0; i < mot.Bones.Count)
                        {
                            int boneIndex = 0;
                            int motParentIndex = 0;
                            for (int j = 0; j < obj.Bones.Length; j++)
                            {
                                if (obj.Bones[j].Name == mot.Bones[i].Name)
                                {
                                    boneIndex = j;
                                }
                            }

                            Matrix4 localTrans = Matrix4.CreateTranslation(mot.Bones[i].Keyframes[mCurrentFrame].Position.ToGL());

                            if (obj.Bones[boneIndex].ParentId != -1)
                            {
                                localTrans *= Matrix4.CreateTranslation(mot.Bones)
                            }

                            //Matrix4 scale = Matrix4.CreateScale(1.0f);
                            //Matrix4 rotation = Matrix4.CreateFromQuaternion(new Quaternion(bone.Keyframes[mCurrentFrame].Rotation.X, bone.Keyframes[mCurrentFrame].Rotation.Y, bone.Keyframes[mCurrentFrame].Rotation.Z, bone.Keyframes[mCurrentFrame].Rotation.W));

                            

                            Matrix4 rotation = Matrix4.CreateRotationY(OpenTK.MathHelper.DegreesToRadians(0.0f));

                            //Console.WriteLine(rotation);=

                            //Console.WriteLine($"IDXB: {boneIndex}");
                            obj.Shader.Uniform($"uBoneTransforms[{boneIndex}]", translation);
                        }
                    }
                }
            }*/

            if (mPlayingAnim)
                mCurrentFrame++;
        }

        public void Render()
        {
            GL.Enable(EnableCap.DepthTest);

            for (int i = 0; i < Objects.Count; i++)
                Objects[i].Render();

            GL.Disable(EnableCap.DepthTest);

            mFont.RenderText(10.0f, 10.0f, Id);
            mFont.RenderText(10.0f, 25.0f, $"CURRENT ANIM FRAME: {mCurrentFrame}");
            mFont.RenderText(10.0f, 45.0f, $"CAM: {Camera.Position} <{Camera.Pitch} | {Camera.Yaw}>");
        }

        public void FlipPlayingState()
        {
            if (mPlayingAnim)
                mPlayingAnim = false;
            else
                mPlayingAnim = true;
        }

        public void Reset()
        {
            mCurrentFrame = 0;
        }


        public void Unload()
        {
            for (int i = 0; i < Objects.Count; i++)
                Objects[i].Dispose();

            Objects.Clear();
            Motions.Clear();
        }
    }
}
