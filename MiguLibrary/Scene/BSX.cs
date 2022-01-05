using System;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiguLibrary.IO;

namespace MiguLibrary.Scene
{
    public class BSX
    {
        public Vector4 BackgroundColor;
        public FileEntry[] ObjectEntries;
        public FileEntry[] MotionEntries;

        public void Read(EndianBinaryReader reader)
        {
            if (!reader.ReadSignature("B3DSCENE", 8))
                throw new Exception("Not a BSX file!");

            reader.SeekBegin(0x40);

            BackgroundColor = reader.ReadColor(ColorFormat.BGRA).AsVector4();

            reader.SeekBegin(0x150);

            int motionCount = reader.ReadInt32();
            reader.SeekCurrent(0x08);
            int objectCount = reader.ReadInt32();

            List<FileEntry> objs = new List<FileEntry>();
            List<FileEntry> mots = new List<FileEntry>();

            while (motionCount > 0 || objectCount > 0)
            {
                FileEntry entry = new FileEntry();

                entry.Name = reader.ReadString(StringBinaryFormat.FixedLength, 0x40).ToUpper();

                // Trick to gather the actual path as its stored weirdly in the bsx
                reader.SeekCurrent(0x02);
                string pathBase = reader.ReadString(StringBinaryFormat.NullTerminated);
                string filename = reader.ReadString(StringBinaryFormat.NullTerminated);

                entry.Path = (pathBase + Path.GetFileName(filename)).Replace("\\", "/").ToUpper();

                int totalLength = pathBase.Length + filename.Length + 0x04;
                reader.SeekCurrent(0x80 - totalLength);

                entry.Filename = reader.ReadString(StringBinaryFormat.FixedLength, 0x40).ToUpper();

                if (entry.Filename.EndsWith(".BMD"))
                {
                    // Skip unknown data
                    reader.SeekCurrent(0xC8);

                    int animationCutCount = reader.ReadInt32();
                    if (animationCutCount == 0)
                        reader.SeekCurrent(4);

                    entry.Cuts = new SceneAnimationCut[animationCutCount];
                    for(int i = 0; i < animationCutCount; i++)
                    {
                        entry.Cuts[i] = SceneAnimationCut.Read(reader);
                    }

                    objs.Add(entry);
                    objectCount--;
                }
                else if (entry.Filename.EndsWith(".BMM"))
                {
                    // Skip unknown data
                    reader.SeekCurrent(0x58);
                    mots.Add(entry);
                    motionCount--;
                }
            }

            ObjectEntries = objs.ToArray();
            MotionEntries = mots.ToArray();

            Console.WriteLine($"BSX POS: {reader.Position}");
        }

        public static BSX FromFile(string path)
        {
            BSX bsx = new BSX();

            using(var reader = new EndianBinaryReader(File.Open(path, FileMode.Open), Encoding.GetEncoding(932), Endianness.LittleEndian))
            {
                bsx.Read(reader);
            }

            return bsx;
        }
    }

    public struct FileEntry
    {
        public string Name;
        public string Path;
        public string Filename;
        public SceneAnimationCut[] Cuts;
    }
}
