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
using MiguLibrary.Data;
using MiguLibrary.Motions;
using MiguLibrary;
using OpenTK;
using OpenTK.Input;
using System.Diagnostics;
using MiguLibrary.IO;
using MiguModelViewer.Structs;
using MiguModelViewer.Animations;
using MiguEngine;
using MiguEngine.Extensions;
using NAudio;
using NAudio.Wave;
using BEPUphysics;

namespace MiguModelViewer
{
    public class ScenePlayer
    {
        public bool IsPlaying = false;

        public string Id = String.Empty;
        public string RoomId = String.Empty;
        public string SongName = String.Empty;

        public Sn.Vector4 ClearColor;
        public int EndFrame;

        public GLSceneData SceneInfo;
        public Motion CameraMotion;

        public int CurrentFrame = 0;
        public int CurrentCamFrame = 0;

        public bool EnableMotionLerp = true;

        // Scene settings
        public LightSetting[] LightSettings;

        // ---------------------------------------
        public bool DebugRendering = false;

        public int FragmentMode = 0;

        public Camera Camera;
        public MiguEngine.Input.InputManager InputManager;

        public const float FREE_CAM_SPEED = 0.7f;
        public const float FREE_CAM_SCROLL_SPEED = 0.05f;
        public const float FREE_CAM_SENSITIVITY = 0.3f;

        public List<MotionPlayer> MotionPlayers;

        // Audio
        System.Threading.Thread AudioThread;

        /*public Vector3[] SkelPoints;
        public int SkelPointsBuffer;
        public int SkelPointsIdBuffer;
        public int SkelPointerVtxArr;
        public Shader SkelPointsShader;
        public Matrix4[] Transforms;*/

        public ScenePlayer(int id)
        {
            LightSettings = new LightSetting[5];

            // Set default values
            for (int i = 0; i < LightSettings.Length; i++)
                LightSettings[i] = new LightSetting(Sn.Vector3.One, 1.0f, i);

            ClearColor = new Sn.Vector4(1.0f);
            Camera = new Camera();
            Camera.Position = new Vector3(0.0f, 1.0f, 5.0f);
            Camera.FieldOfViewAngle = 65.0f;
            InputManager = new MiguEngine.Input.InputManager();
            MotionPlayers = new List<MotionPlayer>();

            Load("S" + id.ToString("000"));
        }

        public void Load(string id)
        {
            // Parse scene data
            B3DScene sceneInfo = B3DScene.FromFile($"{Config.DataPath}/SCENEDATA/{id}/{id}.BSX");

            //CameraMotion = Motion.FromFile($"{Config.DataPath}/MOTIONS/CAMERA/S029.BMM");

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

            string lightPath = $"Resource/Ext/SceneParameter/{Id}_light.txt";

            if (File.Exists(lightPath))
                LightSettings = SpecialStructReader.ReadLightSetting(lightPath);

            ClearColor = sceneInfo.BackgroundColor;
            EndFrame = Config.Framerate > 60.0f ? (int)(sceneInfo.EndFrame * Config.Delta) : Config.Framerate < 60.0f ? (int)(sceneInfo.EndFrame * Config.InverseDelta) : sceneInfo.EndFrame;

            SceneInfo = new GLSceneData(sceneInfo);
            foreach(SceneObject o in SceneInfo.Objects)
            {
                foreach(MotionEntry m in o.Motions)
                {
                    MotionPlayers.Add(new MotionPlayer(m.Motion, o.Object.Bones, CurrentFrame));
                }
            }

            /*float[] skelPointsId = new float[0];
            foreach(var obj in SceneInfo.Objects)
            {
                Console.WriteLine(obj.Name);
                if(obj.Name.EndsWith("OBJ_MIG.BMD"))
                {
                    SkelPoints = new Vector3[obj.Object.Bones.Length];
                    skelPointsId = new float[obj.Object.Bones.Length];
                    for (int i = 0; i < obj.Object.Bones.Length; i++)
                    {
                        var bone = obj.Object.Bones[i];

                        SkelPoints[i] = bone.Position;
                        skelPointsId[i] = (float)i;
                    }
                }
            }

            SkelPointerVtxArr = GL.GenVertexArray();
            GL.BindVertexArray(SkelPointerVtxArr);

            SkelPointsBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, SkelPointsBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, SkelPoints.Length * 3 * sizeof(float), SkelPoints, BufferUsageHint.DynamicDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            SkelPointsIdBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, SkelPointsIdBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float), skelPointsId, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 1, VertexAttribPointerType.Float, false, sizeof(float), 0);

            SkelPointsShader = new Shader(File.ReadAllText("Resource/Shader/SKEL.vert.shp"), File.ReadAllText("Resource/Shader/SKEL.frag.shp"));*/

            AudioThread = new System.Threading.Thread(new System.Threading.ThreadStart(Audio));
            AudioThread.Start();
        }

