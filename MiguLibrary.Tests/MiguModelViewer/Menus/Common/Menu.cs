using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiguEngine;
using MiguEngine.Objects;
using MiguEngine.Textures;
using MiguLibrary.Objects;
using MiguLibrary.Textures;
using MiguModelViewer.Renderer;
using OpenTK.Input;

namespace MiguModelViewer.Menus.Common
{
    public abstract class Menu
    {
        public Camera Camera;
        public Texture[] Sprites;

        public GLObjectData[] Objects;
        public MotionEntry[] Motions;

        public int SelectedIndex = 0;

        public KeyboardState KbdState;
        public KeyboardState OldKbdState;

        public int CurrentFrame = 0;

        private bool mSupressPolling;

        public Menu(bool supressDefaultPolling)
        {
            mSupressPolling = supressDefaultPolling;
            Camera = new Camera();
        }

        public virtual void Load(string[] objects, string[] motions, string uiBasePath, string[] sprites)
        {
            Objects = new GLObjectData[objects.Length];
            Motions = new MotionEntry[motions.Length];
            Sprites = new Texture[sprites.Length];

            for(int i = 0; i < objects.Length; i++)
            {
                Objects[i] = new GLObjectData(ObjectData.Load($"{Config.DataPath}/OBJECTS/{objects[i]}"), Shader.Default);
            }

            for(int i = 0; i < motions.Length; i++)
            {
                Motions[i] = new MotionEntry()
                {
                    Type = MiguLibrary.Scene.MotionType.Object,
                    Motion = MiguLibrary.Motions.Motion.FromFile($"{Config.DataPath}/MOTIONS/{motions[i]}")
                };
            }

            for(int i = 0; i < sprites.Length; i++)
            {
                Sprites[i] = new Texture(TextureResource.Load($"{Config.DataPath}/UI/{uiBasePath}/{sprites[i]}.GIM"));
            }
        }

        public virtual void Update()
        {
            KbdState = Keyboard.GetState();

            if(!mSupressPolling)
            {
                if (KbdState.IsKeyDown(Key.Down) && !OldKbdState.IsKeyDown(Key.Down))
                    SelectedIndex++;
                else if (KbdState.IsKeyDown(Key.Up) && !OldKbdState.IsKeyDown(Key.Up))
                    SelectedIndex--;
            }

            OldKbdState = KbdState;

            CurrentFrame++;
        }

        public abstract void Render();
    }
}
