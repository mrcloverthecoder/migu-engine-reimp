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
    public class Scene
    {
        public bool IsPlaying = false;

        Sn.Vector4 ClearColor;

        string Id = String.Empty;
        string RoomId = String.Empty;
        string SongName = String.Empty;

        private GLSceneData mSceneInfo;

        private int mCurrentFrame = 0;

        private KeyboardState mLastState;
        private Stopwatch mTime;

        private Dictionary<string, int> mPlayCounts;

        public bool ApplyInverseBindPose;

        public bool EnableParentedTransform;
        public int SelectedBoneIndex;

        public float SelectedBoneRotationPitch;
        public float SelectedBoneRotation;
        public float SelectedBoneRotationRoll;

        public string SelectedBoneMatrixDisp = "";

        public Scene(int id)
        {
            mPlayCounts = new Dictionary<string, int>();

            ClearColor = new Sn.Vector4(1.0f);

            mLastState = new KeyboardState();
            mTime = new Stopwatch();

            Load("S" + id.ToString("000"));
        }

        public void Load(string id)
        {
            // Parse scene data
            B3DScene sceneInfo = B3DScene.FromFile($"{Config.DataPath}/SCENEDATA/{id}/{id}.BSX");

            // Parse scene table
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

            /*int physicsNumber = 0;

            foreach (FileEntry entry in mSceneInfo.ObjectEntries)
            {
                Console.WriteLine($"{Config.DataPath}/{entry.Path}");
                ObjectData obj = ObjectData.FromFile($"{Config.DataPath}/{entry.Path}", entry.Path);

                // Physics motion
                if (obj.PhysicsNumber != 0)
                {
                    mCharaMots[1] = Id + $"_P{obj.PhysicsNumber}";
                    physicsNumber = obj.PhysicsNumber;
                }

                Console.WriteLine($"PHSYICS MOT: {mCharaMots[1]}");

                Objects.Add(new GLObjectData(obj, new GLShader(File.ReadAllText("Resource/Shader/OBJ_01.vert.shp"), File.ReadAllText("Resource/Shader/OBJ_01.frag.shp")), Path.GetDirectoryName(entry.Path)));
            }



            foreach(FileEntry entry in mSceneInfo.MotionEntries)
            {
                Console.WriteLine($"{Config.DataPath}/{entry.Path}");
                if (!entry.Path.Contains("_P"))
                    Motions.Add(Motion.FromFile($"{Config.DataPath}/{entry.Path}"));
            }

            Motions.Add(Motion.FromFile($"{Config.DataPath}/MOTIONS/{Id}_P{physicsNumber}.BMM"));*/

            mSceneInfo = new GLSceneData(sceneInfo);

            GL.ClearColor(ClearColor.X, ClearColor.Y, ClearColor.Z, ClearColor.W);

            string st1 = "";
            foreach(ObjectEntry entry in sceneInfo.Entries)
            {
                st1 += $"{entry.FileEntry.Path}\n";
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
            /*
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

            Console.WriteLine($"MOT PHYS NAMES AA: {mCharaMots[0]} {mCharaMots[1]}");

            File.WriteAllText("out1.txt", s);*/
        }

        public void Update()
        {
            KeyboardState state = Keyboard.GetState();

            foreach(SceneObject obj in mSceneInfo.Objects)
            {
                if (!IsPlaying)
                    break;

                foreach(SceneAnimationCut cut in obj.Cuts)
                {
                    if ((int)Math.Floor(mCurrentFrame * (60.0f / Config.Framerate)) > cut.StartFrame)
                    {
                        Matrix4 translation = Matrix4.Invert(Matrix4.CreateTranslation(obj.Object.Model.Row3.Xyz));
                        // For now it just expects that the cut has a single translation point
                        foreach(TranslationPoint point in cut.TranslationPoints)
                        {
                            // Compose model matrix translation
                            translation *= Matrix4.CreateTranslation(point.EndPosition.ToGL());
                        }
                        /*
                        Matrix4 rotation = Matrix4.Identity;
                        foreach (RotationPoint point in cut.RotationPoints)
                            if(point.Rotation != Sn.Quaternion.Identity)
                                rotation = Matrix4.CreateFromQuaternion(point.Rotation.ToGL());*/

                        obj.Object.Model *= translation;
                    }
                }
            }

            foreach (SceneObject obj in mSceneInfo.Objects)
            {
                if (!IsPlaying)
                    break;

                Matrix4[] transforms = new Matrix4[65];
                for (int i = 0; i < transforms.Length; i++)
                    transforms[i] = Matrix4.Zero;

                foreach (MotionEntry motEntry in obj.Motions)
                {
                    /*
                    int frameCount = (int)Math.Floor(motEntry.Motion.Bones[0].Keyframes.Length * (30.0f / Config.Framerate));

                    // Add the motion entry to mPlayCounts if it isn't there yet
                    if (!mPlayCounts.Keys.Contains(motEntry.Motion.Name))
                        mPlayCounts[motEntry.Motion.Name] = 0;

                    int currentLocalBaseFrame = frameCount * mPlayCounts[motEntry.Motion.Name];

                    if (mCurrentFrame > frameCount * (mPlayCounts[motEntry.Motion.Name] + 1) && mPlayCounts[motEntry.Motion.Name] <= motEntry.FrameSetting.PlayCount)
                        mPlayCounts[motEntry.Motion.Name]++;*/


                    // It's guarranteed that a matrix array with the same length as the body motion is returned
                    int i = 0;
                    foreach(Matrix4 transform in AnimationPlayer.GetTransforms(obj.Object.Bones, motEntry.Motion.Bones, obj.Object.BoneInfluenceNames, (int)Math.Floor((mCurrentFrame) * (30.0f / Config.Framerate)), transforms))
                    {
                        if (transforms[i] == Matrix4.Zero && transform != Matrix4.Zero)
                            transforms[i] = transform;
                        i++;
                    }

                    obj.Object.Shader.Uniform("uBoneTransforms", transforms);
                }
            }

            mLastState = state;

            if (IsPlaying)
                mCurrentFrame++;
        }

        public void Render(float delta)
        {

            GL.Enable(EnableCap.DepthTest);

            for (int i = 0; i < mSceneInfo.Objects.Length; i++)
                mSceneInfo.Objects[i].Object.Render();

            GL.Disable(EnableCap.DepthTest);
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
            for (int i = 0; i < mSceneInfo.Objects.Length; i++)
                mSceneInfo.Objects[i].Object.Dispose();
        }
    }
}
