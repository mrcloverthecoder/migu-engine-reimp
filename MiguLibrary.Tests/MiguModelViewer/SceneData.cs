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
using OpenTK.Input;
using System.Diagnostics;
using MiguLibrary.IO;

namespace MiguModelViewer
{
    public class SceneData
    {
        public List<GLObjectData> Objects;
        public List<Motion> Motions;

        public bool IsPlaying = false;

        Sn.Vector4 ClearColor;

        string Id = String.Empty;
        string RoomId = String.Empty;
        string SongName = String.Empty;

        private BSX mSceneInfo;

        private GLFont mFont;

        private int mCurrentFrame = 0;

        private KeyboardState mLastState;
        private Stopwatch mTime;

        private string[] mCharaMots;

        public bool ApplyInverseBindPose;

        public bool EnableParentedTransform;
        public int SelectedBoneIndex;

        public float SelectedBoneRotationPitch;
        public float SelectedBoneRotation;
        public float SelectedBoneRotationRoll;

        public string SelectedBoneMatrixDisp = "";

        public SceneData(int id)
        {
            Objects = new List<GLObjectData>();
            Motions = new List<Motion>();

            ClearColor = new Sn.Vector4(1.0f);

            mLastState = new KeyboardState();
            mTime = new Stopwatch();

            Load("S" + id.ToString("000"));
        }

        public void Load(string id)
        {
            Objects.Clear();
            Motions.Clear();

            mCharaMots = new string[2];

            mSceneInfo = BSX.FromFile($"{Config.DataPath}/SCENEDATA/{id}/{id}.BSX");

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

            // Main body motion
            mCharaMots[0] = Id;

            ClearColor = mSceneInfo.BackgroundColor;

            foreach (FileEntry entry in mSceneInfo.ObjectEntries)
            {
                Console.WriteLine($"{Config.DataPath}/{entry.Path}");
                ObjectData obj = ObjectData.FromFile($"{Config.DataPath}/{entry.Path}", entry.Path);

                // Physics motion
                if(obj.PhysicsNumber != 0)
                    mCharaMots[1] = Id + $"_P{obj.PhysicsNumber}";
                Console.WriteLine($"PHSYICS MOT: {mCharaMots[1]}");

                Objects.Add(new GLObjectData(obj, new GLShader(File.ReadAllText("Resource/Shader/OBJ_01.vert.shp"), File.ReadAllText("Resource/Shader/OBJ_01.frag.shp")), Path.GetDirectoryName(entry.Path)));
            }

            foreach(FileEntry entry in mSceneInfo.MotionEntries)
            {
                Console.WriteLine($"{Config.DataPath}/{entry.Path}");
                Motions.Add(Motion.FromFile($"{Config.DataPath}/{entry.Path}"));

                if (entry.Path == "MOTIONS/S012_P1.BMM")
                {
                    Console.WriteLine($"KEYFRAME 001: {Motions.Last().Bones[2].Keyframes[1].Rotation}");
                }

                /*foreach(Keyframe key in Motions.Last().Bones[1].Keyframes)
                {
                    if (entry.Path == "MOTIONS/S012_P1.BMM")
                        break;

                    Console.WriteLine($"{entry.Path} FRAME");
                    Console.WriteLine($"    {key.Rotation}");
                }*/
            }


            mFont = new GLFont("Resource/Font/map_seurat_pro_b.xml");

            GL.ClearColor(ClearColor.X, ClearColor.Y, ClearColor.Z, ClearColor.W);

            string st1 = "";
            foreach(FileEntry entry in mSceneInfo.ObjectEntries)
            {
                st1 += $"{entry.Path}\n";
                st1 += $"\tCuts\n";

                int i = 0;
                foreach(SceneAnimationCut cut in entry.Cuts)
                {
                    st1 += $"\t\tCut #{i} ({cut.StartFrame}~{cut.Length})\n";
                    st1 += $"\t\t\tTranslation Points:\n";
                    foreach(TranslationPoint point in cut.TranslationPoints)
                    {
                        st1 += $"\t\t\t\t{point.StartPosition} {point.MidwayPosition} {point.EndPosition} ({point.FadeIn} | {point.FadeOut})\n";
                    }
                    st1 += $"\t\t\tRotation Points:\n";
                    foreach(RotationPoint point in cut.RotationPoints)
                    {
                        st1 += $"\t\t\t\t{point.Rotation}\n";
                    }
                    i++;
                }
            }

            File.WriteAllText("test2.txt", st1);

            // 16mb alloc for motion data dump
            char[] m = new char[16384 * 1024]; 

            string s = "";
            foreach (GLObjectData obj in Objects)
            {
                if (!obj.IsChara)
                    continue;

                int i = 0;
                foreach (GLBone bone in obj.Bones)
                {
                    s += $"Bone {bone.Name} ";
                    if(bone.ParentId != -1)
                    {
                        s += $"(Child of {obj.Bones[bone.ParentId].Name})";
                    }
                    s += "\n";
                    s += $"  {bone.InverseBindPose}\n";
                    i++;
                }
            }

            foreach(Motion mot in Motions)
            {
                if (mot.Name != Id)
                    continue;

                foreach(MotionBone bone in mot.Bones)
                {
                    s += $"Motion bone {bone.Name} {bone.Type} {bone.Keyframes[0].Position}\n";
                }
            }

            /*int position = 0;
            foreach(Motion mot in Motions)
            {
                if (mot.Name != Id)
                    break;

                position = m.PushString(mot.Name + $" ({mot.Bones[0].Keyframes.Length} keyframes)" + '\n', position);

                foreach(MotionBone bone in mot.Bones)
                {
                    position = m.PushString($"\t{bone.Name}.{bone.Type}\n", position);

                    Console.WriteLine(mot.Name);
                    Console.WriteLine(bone.Keyframes.Length);
                    for(int i = 0; i < bone.Keyframes.Length; i++)
                    {
                        position = m.PushString($"\t\t{bone.Keyframes[i].Frame} | {bone.Keyframes[i].Position} {bone.Keyframes[i].Rotation}\n", position);
                    }
                }
            }

            using(EndianBinaryWriter writer = new EndianBinaryWriter(File.Open($"cache\\dmp\\mot\\{Id}.txt", FileMode.Create), Endianness.LittleEndian))
            {
                writer.Write(m);
            }

            m = null;*/

            File.WriteAllText("out1.txt", s);
        }

