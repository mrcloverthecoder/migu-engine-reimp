using System;
using Sn = System.Numerics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
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
using MiguEngine.Textures;
using MiguLibrary.Textures;
using MiguLibrary.Sprites;
using MiguModelViewer.Menus;
using MiguModelViewer.Animations;
using MiguEngine;

namespace MiguModelViewer
{
    class Window : GameWindow
    {
        private static DebugProc _debugProcCallback = Callback.DebugCallback;
        private static GCHandle _debugProcCallbackHandle = GCHandle.Alloc(_debugProcCallback);

        KeyboardState lastState = new KeyboardState();

        private ScenePlayer mScenePlayer;

        private ImGuiController mController;

        private Texture mCharaDiagMdlTex;
        private int mImSelectedCostume = 0;
        private int mImSubSelectedCostume = 0;
        private int mImOldSelectedCostume = -1;
        private bool mReloadPhysics = true;

        private ImFontPtr mFontPtr;

        private bool mDebugOverlayEnabled = false;

        private Texture mLoadingTex;
        private Texture Kira1, Kira2;

        private Stopwatch mTime;

        private int mSelectedLightConfig = 0;
        private int mSelectedLightConfigType = 3;

        private MiguModelViewer.Menus.Common.Menu mCurMenu;

        CameraAnimation AnimationBuilder;

        Camera Camera;

        public Window(int width, int height, string title) : base(width, height, GraphicsMode.Default, title, GameWindowFlags.Default, DisplayDevice.Default, 3, 3, GraphicsContextFlags.Default)
        {
            Title = $"{title} | OpenGL {GL.GetString(StringName.Version)} - {GL.GetString(StringName.Vendor)}";
            Camera = new Camera();
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.DebugMessageCallback(_debugProcCallback, IntPtr.Zero);
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);

            mTime = new Stopwatch();

            SpriteDrawer.Init();

            AnimationBuilder = new CameraAnimation();

            SpriteSet set = SpriteSet.FromFile($"{Config.DataPath}/SPRITE/LOADING/LOADING.BSD");

            mLoadingTex = new Texture(TextureResource.Load($"{Config.DataPath}/SPRITE/LOADING/PSP/LD000.GIM"));

            SpriteDrawer.Draw(mLoadingTex, 0, 0, new Vector2(0, 0), new Vector2(Config.Width, Config.Height));
            Context.SwapBuffers();

            Kira1 = new Texture(TextureResource.Load($"{Config.DataPath}/UI/0000_CIRCULARLIST/PSP/HART_1.GIM"));
            Kira2 = new Texture(TextureResource.Load($"{Config.DataPath}/UI/0000_CIRCULARLIST/PSP/HART_2.GIM"));

            mController = new ImGuiController((int)Config.Width, (int)Config.Height);

            ImGuiIOPtr io = ImGui.GetIO();
            mFontPtr = io.Fonts.AddFontFromFileTTF("Resource/Font/NotoSansJP-Medium.otf", 17.0f, null, io.Fonts.GetGlyphRangesChineseFull());
            io.Fonts.Build();
            mController.RecreateFontDeviceTexture();

            GL.Viewport(0, 0, Width, Height);

            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);

            mScenePlayer = new ScenePlayer(State.SceneId);

            mCurMenu = new MainMenu();

            mCharaDiagMdlTex = new Texture(TextureResource.Load($"{Config.DataPath}/UI/PSP/{Tables.Costumes[mImSelectedCostume].SubCostumes[mImSubSelectedCostume].ThumbnailName}.gim"));

            Camera.FieldOfViewAngle = 45.0f;

            mTime.Start();

            mSelectedLightConfigType = (int)mScenePlayer.LightSettings[mSelectedLightConfig].Type;

