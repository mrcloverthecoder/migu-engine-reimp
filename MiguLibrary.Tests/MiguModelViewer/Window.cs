using System;
using Sn = System.Numerics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using MiguModelViewer.Renderer;
using OpenTK.Platform.Windows;
using MiguLibrary.IO;
using MiguLibrary.Objects;
using MiguLibrary.Motions;
using MiguModelViewer.UI;
using ImGuiNET;

namespace MiguModelViewer
{
    class Window : GameWindow
    {
        private static DebugProc _debugProcCallback = Callback.DebugCallback;
        private static GCHandle _debugProcCallbackHandle = GCHandle.Alloc(_debugProcCallback);

        GLFont mFont;

        Vector2 mLastCursorPos = new Vector2(0.0f, 0.0f);

        bool isPressed = false;

        KeyboardState lastState = new KeyboardState();

        Scene mScenePlayer;

        private ImGuiController mController;

        private int mOldSelectedIndex = -1;
        private int mSelectedIndex = 0;

        private int mPreviouslyChosenTest = -1;
        private int mCurrentChosenTest = 0;

        private int mMotionOldSelectedIndex = -1;
        private int mMotionSelectedIndex = 11;

        private Tests mCurrentTest;

        private Dictionary<string, string> mLoadableObjects;
        private Dictionary<string, string> mLoadableMotions;

        private string[] mBoneNames = new string[60]
        {
            "Center", "UpperBody", "UpperBody2", "Neck", "Head", "RightEye", "LeftEye", "RightHair", "LeftHair", "LeftShoulder",
            "LeftArm", "LeftElbow", "LeftHandTwist", "LeftWrist", "LeftThumb0", "LeftThumb1", "LeftIndex1", "LeftIndex2", "LeftMiddle1", "LeftMiddle2",
            "LeftLittle1", "LeftLittle2", "LeftRing1", "LeftRing2", "RightShoulder", "RightArm", "RightElbow", "RightHandTwist", "RightWrist", "RightThumb0",
            "RightThumb1", "RightIndex1", "RightIndex2", "RightMiddle1", "RightMiddle2", "RightLittle1",
            "RightLittle2", "RightRing1", "RightRing2", "LeftChest", "RightChest", "LowerBody", "RightFoot", "RightKnee", "RightAnkle", "RightAnkle2",
            "LeftFoot", "LeftKnee", "LeftAnkle", "LeftAnkle2", "SKIRTm_0_0", "SKIRTm_0_1", "SKIRTm_0_2", "SKIRTm_0_3", "SKIRTm_0_4", "SKIRTm_1_0", "SKIRTm_1_1", "SKIRTm_1_2", "SKIRTm_1_3", "SKIRTm_1_4"
        };

        public Window(int width, int height, string title) : base(width, height, GraphicsMode.Default, title, GameWindowFlags.Default, DisplayDevice.Default, 3, 3, GraphicsContextFlags.Default)
        {
            Title = $"{title} | OpenGL {GL.GetString(StringName.Version)} - {GL.GetString(StringName.Vendor)}";
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.DebugMessageCallback(_debugProcCallback, IntPtr.Zero);
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);

            mController = new ImGuiController((int)Config.Width, (int)Config.Height);

            mLoadableObjects = new Dictionary<string, string>();
            foreach(string dir in Directory.GetDirectories(Config.DataPath + "/OBJECTS"))
            {
                foreach (string file in Directory.GetFiles(dir))
                {
                    string[] split = dir.Replace("\\", "/").Split('/');

                    string name = split[split.Length - 1];
                    string filename = file;

                    if (!file.ToUpper().EndsWith("BMD"))
                        continue;

                    if (mLoadableObjects.ContainsKey(name))
                        name = name + "/" + Path.GetFileName(filename);

                    mLoadableObjects[name] = filename;
                    Console.WriteLine($"AAA: {name} {filename}");
                }
            }

            mLoadableMotions = new Dictionary<string, string>();
            foreach (string file in Directory.GetFiles(Config.DataPath + "/MOTIONS"))
            {
                string name = Path.GetFileName(file).Replace(".BMM", "").ToUpper();
                string filename = file;

                if (!file.ToUpper().EndsWith("BMM"))
                    continue;

                mLoadableMotions[name] = filename;
                Console.WriteLine($"MMM: {name} {filename}");
            }


            //mController.AddFont("Resource/Font/NotoSansJP-Regular.otf", 20.0f);

            //mController.RecreateFontDeviceTexture();

