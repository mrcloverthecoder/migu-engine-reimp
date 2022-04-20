using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiguModelViewer.Renderer;
using MiguEngine.Textures;
using MiguLibrary.Textures;
using OpenTK;
using OpenTK.Input;

namespace MiguModelViewer.Menus.Common
{
    public class CircularList
    {
        private string[] mItems;
        private Texture[] mSprites;

        public int SelectedIndex = 0;

        private KeyboardState mOldState;

        public CircularList(string[] items)
        {
            mItems = items;
            mOldState = new KeyboardState();

            List<string> spriteList = new List<string>()
            {
                "HART_1", "HART_2"
            };

            mSprites = new Texture[spriteList.Count];
            for(int i = 0; i < spriteList.Count; i++)
            {
                mSprites[i] = new Texture(TextureResource.Load($"{Config.DataPath}/UI/0000_CIRCULARLIST/PSP/{spriteList[i]}.GIM"));
            }
        }

        public void Update()
        {
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Key.Down) && !mOldState.IsKeyDown(Key.Down))
                SelectedIndex++;
            else if (state.IsKeyDown(Key.Up) && !mOldState.IsKeyDown(Key.Up))
                SelectedIndex--;

            if (SelectedIndex >= mItems.Length)
                SelectedIndex = 0;
            else if (SelectedIndex < 0)
                SelectedIndex = mItems.Length - 1;

            mOldState = state;
        }

        public void Render()
        {
            for(int i = 0; i < mItems.Length; i++)
            {
                float angle = MathHelper.DegreesToRadians(Utils.Lerp(0.0f, 135.0f, 0, mItems.Length - 1, i));

                float y = (mSprites[1].Height * i) - 10;

                SpriteDrawer.Draw(mSprites[1], 124 + (20 * i * angle), y, Vector2.Zero, new Vector2(mSprites[1].Width, mSprites[1].Height));

                SpriteDrawer.Draw(mSprites[0], 124 + mSprites[1].Width + (20 * i * angle), y, Vector2.Zero, new Vector2(mSprites[0].Width, mSprites[0].Height));
            }
        }
    }
}
