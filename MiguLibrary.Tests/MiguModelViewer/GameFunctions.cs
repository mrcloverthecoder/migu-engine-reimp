using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiguLibrary.IO;

namespace MiguModelViewer
{
    public class GameFunctions
    {
        public static void SaveGame(int slot)
        {
            Console.WriteLine("Saving to slot {0}", slot);

            if (!Directory.Exists("save/"))
                Directory.CreateDirectory("save/");

            using(EndianBinaryWriter w = new EndianBinaryWriter(File.Open($"save/{slot}.bin", FileMode.Create), Endianness.LittleEndian))
            {
                w.Write(0x20220217);
                w.Write(State.Points);
                w.Write(State.SelectedGumi);
            }
        }

        public static void LoadGame(int slot)
        {
            Console.WriteLine("Loading save slot {0}", slot);
        }

        public static bool CheckSave(int slot)
        {
            if (!Directory.Exists("save/"))
                return false;

            return true;
        }
    }
}