            base.OnLoad(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (!Focused) return;

            if (mImSelectedCostume != mImOldSelectedCostume)
            {
                // Clear old texture before updating
                mCharaDiagMdlTex.Dispose();

                mCharaDiagMdlTex = new Texture(TextureResource.Load($"{Config.DataPath}/UI/PSP/{Tables.Costumes[mImSelectedCostume].SubCostumes[mImSubSelectedCostume].ThumbnailName}.gim"));

                mImOldSelectedCostume = mImSelectedCostume;
            }

            KeyboardState state = Keyboard.GetState();

            float speed = 0.1f;

            if (state.IsKeyDown(Key.LControl))
            {
                Console.WriteLine(Camera.Position);
                speed = 0.05f;
            }

            if (state.IsKeyDown(Key.W) || state.IsKeyDown(Key.Up))
                Camera.Position += Camera.Front * speed;
            if (state.IsKeyDown(Key.A) || state.IsKeyDown(Key.Left))
                Camera.Position -= Camera.Right * speed;
            if (state.IsKeyDown(Key.S) || state.IsKeyDown(Key.Down))
                Camera.Position -= Camera.Front * speed;
            if (state.IsKeyDown(Key.D) || state.IsKeyDown(Key.Right))
                Camera.Position += Camera.Right * speed;
            if (state.IsKeyDown(Key.Space))
                Camera.Position += new Vector3(0.0f, 0.7f, 0.0f) * speed;
            if (state.IsKeyDown(Key.LShift))
            {
                if (!state.IsKeyDown(Key.WinLeft))
                    Camera.Position += new Vector3(0.0f, -0.7f, 0.0f) * speed;
            }
            if (state.IsKeyDown(Key.J))
            {
                Camera.Yaw += 1.0f;
            }
            if (state.IsKeyDown(Key.L))
            {
                Camera.Yaw -= 1.0f;
            }
            if (state.IsKeyDown(Key.F1) && !lastState.IsKeyDown(Key.F1))
            {
                /*if (Camera.Mode == CameraMode.FollowTarget)
                    Camera.Mode = CameraMode.LockAtTarget;
                else
                    Camera.Mode = CameraMode.FollowTarget;*/

                mScenePlayer.SwitchState();
            }
            if (state.IsKeyDown(Key.F2) && !lastState.IsKeyDown(Key.F2))
            {
                mScenePlayer.Reset();
            }
            if (state.IsKeyDown(Key.F3) && !lastState.IsKeyDown(Key.F3))
            {
                mDebugOverlayEnabled = mDebugOverlayEnabled == false;
            }

            if(state.IsKeyDown(Key.F10) && !lastState.IsKeyDown(Key.F10))
            {
                AnimationBuilder.PositionKeyframes.Add(new Vector3Keyframe()
                {
                    Frame = mScenePlayer.CurrentFrame,
                    Value = Camera.Position.ToNumerics()
                });

                AnimationBuilder.RotationKeyframes.Add(new Vector3Keyframe()
                {
                    Frame = mScenePlayer.CurrentFrame,
                    Value = new Sn.Vector3(Camera.Pitch, Camera.Yaw, 0.0f)
                });
            }

            if (state.IsKeyDown(Key.F11) && !lastState.IsKeyDown(Key.F11))
            {
                Console.WriteLine("aaa");
                AnimationBuilder.Write("scene001_cam.anim");
            }

            mScenePlayer.Update();
            //mMainMenu.Update();
            //mCurMenu.Update();

            lastState = state;

            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            mController.Update(this, (float)e.Time);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            GL.Enable(EnableCap.DepthTest);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            mScenePlayer.Render((float)e.Time);
            //mMainMenu.Render();
            //mCurMenu.Render();
            
            GL.Disable(EnableCap.DepthTest);

            if (mDebugOverlayEnabled)
            {
                ImGui.PushFont(mFontPtr);

                if (ImGui.Begin($"{mScenePlayer.Id} / {mScenePlayer.SongName}"))
                {
                    ImGui.SliderInt("Frame", ref mScenePlayer.CurrentFrame, 0, mScenePlayer.EndFrame);
                    ImGui.ColorEdit4("Sky color", ref mScenePlayer.ClearColor);
                    ImGui.NewLine();
                    if (ImGui.Button("Play / Pause"))
                        mScenePlayer.SwitchState();
                    ImGui.NewLine();

                    ImGui.Checkbox("Enable motion lerp", ref mScenePlayer.EnableMotionLerp);
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("Don't panic if it freaks out a little when you enable.");

                    ImGui.End();
                }



                /*
                if(ImGui.Begin("Lighting"))
                {
                    if (ImGui.Combo("Light", ref mSelectedLightConfig, mScenePlayer.LightSettings.Select(l => l.Id.ToString()).ToArray(), mScenePlayer.LightSettings.Length))
                    {
                        mSelectedLightConfigType = (int)mScenePlayer.LightSettings[mSelectedLightConfig].Type;
                    }

                    ImGui.ColorEdit3("Color", ref mScenePlayer.LightSettings[mSelectedLightConfig].Color);
                    ImGui.SliderFloat("Intensity", ref mScenePlayer.LightSettings[mSelectedLightConfig].Intensity, 0.0f, 1.0f);

                    if(ImGui.Combo("Type", ref mSelectedLightConfigType, new string[] { "All", "Chara", "Prop", "None" }, 4))
                    {
                        // Update selected light's type
                        mScenePlayer.LightSettings[mSelectedLightConfig].Type = (Structs.LightType)mSelectedLightConfigType;
                    }

                    if(ImGui.Button("Save"))
                    {
                        Structs.SpecialStructWriter.WriteLightSetting(mScenePlayer.LightSettings, $"Resource/Ext/SceneParameter/{mScenePlayer.Id}_light.txt");
                    }
                    ImGui.End();
                }*/

                /*if(ImGui.Begin("Rendering"))
                {
                    ImGui.Checkbox("Enable debug", ref mScenePlayer.DebugRendering);

                    if(mScenePlayer.DebugRendering)
                    {
                        ImGui.Combo("Fragment Mode", ref mScenePlayer.FragmentMode, new string[] { "Normal", "Weights" }, 2);
                    }
                }*/
                /*
                if(ImGui.Begin("Chara"))
                {
                    ImGui.Image((IntPtr)mCharaDiagMdlTex.Id, new Sn.Vector2(mCharaDiagMdlTex.Width, mCharaDiagMdlTex.Height));

                    if(ImGui.Combo("Costume", ref mImSelectedCostume, Tables.Costumes.Select(o => o.Name).ToArray(), Tables.Costumes.Length) || ImGui.Combo("Sub", ref mImSubSelectedCostume, Tables.Costumes[mImSelectedCostume].SubCostumes.Select(o => o.Name).ToArray(), Tables.Costumes[mImSelectedCostume].SubCostumes.Length))
                    {
                        // Reload thumb
                        mCharaDiagMdlTex = new GLTexture(Texture.Load($"{Config.DataPath}/UI/PSP/{Tables.Costumes[mImSelectedCostume].SubCostumes[mImSubSelectedCostume].ThumbnailName}.gim"));
                    }

                    int chrIndex = 1;
                    int i = 0;
                    foreach(SceneObject obj in mScenePlayer.SceneInfo.Objects)
                    {
                        foreach(MotionEntry mot in obj.Motions)
                        {
                            if(mot.Motion.Name == mScenePlayer.Id)
                                chrIndex = i;
                        }
                        i++;
                    }

                    ImGui.Text($"Note: {Tables.Costumes[mImSelectedCostume].Info}");
                    ImGui.Text($"Shop price: ${Tables.Costumes[mImSelectedCostume].SubCostumes[mImSubSelectedCostume].ShopPrice}");
                    ImGui.NewLine();

                    if(ImGui.Button("Load chara"))
                    {
                        // Dispose old object first
                        mScenePlayer.SceneInfo.Objects[chrIndex].Object.Dispose();

                        // Now load the new model
                        string path = Config.DataPath + "/" + Tables.Costumes[mImSelectedCostume].BasePath + "/" + Tables.Costumes[mImSelectedCostume].ObjectFilename;

                        string texPath = Tables.Costumes[mImSelectedCostume].SubCostumes[mImSubSelectedCostume].BasePath;
                        texPath = Path.GetFileName(texPath.Replace('/', Path.DirectorySeparatorChar)).ToLower().StartsWith("tex_var") ? Path.GetFileName(texPath.Replace('/', Path.DirectorySeparatorChar)) : null;

                        mScenePlayer.SceneInfo.Objects[chrIndex].Object = new GLObjectData(ObjectData.Load(path, texPath), GLShader.Default);

                        // Reload physics (Spaghetti)
                        if (mReloadPhysics)
                        {
                            mScenePlayer.SceneInfo.Objects[chrIndex].Motions[1].Motion = Motion.FromFile(Config.DataPath + "/" + "MOTIONS/" + mScenePlayer.Id + "_P" + mScenePlayer.SceneInfo.Objects[chrIndex].Object.PhysicsId.ToString() + ".bmm");
                        }
                    }

                    ImGui.SameLine();
                    ImGui.Checkbox("Reload Phys Motion", ref mReloadPhysics);
                }*/

                mController.Render();
            }

            Context.SwapBuffers();
            base.OnRenderFrame(e);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
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