        public void Audio()
        {
            // Load audio
            var audioFile = new AudioFileReader("Resource/Data/Sound/Song/S029.wav");
            audioFile.CurrentTime = new TimeSpan(0, 0, 0, 0, 0);

            // Create a new output device
            var device = new WaveOutEvent();
            device.Init(audioFile);

            while (true)
            {
                if (IsPlaying)
                    device.Play();
                else
                    device.Pause();

                if (device.PlaybackState == PlaybackState.Stopped && CurrentFrame != 0)
                    break;

                System.Threading.Thread.Sleep(17);
            }

            // Dispose audio device and audio stream
            device.Dispose();
            audioFile.Dispose();
        }

        public void PlayAudio()
        {
            using(var audioFile = new AudioFileReader("Resource/Data/Sound/Song/S029.wav"))
            using(var device = new WaveOutEvent())
            {
                audioFile.CurrentTime = new TimeSpan(0, 0, 0, 1, 500);
                device.Init(audioFile);
                device.Play();
                while(device.PlaybackState == PlaybackState.Playing)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        public void Update()
        {
            GL.ClearColor(ClearColor.X, ClearColor.Y, ClearColor.Z, ClearColor.W);

            InputManager.Update();

            Camera.Yaw += InputManager.GetCursorPositionDelta().X * FREE_CAM_SENSITIVITY;
            Camera.Pitch -= InputManager.GetCursorPositionDelta().Y * FREE_CAM_SENSITIVITY;
            Camera.FieldOfViewAngle -= InputManager.GetScrollWheelDelta() * FREE_CAM_SCROLL_SPEED;

            if (InputManager.IsKeyDown(Key.W))
                Camera.Position += Camera.Front * 0.05f * FREE_CAM_SPEED;
            if (InputManager.IsKeyDown(Key.S))
                Camera.Position -= Camera.Front * 0.05f * FREE_CAM_SPEED;
            if (InputManager.IsKeyDown(Key.A))
                Camera.Position -= Camera.Right * 0.05f * FREE_CAM_SPEED;
            if (InputManager.IsKeyDown(Key.D))
                Camera.Position += Camera.Right * 0.05f * FREE_CAM_SPEED;
            if (InputManager.IsKeyDown(Key.Space))
                Camera.Position += Vector3.UnitY * 0.05f * FREE_CAM_SPEED;
            if (InputManager.IsKeyDown(Key.ShiftLeft) && !InputManager.IsKeyDown(Key.WinLeft))
                Camera.Position += -Vector3.UnitY * 0.05f * FREE_CAM_SPEED;

            foreach (SceneObject obj in SceneInfo.Objects)
            {
                // Proccess lighting
                foreach(LightSetting set in LightSettings)
                {
                    bool lightEnabled = false;

                    if (set.Type == LightType.All)
                        lightEnabled = true;
                    // Hacky way to check if object is chara
                    else if (set.Type == LightType.Chara && obj.Object.Morphs.Length > 5)
                        lightEnabled = true;
                    else if (set.Type == LightType.Prop && obj.Object.Morphs.Length < 5)
                        lightEnabled = true;

                    if (lightEnabled)
                    {
                        obj.Object.Shader.Uniform("uLightColor", set.Color);
                        obj.Object.Shader.Uniform("uLightIntensity", set.Intensity);
                    }
                }

                /*foreach (SceneAnimationCut cut in obj.Cuts)
                {
                    if(CurrentFrame > cut.StartFrame + cut.Length)
                        continue;

                    obj.Object.ResetModelMatrix();
                    Matrix4 translation = Matrix4.Identity;
                    // For now it just expects that the cut has a single translation point
                    foreach (TranslationPoint point in cut.TranslationPoints)
                    {
                        // Compose model matrix translation
                        translation *= Matrix4.CreateTranslation(point.EndPoint.ToGL() + point.StartPoint.ToGL());
                    }

                    obj.Object.Model *= translation;
                }*/

                Matrix4[] transforms = new Matrix4[65];
                for (int i = 0; i < transforms.Length; i++)
                    transforms[i] = Matrix4.Zero;

                int motEntryIdx = 0;
                foreach (MotionEntry motEntry in obj.Motions)
                {
                    if (motEntryIdx == 1)
                        continue;

                    foreach(MotionMorph morph in motEntry.Motion.Morphs)
                    {
                        int morphIndex = Array.IndexOf(obj.Object.MorphNames, morph.Name);

                        if (morphIndex == -1)
                            continue;

                        int frameIndex = motEntry.CurrentMorphIndices[morphIndex];

                        if(frameIndex < morph.Keyframes.Length)
                        {
                            int frame = (int)Math.Floor(morph.Keyframes[frameIndex].Frame * Config.Delta);

                            if (frame > CurrentFrame)
                                continue;

                            
                            float progress = morph.Keyframes[frameIndex].Progress;

                            obj.Object.SetMorph(morphIndex, progress);
                            motEntry.CurrentMorphIndices[morphIndex]++;
                        }
                    }

                    Matrix4[] trans;

                    // With lerp
                    if (EnableMotionLerp)
                        trans = MotionPlayer.GetTransformsLerp(obj.Object.Bones, ref obj.Motions[motEntryIdx], obj.Object.BoneInfluenceNames, CurrentFrame, transforms, true);
                    else // Without lerp
                        trans = MotionPlayer.GetTransforms(obj.Object.Bones, motEntry.Motion.Bones, obj.Object.BoneInfluenceNames, (int)Math.Floor(CurrentFrame * (30.0f / Config.Framerate)), transforms);

                    for (int j = 0; j < trans.Length; j++)
                    {
                        if (transforms[j] == Matrix4.Zero && trans[j] != Matrix4.Zero)
                            transforms[j] = trans[j];
                    }

                    obj.Object.Shader.Uniform("uBoneTransforms", transforms);
                    //Transforms = transforms;

                    motEntryIdx++;
                }
            }

            /*if (CurrentCamFrame < CameraMotion.Bones[0].Keyframes.Length)
            {
                // *CAMERA*
                Keyframe k = CameraMotion.Bones[0].Keyframes[CurrentCamFrame];
                Vector3 rot = new Quaternion(k.Rotation.X, k.Rotation.Y, k.Rotation.Z, k.Rotation.W).ToEulerAngles();
                float fov = OpenTK.MathHelper.DegreesToRadians(k.Position.W);

                Camera.Position = new Vector3(k.Position.X, k.Position.Y, -k.Position.Z);
                //Camera.FieldOfViewAngle = k.Position.W;
                Camera.Pitch = rot.Y;
                Camera.Yaw = -OpenTK.MathHelper.RadiansToDegrees(rot.X);
                Camera.Roll = rot.Y;

                Console.WriteLine($"{OpenTK.MathHelper.RadiansToDegrees(rot.X)} {OpenTK.MathHelper.RadiansToDegrees(rot.Y)} {OpenTK.MathHelper.RadiansToDegrees(rot.Z)}");
            }*/

            if (IsPlaying && CurrentFrame < EndFrame)
            {
                CurrentFrame++;
                CurrentCamFrame = (int)Math.Floor(CurrentFrame / 2.0);
            }

            if(CurrentFrame >= EndFrame)
            {
                SwitchState();
            }
        }

        public void Render(float delta)
        {
            GL.Enable(EnableCap.DepthTest);

            for (int i = 0; i < SceneInfo.Objects.Length; i++)
            {
                if(DebugRendering)
                    SceneInfo.Objects[i].Object.Shader.Uniform("uDebugMode", FragmentMode);
                /*for(int j = 0; j < SceneInfo.Objects[i].Object.Meshes.Length; j++)
                {
                    SceneInfo.Objects[i].Object.Shader.Uniform("uModel", SceneInfo.Objects[i].Object.Model);
                    if (!SceneInfo.Objects[i].Object.Meshes[j].Material.IsAlpha)
                        SceneInfo.Objects[i].Object.Meshes[j].Draw(SceneInfo.Objects[i].Object.Shader, Camera);
                }*/
                SceneInfo.Objects[i].Object.Shader.Uniform("uModel", SceneInfo.Objects[i].Object.Model);
                MiguEngine.Rendering.MeshRenderer.Add(SceneInfo.Objects[i].Object);
            }

            /*for (int i = 0; i < SceneInfo.Objects.Length; i++)
            {
                MiguEngine.Rendering.MeshRenderer.Add(SceneInfo.Objects[i].Object);
            }*/

            MiguEngine.Rendering.MeshRenderer.Draw(Camera);



            /*SkelPointsShader.Use();
            SkelPointsShader.Uniform("uView", Camera.GetViewMatrix());
            SkelPointsShader.Uniform("uProjection", Camera.GetProjectionMatrix());
            if(Transforms != null)
                SkelPointsShader.Uniform("uTransforms", Transforms);
            GL.PointSize(8.0f);
            GL.BindVertexArray(SkelPointerVtxArr);
            GL.DrawArrays(PrimitiveType.Points, 0, SkelPoints.Length);*/

            GL.Disable(EnableCap.DepthTest);
        }

        public void SwitchState()
        {
            if (IsPlaying)
            {
                IsPlaying = false;
            }
            else
            {
                IsPlaying = true;
            }
        }

        public void Reset()
        {
            CurrentFrame = 0;
        }


        public void Unload()
        {
            for (int i = 0; i < SceneInfo.Objects.Length; i++)
                SceneInfo.Objects[i].Object.Dispose();
        }
    }
}
