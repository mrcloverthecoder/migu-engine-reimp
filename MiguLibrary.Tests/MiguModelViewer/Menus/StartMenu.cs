using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiguModelViewer.Renderer;
using OpenTK;
using OpenTK.Input;

namespace MiguModelViewer.Menus
{
    public class StartMenu : Common.Menu
    {
        public StartMenu() : base(false)
        {
            Load(
                new string[0],
                new string[0],
                "0100_STARTMENU/PSP/",
                new string[8]
                {
                    "load_a1", "load_a2", "load_b1", "load_b2",
                    "new_a1", "new_a2", "new_b1",  "new_b2"
                }
                );
        }

        public override void Update()
        {
            KbdState = Keyboard.GetState();

            if(KbdState.IsKeyDown(Key.Enter) && !OldKbdState.IsKeyDown(Key.Enter))
            {
                if (SelectedIndex == 1)
                    GameFunctions.LoadGame(0);
            }

            if (SelectedIndex > 1)
                SelectedIndex = 0;
            else if (SelectedIndex < 0)
                SelectedIndex = 1;

            base.Update();
        }

        public override void Render()
        {

            SpriteDrawer.Draw(SelectedIndex == 0 ? Sprites[7] : Sprites[5], 300, 0, Vector2.Zero, new Vector2(32, 32));
            SpriteDrawer.Draw(SelectedIndex == 0 ? Sprites[6] : Sprites[4], 332, 0, Vector2.Zero, new Vector2(128, 32));

            SpriteDrawer.Draw(SelectedIndex == 1 ? Sprites[3] : Sprites[1], 300, 32, Vector2.Zero, new Vector2(32, 32));
            SpriteDrawer.Draw(SelectedIndex == 1 ? Sprites[2] : Sprites[0], 332, 32, Vector2.Zero, new Vector2(128, 32));
        }
    }
}
