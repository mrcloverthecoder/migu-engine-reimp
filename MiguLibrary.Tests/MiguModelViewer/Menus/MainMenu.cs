using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using MiguEngine;
using MiguEngine.Objects;
using MiguLibrary.Objects;
using MiguLibrary.Motions;
using MiguLibrary.Scene;
using MiguModelViewer.Renderer;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using PuyoTools.Core.Textures.Gim;

namespace MiguModelViewer.Menus
{
    public class MainMenu : Common.Menu
    {
        private Vector3 mCameraPosition = new Vector3(-1.0f, 0.9f, 2.4f);

        private Common.CircularList List;

        public MainMenu() : base(true)
        {
            List = new Common.CircularList(new string[5]
            {
                "Gumi Room", "My Data", "Rhythm Game", "Online", "Save"
            });
            Load(
                new string[1] { "OBJ_MIGU_A11/OBJ_MIG.BMD" },
                new string[2] { "FTEST01.BMM", "SYS000_TA_P1.BMM" },
                "0200_GAMEMENU/PSP",
                new string[10]
                    {
                        "menu_1", "menu_2",
                        "select_1", "select_2", "select_3", "select_4",
                        "o_line1", "o_line2",
                        "ps_b_maru", "ps_b_updown"
                    }
                );
        }

        public override void Update()
        {
            GL.ClearColor(0.0f, 1.0f, 0.0f, 0.0f);
            Camera.Position = mCameraPosition;

            ref GLObjectData gumi = ref Objects[0];

            Matrix4[] transforms = MotionPlayer.GetTransformsLerp(gumi.Bones, ref Motions[0], gumi.BoneInfluenceNames, CurrentFrame, null, true);

            transforms = MotionPlayer.GetTransformsLerp(gumi.Bones, ref Motions[1], gumi.BoneInfluenceNames, CurrentFrame, transforms, true);

            gumi.Shader.Uniform("uBoneTransforms", transforms);

            List.Update();

            KbdState = Keyboard.GetState();

            if(KbdState.IsKeyDown(Key.Enter) && !OldKbdState.IsKeyDown(Key.Enter))
            {
                switch(List.SelectedIndex)
                {
                    case 4:
                        GameFunctions.SaveGame(1);
                        break;
                    default:
                        break;
                }
            }

            OldKbdState = KbdState;

            base.Update();
        }

        public override void Render()
        {
            GL.Disable(EnableCap.DepthTest);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Menu label
            SpriteDrawer.Draw(Sprites[0], 352.0f, 64.0f, Vector2.Zero, new Vector2(128, 64));
            SpriteDrawer.Draw(Sprites[1], 360.0f, 32.0f, Vector2.Zero, new Vector2(64, 32));

            SpriteDrawer.Draw(Sprites[2], 128.0f, 208.0f, Vector2.Zero, new Vector2(128, 64));
            SpriteDrawer.Draw(Sprites[3], 128.0f, 80.0f, Vector2.Zero, new Vector2(64, 128));
            SpriteDrawer.Draw(Sprites[4], 0.0f, 55.0f, Vector2.Zero, new Vector2(128, 128));
            SpriteDrawer.Draw(Sprites[5], 112.0f, 183.0f, Vector2.Zero, new Vector2(16, 16));

            List.Render();

            SpriteDrawer.Draw(Sprites[6], 401.0f, 6.0f, Vector2.Zero, new Vector2(79, 32));
            SpriteDrawer.Draw(Sprites[7], 385.0f, 6.0f, Vector2.Zero, new Vector2(16, 32), -1.0f);

            SpriteDrawer.Draw(Sprites[8], 382.0f, 3.0f, Vector2.Zero, new Vector2(32, 32));
            SpriteDrawer.Draw(Sprites[9], 434.0f, 10.0f, Vector2.Zero, new Vector2(16, 32));

            // Now do 3D rendering
            GL.Enable(EnableCap.DepthTest);
            
            Objects[0].Draw(Camera);
        }
    }
}
