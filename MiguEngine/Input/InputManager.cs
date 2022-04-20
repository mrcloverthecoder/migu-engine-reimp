using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using Vector2 = System.Numerics.Vector2;

namespace MiguEngine.Input
{
    public class InputManager
    {
        protected KeyboardState KbdState;
        protected KeyboardState PreviousKbdState;

        protected MouseState MouseState;
        protected MouseState PreviousMouseState;
        protected Vector2 MousePosition;
        protected Vector2 PreviousMousePosition;
        protected float ScrollWheel;
        protected float PreviousScrollWheel;

        public void Update()
        {
            PreviousMouseState = MouseState;
            PreviousKbdState = KbdState;
            PreviousMousePosition = MousePosition;
            PreviousScrollWheel = ScrollWheel;

            KbdState = Keyboard.GetState();
            MouseState = Mouse.GetState();
            MousePosition = new Vector2(MouseState.X, MouseState.Y);
            ScrollWheel = MouseState.WheelPrecise;
        }

        public bool IsKeyDown(Key key) => KbdState.IsKeyDown(key);

        public bool IsKeyUp(Key key) => KbdState.IsKeyUp(key);

        public bool IsKeyTapped(Key key)
        {
            return KbdState.IsKeyDown(key) && !PreviousKbdState.IsKeyDown(key);
        }

        public bool IsMouseDown(MouseButton button) => MouseState.IsButtonDown(button);

        public bool IsMouseUp(MouseButton button) => MouseState.IsButtonUp(button);

        public bool IsMouseTapped(MouseButton button)
        {
            return MouseState.IsButtonDown(button) && !PreviousMouseState.IsButtonDown(button);
        }

        public bool CursorMoved() => MousePosition != PreviousMousePosition;

        public Vector2 GetCursorPosition() => MousePosition;

        public Vector2 GetCursorPositionDelta() => MousePosition - PreviousMousePosition;

        public float GetScrollWheel() => ScrollWheel;

        public float GetScrollWheelDelta() => ScrollWheel - PreviousScrollWheel;
    }
}