            /*Console.WriteLine(Matrix4.CreateOrthographic(960.0f, 540.0f, 0.01f, 100.0f));
            Console.WriteLine();
            Console.WriteLine(Matrix4.CreateOrthographicOffCenter(0.0f, 960.0f, 0.0f, 540.0f, 1f, -1f));*/
            GL.Viewport(0, 0, Width, Height);
            //GL.ClearColor(0.392f, 0.584f, 0.929f, 1.0f);
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);

            mScenePlayer = new Scene(State.SceneId);

            mFont = new GLFont("Resource/Font/map_seurat_pro_b.xml");

            /*mCharaShader = new GLShader(File.ReadAllText("Resource/Shader/OBJ_01.vert.shp"), File.ReadAllText("Resource/Shader/OBJ_01.frag.shp"));

            mStageShader = new GLShader(File.ReadAllText("Resource/Shader/OBJ_01.vert.shp"), File.ReadAllText("Resource/Shader/OBJ_01.frag.shp"));
            
            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open("tests/OBJ_MIG.BMD", FileMode.Open), Encoding.GetEncoding(932), Endianness.LittleEndian))
            {
                ObjectData obj = new ObjectData();
                obj.Read(reader);

                mSet = new GLObjectData(obj);

                //Console.WriteLine($"Bone count: {obj.Bones.Count}");
                /*foreach (Bone bone in obj.Bones)
                    Console.WriteLine($"{bone.Pose}");*/
            /*sets = new List<GLVertexSet>();
            for(int i = 0; i < obj.VertexSets.Count; i++)
            {
                sets.Add(new GLVertexSet(obj.VertexSets[i], obj.FaceSets[i]));
            }*/

            /*Console.WriteLine($"Set #0 -> {obj.VertexSets[0].Positions.Length}");
            for(int j = 0; j < obj.VertexSets[0].Positions.Length; j++)
                Console.WriteLine($"    {obj.VertexSets[0].Positions[j]}");
            Console.WriteLine($"Set #1");
            for (int j = 0; j < obj.FaceSets[0].FaceIndices.Length; j++)
                Console.WriteLine($"    {obj.FaceSets[0].FaceIndices[j]}");
        }

        using(EndianBinaryReader reader = new EndianBinaryReader(File.Open("tests/OBJBG_STG.BMD", FileMode.Open), Encoding.GetEncoding(932), Endianness.LittleEndian))
        {
            ObjectData obj = new ObjectData();
            obj.Read(reader);

            mSet1 = new GLObjectData(obj);

            foreach (Bone bone in obj.Bones)
                Console.WriteLine($"{bone.Name} {bone.ParentId} {bone.ChildId}");
        }

        using (EndianBinaryReader reader = new EndianBinaryReader(File.Open("tests/A03.BMM", FileMode.Open), Encoding.GetEncoding(932), Endianness.LittleEndian))
        {
            mMot = new Motion();
            mMot.Read(reader);

            foreach (MotionBone bone in mMot.Bones)
            {
                Console.WriteLine($"{bone.Name} {bone.Index} {bone.Type}");
                foreach (Keyframe key in bone.Keyframes)
                {
                    Console.WriteLine($"{key.Frame} {key.Rotation}");
                }
            }
        }*/

            Camera.FieldOfViewAngle = 45.0f;