        public void Update()
        {
            KeyboardState state = Keyboard.GetState();

            if(state.IsKeyDown(Key.F3) && !mLastState.IsKeyDown(Key.F3))
            {
                Console.Write("index: ");
                int index = int.Parse(Console.ReadLine());

                Console.Write("pos: ");
                string line = Console.ReadLine();
                Vector3 pos = new Vector3(float.Parse(line.Split(',')[0]), float.Parse(line.Split(',')[1]), float.Parse(line.Split(',')[2]));

                foreach (GLObjectData obj in Objects)
                {
                    obj.Shader.Uniform($"uBoneTransforms[{index}]", Matrix4.CreateTranslation(pos.X, pos.Y, pos.Z));
                }
            }

            foreach(FileEntry entry in mSceneInfo.ObjectEntries)
            {
                if (!IsPlaying)
                    break;

                foreach(GLObjectData obj in Objects)
                {
                    if (entry.Path != obj.Name)
                        continue;

                    foreach(SceneAnimationCut cut in entry.Cuts)
                    {
                        if ((int)Math.Floor(mCurrentFrame * (60.0f / Config.Framerate)) > cut.StartFrame)
                        {
                            Matrix4 translation = Matrix4.Identity;
                            // For now it just expects that the cut has a single translation point
                            foreach(TranslationPoint point in cut.TranslationPoints)
                            {
                                // Compose model matrix translation
                                translation = Matrix4.CreateTranslation(point.EndPosition.ToGL());
                            }
                            Matrix4 rotation = Matrix4.Identity;
                            foreach (RotationPoint point in cut.RotationPoints)
                                if(point.Rotation != Sn.Quaternion.Identity)
                                    rotation = Matrix4.CreateFromQuaternion(point.Rotation.ToGL());

                            obj.Model *= rotation * translation;
                        }
                    }
                }
            }

            /*if(!IsPlaying && ApplyInverseBindPose)
            {
                Matrix4[] transforms = new Matrix4[60];

                int i = 0;
                foreach(GLBone bone in Objects[1].Bones)
                {
                    //Console.WriteLine("=--=");
                    transforms[i] = bone.InverseBindPose;
                    //Console.WriteLine(Transforms[i]);
                    //Console.WriteLine("\n+--+\n");
                    //Console.WriteLine(Matrix4.CreateTranslation(10f, 5f, -20f));
                    //Console.WriteLine();
                    i++;
                }

                Objects[1].Shader.Uniform("uBoneTransforms", transforms);
            }
            else if(!IsPlaying && !ApplyInverseBindPose)
            {
                Matrix4[] transforms = new Matrix4[60];

                for (int i = 0; i < 60; i++)
                    transforms[i] = Matrix4.Identity;

                Objects[1].Shader.Uniform("uBoneTransforms", transforms);
            }*/

            /*
            if(!IsPlaying)
            {
                Matrix4[] transforms = new Matrix4[60];
                for (int i = 0; i < transforms.Length; i++)
                    transforms[i] = Matrix4.Identity;

                Matrix4 rotation = Matrix4.CreateRotationX(OpenTK.MathHelper.DegreesToRadians(SelectedBoneRotationPitch)) * Matrix4.CreateRotationY(OpenTK.MathHelper.DegreesToRadians(SelectedBoneRotation)) * Matrix4.CreateRotationZ(OpenTK.MathHelper.DegreesToRadians(SelectedBoneRotationRoll));

                SelectedBoneMatrixDisp = Objects[1].Bones[SelectedBoneIndex].InverseBindPose.ToString();

                if (EnableParentedTransform)
                {
                    for(int i = 0; i < Objects[1].Bones.Length; i++)
                    {
                        if (Objects[1].Bones[i].ParentId != -1)
                        {
                            Matrix4 childRotation = Matrix4.Identity;
                        }
                    }
                }

                transforms[SelectedBoneIndex] = rotation * Objects[1].Bones[SelectedBoneIndex].BindPose;

                Objects[1].Shader.Uniform("uBoneTransforms", transforms);
            }*/

            foreach (GLObjectData obj in Objects)
            {
                if (!IsPlaying)
                    break;

                Matrix4[] transforms = new Matrix4[60];
                for (int i = 0; i < transforms.Length; i++)
                    transforms[i] = Matrix4.Zero;

                foreach (Motion mot in Motions)
                {
                    // Break if the motion is a character motion and the object is not a character
                    if (mCharaMots.Contains(mot.Name) && !obj.IsChara)
                        break;

                    // its guarranteed that a matrix array with the same length as the body motion is returned
                    int i = 0;
                    foreach(Matrix4 transform in AnimationPlayer.GetTransforms(obj.Bones, mot.Bones, obj.BoneInfluenceNames, (int)Math.Floor(mCurrentFrame * (30.0f / Config.Framerate)), transforms))
                    {
                        if (transforms[i] == Matrix4.Zero && transform != Matrix4.Zero)
                            transforms[i] = transform;
                        i++;
                    }

                    obj.Shader.Uniform("uBoneTransforms", transforms);
                }
            }

            mLastState = state;

            if (IsPlaying)
                mCurrentFrame++;
        }

        public void Render(float delta)
        {

            GL.Enable(EnableCap.DepthTest);

            for (int i = 0; i < Objects.Count; i++)
                Objects[i].Render();

            GL.Disable(EnableCap.DepthTest);

            /*for(int i = 0; i < Motions.Count; i++)
            {
                if(Motions[i].Name == Id)
                {
                    for(int j = 0; j < Motions[i].Bones.Count; j++)
                    {
                        mFont.RenderText(10.0f, 15.0f * j, $"{j} :: {MiguLibrary.MathHelper.FromQ2(Motions[i].Bones[j].Keyframes[mCurrentFrame].Rotation)}");
                    }
                }
            }*/

            //mFont.RenderText(10.0f, 10.0f, Id);
            //mFont.RenderText(10.0f, 25.0f, $"CURRENT ANIM FRAME: {mCurrentFrame}");
            //mFont.RenderText(10.0f, 45.0f, $"CAM: {Camera.Position} <{Camera.Pitch} | {Camera.Yaw}>");
        }

        public void FlipPlayingState()
        {
            if (IsPlaying)
            {
                IsPlaying = false;
                mTime.Stop();
            }
            else
            {
                IsPlaying = true;
                mTime.Start();
            }
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
