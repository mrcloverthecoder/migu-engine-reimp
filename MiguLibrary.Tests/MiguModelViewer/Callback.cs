using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace MiguModelViewer
{
    public class Callback
    {
        public static void DebugCallback(DebugSource source,
                                  DebugType type,
                                  int id,
                                  DebugSeverity severity,
                                  int length,
                                  IntPtr message,
                                  IntPtr userParam)
        {
            string messageString = Marshal.PtrToStringAnsi(message, length);

            Console.WriteLine($"{severity} {type} | {messageString}");

            if (type == DebugType.DebugTypeError)
            {
                //throw new Exception(messageString);
                //Console.WriteLine()
            }
        }
    }
}