            base.OnLoad(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            mController.Update(this, (float)e.Time);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            GL.Enable(EnableCap.DepthTest);

            //GL.Enable(EnableCap.CullFace);
            //GL.FrontFace(FrontFaceDirection.Cw);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            mScenePlayer.Render((float)e.Time);
            //mCurrentTest?.Render();

            GL.Disable(EnableCap.DepthTest);

            /*mFont.RenderText(-480.0f, 250.0f, "Megupoido ja myujikku.... C Shappu!!", MiguLibrary.Color.White);
            mFont.RenderText(-480.0f, 230.0f, $"FPS: {this.RenderFrequency.ToString("0")}", MiguLibrary.Color.White, 1.0f);
            mFont.RenderText(-480.0f, 200.0f, $"Frame: {mCurrentFrame}");
            mFont.RenderText(-480.0f, 180.0f, $"Camera: {Camera.Position} | {Camera.Yaw}");
            mFont.RenderText(-480.0f, 165.0f, $"Anim is playing: {mAnimIsPlaying}");
            if(mAnimIsPlaying)
                mFont.RenderText(-480.0f, 140.0f, $"Anim frame: {mCurrentFrame - mAnimFrameStart}");*/

            /*mFont.RenderText(10.0f, 30.0f, "< ");
            mFont.RenderText(25.0f, 30.0f, State.SceneId.ToString());
            mFont.RenderText(50.0f, 30.0f, ">");
            mFont.RenderText(10.0f, 50.0f, "OK");*/

            if (!ImGui.Begin("Debugging site"))
            {
                ImGui.End();
            }

            ImGui.Combo("", ref mCurrentChosenTest, new string[] { "Object Test", "Motion Test" }, 2);

            if (mCurrentChosenTest == 0 || mCurrentChosenTest == 1)
            {
                ImGui.NewLine();

                ImGui.Combo("Object", ref mSelectedIndex, mLoadableObjects.Keys.ToArray(), mLoadableObjects.Count);
            }

            if (mCurrentChosenTest == 1)
            {
                ImGui.NewLine();
                ImGui.Combo("Motion", ref mMotionSelectedIndex, mLoadableMotions.Keys.ToArray(), mLoadableMotions.Count);
                ImGui.SliderInt("Motion Frame", ref mCurrentTest.CurrentFrame, 0, mCurrentTest.Get(GetAction.MotionLastFrame));

                if(ImGui.Button("Play / Pause"))
                {
                    mCurrentTest.Switch();
                }
            }

            /*ImGui.Checkbox("Apply inverse bind pose", ref mScenePlayer.ApplyInverseBindPose);
            ImGui.Checkbox("Apply child transforms", ref mScenePlayer.EnableParentedTransform);

            ImGui.Combo("Bone", ref mScenePlayer.SelectedBoneIndex, mBoneNames, mBoneNames.Length);

            ImGui.SliderFloat("Pitch", ref mScenePlayer.SelectedBoneRotationPitch, -179.0f, 179.0f);
            ImGui.SliderFloat("Yaw", ref mScenePlayer.SelectedBoneRotation, -179.0f, 179.0f);
            ImGui.SliderFloat("Roll", ref mScenePlayer.SelectedBoneRotationRoll, -179.0f, 179.0f);

            ImGui.Text(mScenePlayer.SelectedBoneMatrixDisp);*/

            /*
            if(ImGui.TreeNode("Objects"))
            {
                for(int i = 0; i < mScenePlayer.Objects.Count; i++)
                {
                    if(ImGui.TreeNode($"Object {i} (IsChara: {mScenePlayer.Objects[i].IsChara})"))
                    {
                        if(ImGui.TreeNode($"Bones"))
                        {
                            for (int j = 0; j < mScenePlayer.Objects[i].Bones.Length; j++)
                            {
                                if(ImGui.TreeNode($"Bone {j} ({mScenePlayer.Objects[i].Bones[j].Name})"))
                                {
                                    ImGui.Text($"Position: {mScenePlayer.Objects[i].Bones[j].Position}");
                                    ImGui.Text($"Rotation: {mScenePlayer.Objects[i].Bones[j].Rotation}");
                                    ImGui.TreePop();
                                }
                            }

                            ImGui.TreePop();
                        }
                        
                        ImGui.TreePop();
                    }
                }

                ImGui.TreePop();
            }*/

            mController.Render();

            Context.SwapBuffers();
            base.OnRenderFrame(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (!Focused) return;

            if(mCurrentChosenTest != mPreviouslyChosenTest)
            {
                mCurrentTest?.Dispose();

                if (mCurrentChosenTest == 0)
                    mCurrentTest = new ObjectTest();
                else if (mCurrentChosenTest == 1)
                    mCurrentTest = new MotionTest();

                // To force it to automatically reload the object
                mOldSelectedIndex = -1;

                mPreviouslyChosenTest = mCurrentChosenTest;
            }

            if(mSelectedIndex != mOldSelectedIndex)
            {
                if(mCurrentChosenTest == 0 || mCurrentChosenTest == 1)
                    // a little trick
                    mCurrentTest.Reload(mLoadableObjects[ mLoadableObjects.Keys.ToArray()[mSelectedIndex] ]);

                mOldSelectedIndex = mSelectedIndex;
            }

            if(mCurrentChosenTest == 1)
            {
                if(mMotionSelectedIndex != mMotionOldSelectedIndex)
                {
                    mCurrentTest.Reload(mLoadableMotions[mLoadableMotions.Keys.ToArray()[mMotionSelectedIndex]], ReloadType.Motion);

                    mMotionOldSelectedIndex = mMotionSelectedIndex;
                }
            }



            mCurrentTest.Update();

            /*

            mCurrentFrame += 1;

            if(mAnimIsPlaying)
            {
                if (mAnimFrameStart == -1)
                    mAnimFrameStart = mCurrentFrame;

                int currentFrame = mCurrentFrame - mAnimFrameStart;

                Matrix4[] Transforms = new Matrix4[mMot.Bones.Count];
                int boneIdx = 0;
                foreach(MotionBone bone in mMot.Bones)
                {
                    for(int i = 0; i < bone.Keyframes.Length; i++)
                    {
                        if (bone.Keyframes[i].Frame > currentFrame)
                            continue;
                        else
                        {
                            Transforms[boneIdx] = Matrix4.CreateFromQuaternion(new Quaternion(bone.Keyframes[i].Rotation.X, bone.Keyframes[i].Rotation.Y, bone.Keyframes[i].Rotation.Z, bone.Keyframes[i].Rotation.W));
                        }
                    }

                    boneIdx++;
                }

                mStageShader.Uniform("uBoneTransforms", Transforms);
            }

            */



            KeyboardState state = Keyboard.GetState();

            float speed = 1.0f;

            if (state.IsKeyDown(Key.LControl))
                speed = 0.3f;

            if (state.IsKeyDown(Key.W) || state.IsKeyDown(Key.Up))
                Camera.Position += new Vector3(0.0f, 0.0f, -0.1f) * speed;
            if (state.IsKeyDown(Key.A) || state.IsKeyDown(Key.Left))
                Camera.Position -= Vector3.Normalize(Vector3.Cross(new Vector3(0.0f, 0.0f, -0.1f), new Vector3(0.0f, 0.1f, 0.0f))) * 0.05f * speed;
            if (state.IsKeyDown(Key.S) || state.IsKeyDown(Key.Down))
                Camera.Position += new Vector3(0.0f, 0.0f, 0.1f) * speed;
            if (state.IsKeyDown(Key.D) || state.IsKeyDown(Key.Right))
                Camera.Position += Vector3.Normalize(Vector3.Cross(new Vector3(0.0f, 0.0f, -0.1f), new Vector3(0.0f, 0.1f, 0.0f))) * 0.05f * speed;
            if (state.IsKeyDown(Key.Space))
                Camera.Position += new Vector3(0.0f, 0.1f, 0.0f) * speed;
            if (state.IsKeyDown(Key.LShift))
            {
                if (!state.IsKeyDown(Key.WinLeft))
                    Camera.Position += new Vector3(0.0f, -0.1f, 0.0f) * speed;
            }
            if(state.IsKeyDown(Key.Period) && !lastState.IsKeyDown(Key.Period))
            {
                Console.Clear();
                Console.WriteLine("Pick an id (1~30): ");
                int id = MathHelper.Clamp(int.Parse(Console.ReadLine()), 1, 30);

                mScenePlayer.Unload();
                mScenePlayer = new Scene(id);
            }
            if (state.IsKeyDown(Key.Comma) && !isPressed)
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            }
            if(state.IsKeyDown(Key.J))
            {
                Camera.Yaw += 1.0f;
            }
            if (state.IsKeyDown(Key.L))
            {
                Camera.Yaw -= 1.0f;
            }
            if(state.IsKeyDown(Key.F1) && !lastState.IsKeyDown(Key.F1))
            {
                /*if (Camera.Mode == CameraMode.FollowTarget)
                    Camera.Mode = CameraMode.LockAtTarget;
                else
                    Camera.Mode = CameraMode.FollowTarget;*/

                mScenePlayer.FlipPlayingState();
            }
            if(state.IsKeyDown(Key.F2) && !lastState.IsKeyDown(Key.F2))
            {
                mScenePlayer.Reset();
            }

            // FOV Controller
            if (state.IsKeyDown(Key.I))
                if (Camera.FieldOfViewAngle > 1.0f)
                    Camera.FieldOfViewAngle -= 0.5f;
            if (state.IsKeyDown(Key.K))
                if (Camera.FieldOfViewAngle < 90.0f)
                    Camera.FieldOfViewAngle += 0.5f;

            mScenePlayer.Update();

            lastState = state;

            base.OnUpdateFrame(e);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            MouseState state = Mouse.GetState();

            if (!state.IsButtonDown(MouseButton.Right))
                return;

            if (mLastCursorPos == Vector2.Zero)
                mLastCursorPos = new Vector2(state.X, state.Y);
            else
            {
                Vector2 delta = new Vector2(state.X, state.Y) - mLastCursorPos;

                Camera.Yaw += delta.X * Camera.Sensitivity;
                Camera.Pitch += delta.Y * Camera.Sensitivity;
            }

            base.OnMouseMove(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            Camera.AspectRatio = (float)Width / (float)Height;
            Config.Width = (float)Width;
            Config.Height = (float)Height;
            base.OnResize(e);
        }
    }
}
